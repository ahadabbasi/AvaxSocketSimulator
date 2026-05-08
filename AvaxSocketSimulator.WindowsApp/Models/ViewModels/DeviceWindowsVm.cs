using AvaxSocketSimulator.WindowsApp.Models.Persistence;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AvaxSocketSimulator.WindowsApp.Models.ViewModels
{
    public class DeviceWindowsVm : INotifyPropertyChanged
    {
        public DeviceWindowsVm()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                Options =
                    context.Packets
                        .Select(item => item.Type)
                        .Distinct().ToList()
                        .Select((value, key) =>
                            new KeyValuePair<int, string>(key + 1, value)
                        );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
        {
            if (!string.IsNullOrWhiteSpace(property)) 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string property = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(property);
            return true;
        }

        private int _selected;

        public int Selected
        {
            get => _selected;
            set => SetField(ref _selected, value);
        }

        public IEnumerable<KeyValuePair<int, string>> Options { get; }

        private string _webServerAddress;

        public string WebServerAddress
        {
            get => _webServerAddress;
            set => SetField(ref _webServerAddress, value);
        }

        private bool _isProcessing;

        public bool IsProcessing
        {
            get => _isProcessing;
            set { 
                if(SetField(ref _isProcessing, value)) 
                    OnPropertyChanged(nameof(ProcessStatus));
            }
        }

        public bool ProcessStatus => !IsProcessing;

        private string _delayTime;

        public string DelayTime
        {
            get => _delayTime;
            set => SetField(ref _delayTime, value);
        }

        public bool IsValid() =>
            Options.Select(item => item.Key).Contains(Selected) &&
            !string.IsNullOrEmpty(WebServerAddress) &&
            !string.IsNullOrEmpty(DelayTime) &&
            int.TryParse(DelayTime, out _);
    }
}