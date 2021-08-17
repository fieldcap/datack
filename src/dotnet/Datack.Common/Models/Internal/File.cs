using System;

namespace Datack.Common.Models.Internal
{
    public class File
    {
        public String DatabaseName { get;set; }
        public Int32 Type { get; set; }
        public String PhysicalName { get; set; }
        public Int64 Size { get; set; }
    }
}
