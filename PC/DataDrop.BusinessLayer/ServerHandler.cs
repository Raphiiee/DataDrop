using System;
using System.Dynamic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using DataDrop.Models;

namespace DataDrop.BusinessLayer
{
    public class ServerHandler
    {
        private Server _server { get; set; }
        public bool Status { get; private set; }
        private Task _serverTask;

        public void Start(string filePath, string  ipAdress,int port)
        {
            _server = new Server(filePath, ipAdress, port);
            Status = true;
            _server.StartServerSocket();
            
        }

        public void Stop()
        {
            _server.StopServerListener();
            Status = false;
        }

    }
}
