﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datack.Data.Models.Internal;

namespace Datack.Data.Models.Data
{
    public class Step
    {
        [Key]
        public Guid StepId { get; set; }

        public Guid JobId { get; set; }

        [ForeignKey("JobId")]
        public Job Job { get; set; }

        public String Type { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public Int32 Order { get; set; }

        public StepSettings Settings { get; set; }
    }
}
