using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DataDrop.Models.Enum;

namespace DataDrop.Models
{
    public  class Server
    {
        public bool IsRunning { get; set; } = false;
        private TcpListener _serverListener;

        public  Server(string ip = "127.0.0.1", int port = 49153)
        {
            IPAddress localAddress = IPAddress.Parse(ip);
            _serverListener = new TcpListener(localAddress, port);
        }
        
        public async void StartServerListener(string filePath)
        {
            try
            {
                IsRunning = true;
                _serverListener.Start();
                List<Task> allTasks = new List<Task>();
                while (IsRunning)
                {
                    TcpClient client = await _serverListener.AcceptTcpClientAsync();
                    allTasks.Add(Task.Run(() => HandleAcceptedClient(client, filePath)));
                    await Task.WhenAll(allTasks);
                }
                _serverListener.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                IsRunning = false;
                _serverListener.Stop();
                throw;
            }
        }

        private void HandleAcceptedClient(TcpClient client, string filePath)
        {
            var stream = client.GetStream();
            // reading data buffer
            string clientDataBuffer;
            Byte[] buffer = new byte[9_000_000]; // 9 MB buffer
            int bytesRead;
            TcpHandler handler = new TcpHandler();

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    clientDataBuffer = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    clientDataBuffer = clientDataBuffer.ToUpper();
                    Console.WriteLine(clientDataBuffer);

                    handler.ProcessData(clientDataBuffer);

                    // Check which path & method was sent
                    if (handler.GetMethod() == AllowedMethods.GET && handler.GetPath() == AllowedPaths.SendData)
                    {
                        handler.SendData(filePath);
                    }
                    else if (handler.GetMethod() == AllowedMethods.POST && handler.GetPath() == AllowedPaths.ReceiveData)
                    {
                        handler.ReceiveData(filePath);
                    }
                    else
                    {
                        handler.Error();
                    }

                    // Create header for response
                    handler.CreateResponseHeader();
                    // make header ready for sending back
                    Byte[] reply = Encoding.ASCII.GetBytes(handler.GetResponseHeader());
                    stream.Write(reply, 0, reply.Length);
                    Console.WriteLine($"Sending Back: {handler.GetResponseHeader()}");

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
