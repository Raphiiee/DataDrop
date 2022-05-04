using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataDrop.Models
{
    public class FileInformation
    {
        [JsonProperty]
        public string Filename { get; set; }

        [JsonProperty]
        public string FileExtension { get; set; }

        [JsonProperty]
        public int FileSize { get; set; }

        [JsonProperty]
        public int BufferSize { get; set; }

        [JsonProperty]
        public int SequenceCount { get; set; }

        [JsonProperty]
        public int HashSequenceCount { get; set; }
    }
}