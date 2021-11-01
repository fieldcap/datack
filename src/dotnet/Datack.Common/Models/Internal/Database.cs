using System;

namespace Datack.Common.Models.Internal
{
    public class Database
    {
        public Int32 DatabaseId { get; set; }
        public String DatabaseName { get;set; }
        public Boolean HasAccess { get; set; }
    }
}
