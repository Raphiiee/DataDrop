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

        public void Start(string filePath = @"G:\Meine Ablage\Studium\BIF6\BA\DataDrop\PC\DataDrop\Images\logo.png", int port = 49153, string  ipAdress = "127.0.0.1")
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
