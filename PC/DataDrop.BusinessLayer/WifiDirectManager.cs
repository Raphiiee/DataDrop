using Windows.Devices.WiFiDirect;

namespace DataDrop.BusinessLayer
{
    public class WifiDirectManager
    {
        private WiFiDirectAdvertisementPublisher _publisher;
        private WiFiDirectAdvertisement _advertisement;

        public string SsidName { get; set; }
        public string SsidPassword { get; set; }
        public bool IsWifiDirectOn { get; set; }

        public WifiDirectManager(string ssidName, string ssidPassword)
        {
            SsidName = ssidName;
            SsidPassword = ssidPassword;
        }

        public void Start()
        {
            _publisher = new WiFiDirectAdvertisementPublisher();

            _advertisement = _publisher.Advertisement;
            _advertisement.IsAutonomousGroupOwnerEnabled = true;

            IsWifiDirectOn = true;

            _publisher.Start();
        }

        public void Stop()
        {
            IsWifiDirectOn = false;
            _publisher.Stop();
        }
    }
}