using System;
using System.ComponentModel.DataAnnotations;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data
{
    public class Server
    {
        [Key]
        public Guid ServerId { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public String Key { get; set; }
        
        public ServerDbSettings DbSettings { get; set; }

        public ServerSettings Settings { get; set; }
    }
}
