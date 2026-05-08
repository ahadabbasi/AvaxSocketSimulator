using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvaxSocketSimulator.WindowsApp.Models.ViewModels
{
    public sealed class MainWindowsVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            if (!string.IsNullOrWhiteSpace(property))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        public MainWindowsVm()
        {
            IsProcessing = false;
        }

        private string _dataType;

        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; OnPropertyChanged(nameof(DataType)); }
        }


        private string _webServerAddress;

        public string WebServerAddress
        {
            get { return _webServerAddress; }
            set { _webServerAddress = value; OnPropertyChanged(nameof(WebServerAddress)); }
        }

        private string _delayTime;

        public string DelayTime
        {
            get { return _delayTime; }
            set { _delayTime = value; OnPropertyChanged(nameof(DelayTime)); }
        }

        private bool _isProcessing;

        public bool IsProcessing
        {
            get { return _isProcessing; }
            set { _isProcessing = value; OnPropertyChanged(nameof(IsProcessing)); OnPropertyChanged(nameof(ProcessStatus)); }
        }

        public bool ProcessStatus => !IsProcessing;

        public bool IsValid() => 
            !string.IsNullOrEmpty(DataType) &&
            !string.IsNullOrEmpty(WebServerAddress) &&
            !string.IsNullOrEmpty(DelayTime) &&
            int.TryParse(DelayTime, out _);
    }
}
