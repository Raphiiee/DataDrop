using System;
using System.Net;
using DataDrop.Models.Enum;
using DataDrop.Models.Struct;

namespace DataDrop.Models
{
    public class TcpHandler
    {
        private ResponseContext _response;
        private RequestContext _request;
        private string _extension; // Data extension 

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
            if (_extension != null)
            {
                _response.ContentType = $"Content-Type: application/{_extension.ToLower()}\n";
            }
            _response.ContentLength = $"Content-Length: {_response.Message.Length}\n";
            _response.Header = _response.Version
                               + _response.Status
                               + _response.Server
                               + _response.ContentLength
                               + _response.ContentLanguage
                               + _response.ContentType
                               + _response.Connection
                               + _response.Charset
                               + _response.Message;
        }

        public void ProcessData(string data)
        {
            _request = default;
            _request.Authorization = "";
            _response = default;

            string[] requestHeader = data.Split("\n");
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

            if (resource[1].Contains("RECEIVEDATA"))
            {
                _request.Path = AllowedPaths.ReceiveData;
            }
            else if (resource[1].Contains("SENDDATA"))
            {
                _request.Path = AllowedPaths.SendData;
            }
            else
            {
                _request.Path = AllowedPaths.Error;
            }

            // HTTP Version
            _request.Version = resource[2];

            foreach (var requestData in requestHeader)
            {
                if (requestData.Contains("AUTHORIZATION"))
                {
                    _request.Authorization = requestData;
                    break;
                }
            }

            _request.Header = data;
            if (_request.Header.Contains("{"))
            {
                _request.Message = _request.Header[_request.Header.IndexOf("{", StringComparison.Ordinal)..];
            }

            _response.Version = "HTTP/1.1 ";
            _response.Status = "200 OK\n";
            _response.Server = "Server: DataDrop\n";
            _response.Message = "Something went wrong";
            _response.ContentLength = $"Content-Length: {_response.Message.Length}\n";
            _response.ContentLanguage = "Content-Language: de\n";
            _response.ContentType = "Content-Type: text/plain\n";
            _response.Connection = "Connection: close\nKeep-Alive: timeout=50, max=0\n";
            _response.Charset = "Charset: utf-16\n\n";

        }

        public void Error()
        {
            _response.Status = "403 Forbidde\n";
            _response.Message = "Wrong Path or Method";
        }

        public void SendData(string filePath)
        {

        }

        public void ReceiveData(string filePath)
        {

        }


    }
}