using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LogViewer3.Models.Enums;

namespace LogViewer3.Models
{
    public class LogStatusModel
    {
        public string Site { get; set; }
        public int Count { get; set; }
        public LogStatus Status { get; set; }
    }
}