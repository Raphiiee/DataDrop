using System;
using System.Dynamic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using DataDrop.Models;

namespace DataDrop.BusinessLayer
{
    public class ServerHandler
    {
        private Server _server { get; set; }
        public bool Status { get; private set; }

        public void Start(string filePath, int port = 49153, string  ipAdress = "127.0.0.1")
        {
            _server = new Server(ipAdress, port);
            Status = true;
            _server.StartServerListener(filePath);
            
        }

        public void Stop()
        {
            _server.IsRunning = false;
            Status = false;
        }

    }
}
