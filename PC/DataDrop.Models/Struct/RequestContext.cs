using DataDrop.Models.Enum;

namespace DataDrop.Models.Struct
{
    public struct RequestContext
    {
        public AllowedMethods Method;
        public int Resource;
        public AllowedPaths Path;
    }
}