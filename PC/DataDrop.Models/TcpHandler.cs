using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private List<byte[]> _fileBytesList = new ();
        private FileInformation _fileInformation { get; set; }
        private HashInformation _hashInformation { get; set; }

        public TcpHandler(string filepath, int bufferSize)
        {
            _filepath = filepath;
            _bufferSize = bufferSize;
            _fileInformation = new FileInformation();
            _fileInformation.BufferSize = _bufferSize;
            ProcessFileInformation();
        }

        public AllowedPaths GetPath()
        {
            return _request.Path;
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
            _response = default;
            _debugData = rawData;

            _response.Message = "Default";

            if (IsDebugSession)
            {
                _request.Path = AllowedPaths.Error;
                return;
            }

            _request.Resource = -1;
            string[] temp = rawData.Split("/"); 

            if (temp.Length > 1 && temp[2].Length > 0)
            {
                _request.Resource = Int32.Parse(temp[2]);
            }

            if (rawData.Contains(AllowedPaths.DataInformation.ToString()))
            {
                _request.Path = AllowedPaths.DataInformation;
            }
            else if (rawData.Contains(AllowedPaths.SendData.ToString()))
            {
                _request.Path = AllowedPaths.SendData;
            }
            else if (rawData.Contains(AllowedPaths.HashInformation.ToString()))
            {
                
                _request.Path = AllowedPaths.HashInformation;
            }
            else
            {
                _request.Path = AllowedPaths.Error;
            }
        }

        private void ProcessFileInformation()
        {
            GetFileInformation();
            SplitFile();
            HashAllSquences();
        }

        public void HashAllSquences()
        {
            _hashInformation = new HashInformation();
            _hashInformation.HashSequenceDictionary = new Dictionary<int, string>();
            int k = 0;

            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                foreach (var dataArray in _fileBytesList)
                {
                    byte[] hashBytes = hashAlgorithm.ComputeHash(dataArray);
                    StringBuilder hashString = new StringBuilder();  
                    for (int i = 0; i < hashBytes.Length; i++)  
                    {  
                        hashString.Append(hashBytes[i].ToString("x2"));  
                    } 
                    _hashInformation.HashSequenceDictionary.TryAdd(k++, hashString.ToString());
                }
            }

            _fileInformation.HashSequenceCount = _hashInformation.HashSequenceDictionary.Count;
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
                int remainingBytes = fileBytes.Length - (i * _bufferSize);

                if (remainingBytes > _bufferSize)
                {
                    remainingBytes = _bufferSize;
                }

                Array.ConstrainedCopy(fileBytes, i * _bufferSize, fileBytesListEntry, 0, remainingBytes);
                
                _fileBytesList.Add(fileBytesListEntry);
            }

            _fileInformation.SequenceCount = _fileBytesList.Count;
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
                if (_request.Resource == -1)
                {
                    return Encoding.UTF8.GetBytes("Error no Resource");
                }

                if (_request.Resource > _fileInformation.SequenceCount)
                {
                    return Encoding.UTF8.GetBytes($"Error no Resource with ID '{_request.Resource}'");
                }
                
                return _fileBytesList[_request.Resource];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void DataInformationJSON()
        {
            _response.Message = JsonConvert.SerializeObject(_fileInformation);
        }

        public void HashInformation()
        {

            if (_request.Resource > _fileInformation.HashSequenceCount)
            {
                _response.Message = ($"Error no Resource with ID '{_request.Resource}'");
                return;
            }
            _response.Message = _hashInformation.HashSequenceDictionary[_request.Resource];
        }
    }
}