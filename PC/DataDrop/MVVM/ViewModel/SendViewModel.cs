using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DataDrop.BusinessLayer;
using DataDrop.Core;

namespace DataDrop.MVVM.ViewModel
{
    public class SendViewModel : BaseViewModel
    {
        private ICommand _toggleServerCommand;
        private ServerHandler ServerHandler = new();
        private QrCodeHandler QrCodeHandler = new();
        private WifiDirectManager wifiDirect = new("DataDrop", "12345678");
        private string _serverStatus = "Status: Offline";
        private string _serverIp = "IP: 127.0.0.1";
        private string _serverPort = "Port: 49153";
        private string _filePath;
        private BitmapImage _qrCodeImage = new BitmapImage(new Uri(@"C:\Users\Raphael\Documents\DataDrop\PC\DataDrop\Images\logo.png"));
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

        public BitmapImage QrCodeImage
        {
            //get => _qrCodeImage;
            get
            {
                QrCodeHandler.CreateQrCodeImage(_serverIp.Substring(_serverIp.IndexOf(' ')).Trim(), Int32.Parse(_serverPort.Substring(_serverPort.IndexOf(' ')).Trim()), wifiDirect.IsWifiDirectOn, wifiDirect.SsidName, wifiDirect.SsidPassword, "198.168.137.1");
                _qrCodeImage = QrCodeHandler.QrCodeImage;
                return _qrCodeImage;
            }
            set
            {
                _qrCodeImage = value;
                RaisePropertyChangedEvent();
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
                wifiDirect.Stop();
            }
            else
            {
                ServerStatus = "Status: Online";
                ServerHandler.Start(FilePath, "127.0.0.1", 49153);
                wifiDirect.Start();
            }
        }
    }
}