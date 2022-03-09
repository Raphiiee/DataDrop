using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DataDrop.Models.Enum;
using Newtonsoft.Json;

namespace DataDrop.Models
{
    public class Client
    {
        public bool IsConnected { get; set; } = false;
        private static byte[] _buffer = new byte[10_000];
        private static Socket _socket { get; set; }
        private static string _filepath { get; set; }
        private List<byte[]> _fileBytesList { get; set; } = new List<byte[]>();
        private FileInformation _fileInformation { get; set; }
        private string _ip { get; set; }
        private int _port { get; set; }
        public int Progress { get; set; }

        public  Client(string filepath, string ip = "127.0.0.1", int port = 49153)
        {
            _ip = ip;
            _port = port;
            _filepath = filepath;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void StartClient()
        {
            _socket.Connect(IPAddress.Loopback, _port);
            GetFileInformation();
            GetFileData();
            WriteToFilesystem();
        }

        private void GetFileInformation()
        {
            string fileInformationJson = Encoding.ASCII.GetString(GetDataFromServer(AllowedPaths.DataInformation));
            Console.WriteLine(fileInformationJson);

            _fileInformation = JsonConvert.DeserializeObject<FileInformation>(fileInformationJson);
        }

        private void GetFileData()
        {
            for (int i = 0; i <= _fileInformation?.SequenzeCount; i++)
            {
                Console.WriteLine($"Download {i}");
                _fileBytesList.Add(GetDataFromServer(AllowedPaths.SendData, $"{i}"));
            }
        }

        private void WriteToFilesystem()
        {
            var filestream = new FileStream($@"{_filepath}\{_fileInformation.Filename}", FileMode.CreateNew);
            
            foreach (var fileByte in _fileBytesList)
            {
                filestream.Write(fileByte);
            }

            filestream.Close();
        }

        private static byte[] GetDataFromServer(AllowedPaths paths, string resource = "")
        {
            var data = new byte[10_000];
            var dataB = Encoding.ASCII.GetBytes($"{paths}/{resource}");
            Console.WriteLine($"{paths}/{resource}");
            _socket.Send(Encoding.ASCII.GetBytes($"{paths}/{resource}"));

            byte[] receiveBuffer = new byte[10_000];
            int receive = _socket.Receive(receiveBuffer);
            data = new byte[receive];
            Array.Copy(receiveBuffer, data, receive);

            return data;
        }

        public void StopClient()
        {
            
        }
    }
}