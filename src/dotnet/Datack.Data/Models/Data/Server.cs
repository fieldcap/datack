using System;
using System.ComponentModel.DataAnnotations;
using Datack.Data.Models.Internal;

namespace Datack.Data.Models.Data
{
    public class Server
    {
        [Key]
        public Guid ServerId { get; set; }

        public String Name { get; set; }

        public ServerDbSettings DbSettings { get; set; }

        public ServerSettings Settings { get; set; }
    }
}
