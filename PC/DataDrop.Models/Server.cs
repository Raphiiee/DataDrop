using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private static TcpHandler _handler { get; set; }

        public  Server(string filepath, string ip = "127.0.0.1", int port = 49153)
        {
            _ip = ip;
            _port = port;
            _filepath = filepath;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _handler = new TcpHandler(_filepath, _buffer.Length);
            _handler.IsDebugSession = false;
        }

        public void StopServerListener()
        {
            _socket.Close();
        }

        public void StartServerSocket()
        {
            try
            {
                Console.WriteLine("Setting up Server ...");
                _socket.Bind(new IPEndPoint(IPAddress.Any, 49153));
                _socket.Listen(5);
                _socket.ReceiveBufferSize = 10_000;
                _socket.SendBufferSize = 10_000; 
                _socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
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
            Socket socket = _socket.EndAccept(asyncResult);

            Console.WriteLine("Client connected");
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(HandleAcceptedClient), socket);
            _socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void HandleAcceptedClient(IAsyncResult asyncResult)
        {
            Socket clientSocket = (Socket)asyncResult.AsyncState;
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

                string rawRequest = Encoding.UTF8.GetString(databuffer);

                _handler.ProcessData(rawRequest);

                // Check which path & method was sent
                if (_handler.GetPath() == AllowedPaths.SendData)
                {
                    
                    isSendingFileData = true;
                }
                else if (_handler.GetPath() == AllowedPaths.DataInformation)
                {
                    _handler.DataInformationJSON();
                }
                else if (_handler.GetPath() == AllowedPaths.HashInformation)
                {
                    _handler.HashInformation();
                }
                else
                {
                    _handler.Error();
                }

                // Create header for response
                _handler.CreateResponseHeader();
                // make header ready for sending back
                byte[] replyData = new byte[_buffer.Length];
                var aaaaaaaa = _handler.GetResponseHeader();
                replyData = Encoding.UTF8.GetBytes(_handler.GetResponseHeader());
                if (isSendingFileData)
                {
                    replyData = _handler.SendData();
                }

                clientSocket.BeginSend(replyData, 0, replyData.Length, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);
                clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(HandleAcceptedClient), clientSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        private static void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket clientSocket = (Socket)asyncResult.AsyncState;
                clientSocket?.EndSend(asyncResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
