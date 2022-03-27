using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DataDrop.Models.Struct;
using Newtonsoft.Json;
using QRCoder;

namespace DataDrop.BusinessLayer
{
    public class QrCodeHandler
    {
        public BitmapImage QrCodeImage { get; set; }

        public void CreateQrCodeImage(string ipAddress, int port)
        {
            HostInformation hostInformation = new HostInformation();
            hostInformation.IpAddress = ipAddress;
            hostInformation.Port = port;
            hostInformation.IsSsidOn = false;
            hostInformation.SsidName = "";
            hostInformation.SsidPassword = "";
            hostInformation.SsidNetworkIpAddress = "";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(JsonConvert.SerializeObject(hostInformation), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            using (MemoryStream memory = new MemoryStream())
            {
                qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                QrCodeImage = bitmapimage;
            }
            
        }
    }
}