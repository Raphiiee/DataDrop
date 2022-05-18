using Windows.Devices.WiFiDirect;

namespace DataDrop.BusinessLayer
{
    public class WifiDirectManager
    {
        private WiFiDirectAdvertisementPublisher _publisher;
        private WiFiDirectAdvertisement _advertisement;
        private WiFiDirectLegacySettings _legacySettings;

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

            _legacySettings = _advertisement.LegacySettings;
            _legacySettings.IsEnabled = false;
            _legacySettings.Ssid = SsidName;

            var passwordCredentials = _legacySettings.Passphrase;
            passwordCredentials.Password = SsidPassword;
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