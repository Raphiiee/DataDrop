using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataDrop.Models;
using DataDrop.Models.Enum;
using Newtonsoft.Json;

namespace DataDrop.CliDownloadClient
{
    internal class Program
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start Connting to Server");
            List<byte[]> fileBytesList = new List<byte[]>();

            _clientSocket.Connect(IPAddress.Loopback, 49153);

            Console.WriteLine("Connected; JSON DECODER!");

            string json = Encoding.UTF8.GetString(GetDataFromServer(AllowedPaths.DataInformation));
            Console.WriteLine(json);

            json = json.Substring(json.IndexOf("{", StringComparison.Ordinal));
            FileInformation fileInformation = JsonConvert.DeserializeObject<FileInformation>(json);


            for (int i = 0; i < fileInformation?.SequenceCount; i++)
            {
                Console.WriteLine($"Download {i}");
                fileBytesList.Add(GetDataFromServer(AllowedPaths.SendData, $"{i}"));
            }

            Console.WriteLine("Create File");
            var filestream = new FileStream($@"G:\Meine Ablage\Studium\BIF6\BA\DataDrop\PC\DataDrop.Test\obj\Debug\net5.0\ref\{fileInformation.Filename}", FileMode.CreateNew);

            Console.WriteLine("Write File");
            foreach (var fileByte in fileBytesList)
            {
                filestream.Write(fileByte);
            }

            Console.WriteLine("Close ");

            filestream.Close();


        }

        static byte[] GetDataFromServer(AllowedPaths paths, string resource = "")
        {
            var data = new byte[10_000];
            var dataB = Encoding.UTF8.GetBytes($"GET /{paths}/{resource}");
            Console.WriteLine($"GET /{paths}/{resource}");
            _clientSocket.Send(Encoding.UTF8.GetBytes($"GET /{paths}/{resource}/"));
            //_clientSocket.Send(Encoding.UTF8.GetBytes(Console.Read().ToString()));

            byte[] receiveBuffer = new byte[10_000];
            int receive = _clientSocket.Receive(receiveBuffer);
            data = new byte[receive];
            Array.Copy(receiveBuffer, data, receive);

            Console.WriteLine($"Received: {Encoding.UTF8.GetString(data)}");

            return data;
        }

    }
}
