using System;

namespace Datack.Common.Models.Internal
{
    public class JobSettings
    {
        public Boolean EmailOnError { get; set; }
        public Boolean EmailOnSuccess { get; set; }
        public String EmailTo { get; set; }
    }
}
