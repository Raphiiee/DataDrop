using System;
using System.Dynamic;
using System.Net;
using System.Reflection;

namespace DataDrop.BusinessLayer
{
    public class Server
    {
        public int Port { get; set; } = 13013;
        public IPAddress LocalAddress { get; set; } = IPAddress.Parse("127.0.0.1");
        public bool IsRunning { get; set; }

    }
}
