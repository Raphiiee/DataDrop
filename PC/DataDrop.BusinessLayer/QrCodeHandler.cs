using System.Windows.Media.Imaging;
using DataDrop.Models;

namespace DataDrop.BusinessLayer
{
    public class QrCodeHandler
    {
        public BitmapImage QrCodeImage { get; set; }

        private QrCodeModel _qrCode { get; set; }

        public void CreateQrCodeImage(string ipAddress, int port, bool isWifiDirectOn, string ssidName, string ssidPassword, string hostIpAddress)
        {
            _qrCode = new QrCodeModel();
            _qrCode.CreateQrCodeImage(ipAddress, port, isWifiDirectOn, ssidName, ssidPassword, hostIpAddress);
            QrCodeImage = _qrCode.QrCodeImage;
        }
    }
}