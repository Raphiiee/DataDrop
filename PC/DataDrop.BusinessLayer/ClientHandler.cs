using System.Threading.Tasks;
using DataDrop.Models;

namespace DataDrop.BusinessLayer
{
    public class ClientHandler
    {        
        private Client _client { get; set; }
        public bool Status { get; private set; }
        public int Progress { get; private set; }

        public void Start(string filePath, string  ipAdress,int port)
        {
            _client = new Client(filePath, ipAdress, port);
            _client.StartClient();
            Status = _client.IsConnected;
            Progress = _client.Progress;
        }

        public void Stop()
        {
            _client.StopClient();
            Status = false;
        }
    }
}