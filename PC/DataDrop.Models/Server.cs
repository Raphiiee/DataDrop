using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using DataDrop.Models.Enum;

namespace DataDrop.Models
{
    public  class Server
    {
        public bool IsRunning { get; set; } = false;
        private static byte[] _buffer = new byte[10_000];
        private static Socket _socket { get; set; }
        private static string _filepath { get; set; }
        private string _ip { get; set; }
        private int _port { get; set; }

        public  Server(string filepath, string ip = "127.0.0.1", int port = 49153)
        {
            _ip = ip;
            _port = port;
            _filepath = filepath;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void StopServerListener()
        {
            // Function to stop here
            _socket.Close();
        }

        public void StartServerSocket()
        {
            try
            {
                IPAddress localIpAddress = IPAddress.Parse(_ip);
                // Binding socket to ip & port
                _socket.Bind(new IPEndPoint(localIpAddress, _port));
                _socket.Listen(5);
                _socket.ReceiveTimeout = 100;
                _socket.SendTimeout = 100;
                // Configure max buffer size 8kb
                _socket.ReceiveBufferSize = 8_192;
                _socket.SendBufferSize = 8_192; 
                IsRunning = true;
                _socket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                IsRunning = false;
                throw;
            }
        }

        private static void AcceptCallback(IAsyncResult asyncResult)
        {
            Socket clientSocket = _socket.EndAccept(asyncResult);
            if (clientSocket.Connected)
            {
                clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, HandleAcceptedClient, clientSocket);
                _socket.BeginAccept(AcceptCallback, null);
            }
        }

        private static void HandleAcceptedClient(IAsyncResult asyncResult)
        {
            Socket clientSocket = (Socket)asyncResult.AsyncState;
            TcpHandler handler = new TcpHandler(_filepath, _buffer.Length);
            bool isSendingFileData = false;

            if (clientSocket == null)
            {
                return;
            }

            try
            {
                // Length of received message
                int lengthReceived = clientSocket.EndReceive(asyncResult);
                byte[] databuffer = new byte[lengthReceived];
                Array.Copy(_buffer, databuffer, lengthReceived);

                string rawRequest = Encoding.ASCII.GetString(databuffer).ToUpper();

                handler.ProcessData(rawRequest);

                // Check which path & method was sent
                if (handler.GetMethod() == AllowedMethods.GET && handler.GetPath() == AllowedPaths.SendData)
                {
                    
                    isSendingFileData = true;
                }
                else if (handler.GetMethod() == AllowedMethods.GET && handler.GetPath() == AllowedPaths.DataInformation)
                {
                    handler.DataInformationJSON();
                }
                else if (handler.GetMethod() == AllowedMethods.GET && handler.GetPath() == AllowedPaths.ClientFinsihed)
                {
                    handler.ClientFinished();
                }
                else
                {
                    handler.Error();
                }

                // Create header for response
                handler.CreateResponseHeader();
                // make header ready for sending back
                byte[] replyData = new byte[_buffer.Length];
                if (isSendingFileData)
                {
                    replyData = handler.SendData();
                }
                else
                {
                    replyData = Encoding.ASCII.GetBytes(handler.GetResponseHeader());
                }
               
                clientSocket.BeginSend(replyData, 0, replyData.Length, SocketFlags.None, SendCallback, clientSocket);
                Console.WriteLine($"Sending Back: {handler.GetResponseHeader()}");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void SendCallback(IAsyncResult asyncResult)
        {
            Socket clientSocket = (Socket) asyncResult.AsyncState;
            clientSocket?.EndSend(asyncResult);
            clientSocket?.Shutdown(SocketShutdown.Both);
            clientSocket?.Close();
        }
    }
}
