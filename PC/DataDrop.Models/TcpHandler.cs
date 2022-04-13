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
        public bool IsDebugSession { get; set; }
        private string _debugData { get; set; }
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
            _response.Header = _response.Message;
        }

        public void ProcessData(string rawData)
        {
            _request = default;
            _request.Authorization = "";
            _response = default;
            _debugData = rawData;

            _response.Message = "Default";

            if (IsDebugSession)
            {
                _request.Path = AllowedPaths.Error;
                return;
            }

            if (rawData.Contains(AllowedPaths.DataInformation.ToString()))
            {
                _request.Path = AllowedPaths.DataInformation;
            }
            else if (rawData.Contains(AllowedPaths.SendData.ToString()))
            {
                _request.Path = AllowedPaths.SendData;

                string[] temp = rawData.Split("/");

                var aaaa = temp.Length; 

                if (temp.Length > 0)
                {
                    _request.Resource = temp[2];
                }
                else
                {
                    _request.Resource = "Error";
                }
            }
            else
            {
                _request.Path = AllowedPaths.Error;
            }

            /*GetFileInformation();
            if (!_isFileSplit)
            {
                SplitFile();
            }*/

        }

        public void GetFileInformation()
        {
            string filename = _filepath.Split(@"\").Last();

            _fileInformation.Filename = filename;
            _fileInformation.FileExtension = filename.Substring(filename.LastIndexOf(".", StringComparison.Ordinal)+1);
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

            _fileInformation.SequenzeCount = _fileBytesList.Count;
            _isFileSplit = true;
        }

        public void Error(string message = "Wrong Path or Method")
        {
            _response.Status = "403 Forbidden\n";

            if (IsDebugSession)
            {
                _response.Message = _debugData;
                return;
            }

            _response.Message = message;
        }

        public byte[] SendData()
        {
            try
            {
                if (_request.Resource == null)
                {
                    return Encoding.UTF8.GetBytes("Error no Resource");
                }

                int sequenze = Int32.Parse(_request.Resource);

                if (sequenze > _fileInformation.SequenzeCount)
                {
                    return Encoding.UTF8.GetBytes($"Error no Resource with ID '{sequenze}'");
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