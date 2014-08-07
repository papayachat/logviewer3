using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LogViewer3.Models
{
    public class LogDataModel
    {
        public class LogDataDetail
        {
            public int Id { get; set; }
            public string Site { get; set; }
            public string ServerIp { get; set; }
            public string UserIp { get; set; }
            //public string Logger { get; set; }
            public string Url { get; set; }
            public string Referrer { get; set; }
            public string Message { get; set; }
            public string Exception { get; set; }
            public string LogLevel { get; set; }
            public string UserAgent { get; set; }

            [DataType(DataType.DateTime)]
            public DateTime LogDate { get; set; }
            public string Arguments { get; set; }

        }

        /// <summary>
        /// 保存按Message分组后的数据
        /// </summary>
        public class LogDataSummary
        {
            //public string Logger { get; set; }
            public string Message { get; set; }
            public string LogLevel { get; set; }
            public int ErrorCnt { get; set; }
        }

        /// <summary>
        /// 供图表显示的数据
        /// </summary>
        public class LogDataChart
        {
            public string GroupedDateTime { get; set; }
            public int ErrorCnt { get; set; }
        }
    }
}