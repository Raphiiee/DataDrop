using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using DataDrop.BusinessLayer;
using DataDrop.Core;

namespace DataDrop.MVVM.ViewModel
{
    public class SendViewModel : BaseViewModel
    {
        private ICommand _toggleServerCommand;
        private ServerHandler ServerHandler = new();
        private string _serverStatus = "Status: Offline";
        private string _serverIp = "IP: 127.0.0.1";
        private string _serverPort = "Port: 49153";
        private string _filePath;
        public string ServerStatus 
        {
            get => _serverStatus;
            set
            {
                _serverStatus = value;
                RaisePropertyChangedEvent();
            }
        }
        public string ServerIp 
        {
            get => _serverIp;
            set
            {
                _serverIp = value;
                RaisePropertyChangedEvent();
            }
        }
        public string ServerPort 
        {
            get => _serverPort;
            set
            {
                _serverPort = value;
                RaisePropertyChangedEvent();
            }
        }
        public string FilePath 
        {
            get => _filePath;
            set
            {
                _filePath = value.Replace("\"", string.Empty);
                RaisePropertyChangedEvent(nameof(FilePath));
            }
        }

        public ICommand ToggleServerCommand => _toggleServerCommand ??= new RelayCommand(ToggleServer);
        private void ToggleServer(object commandParameter)
        {
            var aaaa = FilePath;
            if (ServerHandler.Status)
            {
                ServerHandler.Stop();
                ServerStatus = "Status: Offline";
            }
            else
            {
                ServerStatus = "Status: Online";
                ServerHandler.Start(FilePath, "127.0.0.1", 49153);
            }
        }
    }
}