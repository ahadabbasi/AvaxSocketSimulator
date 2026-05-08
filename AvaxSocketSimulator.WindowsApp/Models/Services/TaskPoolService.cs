using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvaxSocketSimulator.WindowsApp.Models.Services
{
    public class TaskPoolService : IDisposable
    {
        private const int MaxConcurrentTasks = 4;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(MaxConcurrentTasks);
        private readonly ConcurrentDictionary<string, BackgroundTask> _tasks = new ConcurrentDictionary<string, BackgroundTask>();
        private readonly ConcurrentDictionary<string, ITaskHandler> _taskHandlers = new ConcurrentDictionary<string, ITaskHandler>();
        private readonly CancellationTokenSource _appShutdownToken = new CancellationTokenSource();
        
        public void RegisterTaskHandler(ITaskHandler handler)
        {
            _taskHandlers[handler.TaskType] = handler;
        }

        public Task<string> QueueTaskAsync(string taskType, string taskName, object parameters = null)
        {
            if (!_taskHandlers.ContainsKey(taskType))
                throw new InvalidOperationException($"No handler registered for task type: {taskType}");

            var task = new BackgroundTask
            {
                Name = taskName,
                Type = taskType,
                Status = TaskStatus.Pending,
                Parameters = parameters,
                CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_appShutdownToken.Token)
            };

            _tasks[task.Id] = task;

            // Start task execution in background without waiting
            _ = Task.Run(() => ExecuteTaskAsync(task));

            return Task.FromResult(task.Id);
        }

        private async Task ExecuteTaskAsync(BackgroundTask task)
        {
            await _semaphore.WaitAsync();

            try
            {
                task.Status = TaskStatus.Running;
                task.StartedAt = DateTime.UtcNow;
                task.Progress = 0;
                task.ProgressMessage = "Starting...";

                if (_taskHandlers.TryGetValue(task.Type, out var handler))
                {
                    var result = await handler.ExecuteAsync(task, task.CancellationTokenSource.Token);
                    task.Result = result;

                    if (result.Success)
                    {
                        task.Status = TaskStatus.Completed;
                        task.Progress = 100;
                        task.ProgressMessage = "Completed successfully";
                    }
                    else
                    {
                        task.Status = TaskStatus.Failed;
                        task.ErrorMessage = result.Message;
                        task.ProgressMessage = "Failed";
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Handler not found for task type: {task.Type}");
                }
            }
            catch (OperationCanceledException)
            {
                task.Status = TaskStatus.Cancelled;
                task.ProgressMessage = "Cancelled";
            }
            catch (Exception ex)
            {
                task.Status = TaskStatus.Failed;
                task.ErrorMessage = ex.Message;
                task.ProgressMessage = "Failed with error";
            }
            finally
            {
                task.CompletedAt = DateTime.UtcNow;
                _semaphore.Release();
                task.CancellationTokenSource?.Dispose();
            }
        }

        public Task<bool> CancelTaskAsync(string taskId)
        {
            bool result = false;
            if (_tasks.TryGetValue(taskId, out var task) &&
                (task.Status == TaskStatus.Pending || task.Status == TaskStatus.Running))
            {
                task.CancellationTokenSource?.Cancel();
                task.Status = TaskStatus.Cancelled;
                task.ProgressMessage = "Cancellation requested";
                result = true;
            }
            return Task.FromResult(result);
        }

        public BackgroundTask GetTaskStatus(string taskId)
        {
            _tasks.TryGetValue(taskId, out var task);
            return task;
        }

        public List<BackgroundTask> GetAllTasks()
        {
            return _tasks.Values.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public Task<int> GetWaitingTaskCountAsync()
        {
            var waitingCount = _tasks.Values.Count(t => t.Status == TaskStatus.Pending);
            var currentRunning = MaxConcurrentTasks - _semaphore.CurrentCount;

            return Task.FromResult(Math.Max(0, waitingCount - currentRunning));
        }

        public async Task ShutdownAsync()
        {
            _appShutdownToken.Cancel();

            var timeout = TimeSpan.FromSeconds(30);
            var runningTasks = _tasks.Values.Where(t => t.Status == TaskStatus.Running).ToList();

            if (runningTasks.Any())
            {
                await Task.WhenAny(
                    Task.WhenAll(runningTasks.Select(t =>
                        Task.Delay(Timeout.Infinite, t.CancellationTokenSource.Token))),
                    Task.Delay(timeout)
                );
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
            _appShutdownToken?.Dispose();

            foreach (var task in _tasks.Values)
            {
                task.CancellationTokenSource?.Dispose();
            }
        }
    }

    public enum TaskStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    public class TaskResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public Exception Exception { get; set; }
    }

    public class BackgroundTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Type { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public double Progress { get; set; }
        public string ProgressMessage { get; set; }
        public object Parameters { get; set; }
        public TaskResult Result { get; set; }
    }

    public interface ITaskHandler
    {
        string TaskType { get; }
        Task<TaskResult> ExecuteAsync(BackgroundTask task, CancellationToken cancellationToken);
    }

}