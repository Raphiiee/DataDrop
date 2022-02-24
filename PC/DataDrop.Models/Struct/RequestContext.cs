using DataDrop.Models.Enum;

namespace DataDrop.Models.Struct
{
    public struct RequestContext
    {
        public string Header;
        public string Version;
        public AllowedMethods Method;
        public string Resource;
        public AllowedPaths Path;
        public string Authorization;
        public string Message;
    }
}