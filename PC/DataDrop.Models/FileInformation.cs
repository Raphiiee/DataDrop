using System;
using Newtonsoft.Json;

namespace DataDrop.Models
{
    public class FileInformation
    {
        [JsonProperty]
        public string Filename { get; set; }

        [JsonProperty]
        public int FileSize { get; set; }

        [JsonProperty]
        public int BufferSize { get; set; }

        [JsonProperty]
        public int SequenzeCount {
            get
            {
                if (FileSize > 0 && BufferSize > 0)
                {
                    return FileSize / BufferSize;
                }

                return 0;
            }
        }
    }
}