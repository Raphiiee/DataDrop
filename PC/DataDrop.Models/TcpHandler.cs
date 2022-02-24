using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DataDrop.Models.Enum;
using DataDrop.Models.Struct;
using Newtonsoft.Json;

namespace DataDrop.Models
{
    public class TcpHandler
    {
        private ResponseContext _response;
        private RequestContext _request;
        private string _filepath;
        private int _bufferSize = 10_000;
        private List<byte[]> _fileBytesList = new List<byte[]>();
        private bool _isFileSplit = false;
        private FileInformation _fileInformation { get; set; }

        public TcpHandler(string filepath, int bufferSize)
        {
            _filepath = filepath;
            _bufferSize = bufferSize;
            _fileInformation = new FileInformation();
            _fileInformation.BufferSize = _bufferSize;
        }

        public AllowedPaths GetPath()
        {
            return _request.Path;
        }

        public AllowedMethods GetMethod()
        {
            return _request.Method;
        }

        public string GetResponseHeader()
        {
            return _response.Header;
        }

        public void CreateResponseHeader()
        {
            _response.ContentLength = $"Content-Length: {_response.Message.Length}\n";
            _response.Header = _response.Version
                               + _response.Status
                               + _response.ContentLength
                               + _response.ContentType
                               + _response.Charset
                               + _response.Message;
        }

        public void ProcessData(string rawData)
        {
            _request = default;
            _request.Authorization = "";
            _response = default;

            string[] requestHeader = rawData.Split("\n");
            // First element in array is a resource methode; second element is a resource path; third HTTP Version
            string[] resource = requestHeader[0].Split(" ");

            if (resource[0].Contains("GET"))
            {
                _request.Method = AllowedMethods.GET;
            }
            else if (resource[0].Contains("PUT"))
            {
                _request.Method = AllowedMethods.POST;
            }
            else
            {
                _request.Method = AllowedMethods.Error;
            }

            if (resource[1].Contains("DATAINFORMATION"))
            {
                _request.Path = AllowedPaths.DataInformation;
            }
            else if (resource[1].Contains("SENDDATA"))
            {
                _request.Path = AllowedPaths.SendData;

                string[] temp = resource[1].Split('/');

                // temp[2] have length of 3!
                if (temp.Length > 2)
                {
                    _request.Resource = temp[2];
                }
            }
            else
            {
                _request.Path = AllowedPaths.Error;
            }

            _request.Header = rawData;
            if (_request.Header.Contains("{"))
            {
                _request.Message = _request.Header[_request.Header.IndexOf("{", StringComparison.Ordinal)..];
            }

            _response.Version = "HTTP/1.1 ";
            _response.Status = "200 OK\n";
            _response.Message = "";
            _response.ContentType = "Content-Type: text/plain\n";
            _response.Charset = "Charset: utf-16\n\n";

            GetFileInformation();
            if (!_isFileSplit)
            {
                SplitFile();
            }

        }

        public void GetFileInformation()
        {
            string filename = _filepath.Split(@"\").Last();

            _fileInformation.Filename = filename;
            if (_fileInformation.Filename.Length < 1)
            {
                // throw expection not a file
            }

            _fileInformation.FileSize = File.ReadAllBytes(_filepath).Length;

            _fileInformation.BufferSize = _bufferSize;
        }

        public void SplitFile()
        {
            var fileBytes = File.ReadAllBytes(_filepath);

            for (int i = 0; i < (int)Math.Ceiling((double)fileBytes.Length / _bufferSize); i++)
            {
                byte[] fileBytesListEntry = new byte[_bufferSize];
                // Calculate remainingBytes for the constrainedCopy
                int remainingBytes = fileBytes.Length - (i * _bufferSize);

                if (remainingBytes > _bufferSize)
                {
                    remainingBytes = _bufferSize;
                }

                Array.ConstrainedCopy(fileBytes, i * _bufferSize, fileBytesListEntry, 0, remainingBytes);
                
                _fileBytesList.Add(fileBytesListEntry);
            }

            _isFileSplit = true;
        }

        public void Error(string message = "Wrong Path or Method")
        {
            _response.Status = "403 Forbidde\n";

            _response.Message = message;
        }

        public byte[] SendData()
        {
            try
            {
                if (_request.Resource == null)
                {
                    return Encoding.ASCII.GetBytes("Error no Resource");
                }

                int sequenze = Int32.Parse(_request.Resource);

                if (sequenze > _fileInformation.SequenzeCount)
                {
                    return Encoding.ASCII.GetBytes($"Error no Resource with ID '{sequenze}'");
                }
                
                return _fileBytesList[sequenze];
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void DataInformationJSON()
        {
            // Information for the client;
            // Filename, size of file [bytes], buffer size [bytes], sequenze count,
            // later checksums of each sequenze, maybe auto decompress zip [bool]

            _response.Message = JsonConvert.SerializeObject(_fileInformation);
        }



        public void ClientFinished()
        {
            // Stop/Close everything
            

        }


    }
}