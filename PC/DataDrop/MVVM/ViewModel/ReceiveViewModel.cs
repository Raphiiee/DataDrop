using System.Windows.Input;
using DataDrop.BusinessLayer;
using DataDrop.Core;

namespace DataDrop.MVVM.ViewModel
{
    public class ReceiveViewModel : BaseViewModel
    {
        private ICommand _toggleClientCommand;
        private ClientHandler _client = new();
        private string _maxFileSize;
        private string _currentFileSize;
        private string _filepath;
        private string _clientStatus = "Status: Nicht Verbunden";
        private string _clientIp = "127.0.0.1";
        private int _clientPort = 49153;
        public int Progress { get; set; }

        public string MaxFileSize
        {
            get => _maxFileSize;
            set
            {
                _maxFileSize = value;
                RaisePropertyChangedEvent();
            }
        }

        public string CurrentFileSize
        {
            get => _currentFileSize;
            set
            {
                _currentFileSize = value;
                RaisePropertyChangedEvent();
            }
        }

        public string Filepath
        {
            get => _filepath;
            set
            {
                _filepath = value;
                RaisePropertyChangedEvent();
            }
        }

        public string ClientStatus
        {
            get => _clientStatus;
            set
            {
                _clientStatus = value;
                RaisePropertyChangedEvent();
            }
        }

        public string ClientIp
        {
            get => _clientIp;
            set
            {
                _clientIp = value;
                RaisePropertyChangedEvent();
            }
        }

        public int ClientPort
        {
            get => _clientPort;
            set
            {
                _clientPort = value;
                RaisePropertyChangedEvent();
            }

        }

        public ICommand ToggleClientCommand => _toggleClientCommand ??= new RelayCommand(ToggleClient);

        private void ToggleClient(object obj)
        {
            _client.Start(Filepath, ClientIp, ClientPort);
            
        }
    }
}