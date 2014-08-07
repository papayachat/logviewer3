using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using LogViewer3.Models.Enums;

namespace LogViewer3.Models.Repos.Impl
{
    public class LogRespository : ILogRespository
    {
        readonly string _connString = ConfigurationManager.ConnectionStrings["LogConnectionString"].ConnectionString;
        public LogDataModel.LogDataDetail LogDetail(int id)
        {
            var detail = new LogDataModel.LogDataDetail();
            using (SqlConnection conn = new SqlConnection { ConnectionString = _connString })
            {
                var sql = @"select * from  ErrorLog(nolock) where id=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", id);
                conn.Open();
                var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                while (reader.Read())
                {
                    detail.Id = id;
                    detail.Site = reader["Site"].ToString();
                    detail.Exception = reader["Exception"].ToString();
                    detail.LogDate = (DateTime) reader["LogDate"];
                    detail.LogLevel = reader["LogLevel"].ToString();
                    //detail.Logger = reader["Logger"].ToString();
                    detail.Message = reader["Message"].ToString();
                    detail.ServerIp = reader["ServerIp"].ToString();
                    detail.UserAgent = reader["UserAgent"].ToString();
                    detail.UserIp = reader["UserIp"].ToString();
                    detail.Arguments = reader["Arguments"].ToString();
                    detail.Url = reader["Url"].ToString();
                    detail.Referrer = reader["Referrer"].ToString();
                }
                conn.Close();
                return detail;
            }
        }

        public IList<LogDataModel.LogDataSummary> LogSummary(string site, DateTime startDateTime)
        {
            using (SqlConnection conn = new SqlConnection {ConnectionString = _connString})
            {
                var sql = @"
select [Message],[loglevel],COUNT(*) as ErrorCnt 
from ErrorLog(nolock) 
where site=@site and logdate>@startDateTime
group by [Message],[loglevel]
order by count(*) desc";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("site", site);
                cmd.Parameters.AddWithValue("startDateTime", startDateTime);
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                DataSet ds = new DataSet();
                conn.Open();
                da.Fill(ds);
                conn.Close();
                if (ds.Tables.Count > 0)
                {
                    IList<LogDataModel.LogDataSummary> logList =
                        ds.Tables[0].AsEnumerable().Select(row => new LogDataModel.LogDataSummary
                        {
                            //Logger = row.Field<string>("logger"),
                            Message = row.Field<string>("message"),
                            LogLevel = row.Field<string>("loglevel"),
                            ErrorCnt = row.Field<int>("errorcnt"),
                        }).ToList();
                    return logList;
                }
            }
            return null;
        }

        public IList<LogDataModel.LogDataDetail> LogList(string site, string message, DateTime startDateTime, DateTime endDateTime, int pageIndex = 1,
            int pageSize = 20)
        {
            int pageStart = (pageIndex-1) * pageSize + 1;
            int pageEnd = (pageIndex) * pageSize;

            using (SqlConnection conn = new SqlConnection {ConnectionString = _connString})
            {
                var sql = @"
with loglist as
(
	select ROW_NUMBER() over (order by id desc) as row, ID,ServerIP,UserIP,Logger,[Message],LogLevel,UserAgent,LogDate,Arguments,Url,Referrer
	from ErrorLog(nolock)
	where [Site]=@site and [message]=@message and LogDate between @startDateTime and @endDateTime
)
select row,ID,ServerIP,UserIP,Logger,[Message],LogLevel,UserAgent,LogDate,Arguments,Url,Referrer
from loglist
where row between @pageStart and @pageEnd";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("site", site);
                cmd.Parameters.AddWithValue("message", message);
                cmd.Parameters.AddWithValue("startDateTime", startDateTime);
                cmd.Parameters.AddWithValue("endDateTime", endDateTime);
                cmd.Parameters.AddWithValue("pageStart", pageStart);
                cmd.Parameters.AddWithValue("pageEnd", pageEnd);

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                DataSet ds = new DataSet();
                conn.Open();
                da.Fill(ds);
                conn.Close();
                if (ds.Tables.Count > 0)
                {
                    IList<LogDataModel.LogDataDetail> logList =
                        ds.Tables[0].AsEnumerable().Select(row => new LogDataModel.LogDataDetail
                        {
                            Id = row.Field<int>("id"),
                            Site = site,
                            //Logger = row.Field<string>("logger"),
                            Url = row.Field<string>("url"),
                            Referrer = row.Field<string>("referrer"),
                            Message = row.Field<string>("message"),
                            LogLevel = row.Field<string>("loglevel"),
                            ServerIp = row.Field<string>("serverip"),
                            UserIp = row.Field<string>("userip"),
                            UserAgent = row.Field<string>("useragent"),
                            LogDate = row.Field<DateTime>("logdate"),
                            Arguments = row.Field<string>("arguments")
                        }).ToList();
                    return logList;
                }
            }
            return null;
        }

        public int LogCount(string site, string message, DateTime startDateTime, DateTime endDateTime)
        {
            using (SqlConnection conn = new SqlConnection { ConnectionString = _connString })
            {
                var sql = @"
select count(1) 
from ErrorLog(nolock)
where [Site]=@site and [message]=@message and LogDate between @startDateTime and @endDateTime
";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("site", site);
                cmd.Parameters.AddWithValue("message", message);
                cmd.Parameters.AddWithValue("startDateTime", startDateTime);
                cmd.Parameters.AddWithValue("endDateTime", endDateTime);
                
                conn.Open();
                object o = cmd.ExecuteScalar();
                conn.Close();
                return (int) o;
            }
        }

        public IList<LogStatusModel> CurrentLogCount(DateTime startDateTime, DateTime endDateTime)
        {
            IList<LogStatusModel> list = new List<LogStatusModel>();
            using (SqlConnection conn = new SqlConnection { ConnectionString = _connString })
            {
                var sql = @"
select sites.[Site],COUNT(id) as cnt from sites
left outer join ErrorLog on sites.[Site]=ErrorLog.[site]
and logdate between @startDateTime and @endDateTime
group by sites.[site]
order by sites.[site]
";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("startDateTime", startDateTime);
                cmd.Parameters.AddWithValue("endDateTime", endDateTime);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
                while (reader.Read())
                {
                    list.Add(new LogStatusModel
                    {
                        Site = reader["site"].ToString(),
                        Count = (int)reader["cnt"],
                        Status = CurrentStatus((int)reader["cnt"],endDateTime,endDateTime-startDateTime)
                    });
                    //dictionary.Add(reader["site"].ToString(),(int)reader["cnt"]);
                }
                conn.Close();
                return list;
            }
        }

        public LogStatus CurrentStatus(int logCount, DateTime currentDateTime, TimeSpan timeSpan)
        {
            int[] threshold = GetThreshold(currentDateTime);

            var status = LogStatus.Normal;
            if (logCount > threshold[0] * timeSpan.TotalMinutes) status = LogStatus.Warning;
            if (logCount > threshold[1] * timeSpan.TotalMinutes) status = LogStatus.Dangerous;
            return status;
        }

        public IList<LogDataModel.LogDataChart> Chart(string site, DateTime startDateTime, DateTime endDateTime, Freq freq)
        {
            IList<LogDataModel.LogDataChart> list = new List<LogDataModel.LogDataChart>();
            using (SqlConnection conn = new SqlConnection { ConnectionString = _connString })
            {

                var sql = freq == Freq.Minute
                    ? @"select convert(varchar(16),logdate,20) as logdate,count(*) as cnt from errorlog(nolock) where [site]='@site' and logdate>'@startDateTime' and logdate<'@endDateTime' group by convert(varchar(16),logdate,20) order by convert(varchar(16),logdate,20)"
                    : @"select convert(varchar(13),logdate,20) as logdate,count(*) as cnt from errorlog(nolock) where [site]='@site' and logdate>'@startDateTime' and logdate<'@endDateTime' group by convert(varchar(13),logdate,20) order by convert(varchar(13),logdate,20)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("site", site);
                cmd.Parameters.AddWithValue("startDateTime", startDateTime);
                cmd.Parameters.AddWithValue("endDateTime", endDateTime);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
                while (reader.Read())
                {
                    list.Add(new LogDataModel.LogDataChart
                    {
                        GroupedDateTime = reader[0].ToString(),
                        ErrorCnt = (int)reader[1]
                    });
                }
                conn.Close();
                return list;
            }
        }

        /// <summary>
        /// 注意这里是每分钟错误数阈值
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        private int[] GetThreshold(DateTime datetime)
        {
            if (datetime.Hour >= 0 && datetime.Hour < 8)
            {
                return new[] {5, 10};
            }
            if (datetime.Hour >= 8 && datetime.Hour <= 23)
            {
                return new[] {10, 20};
            }
            return new[] { 5, 10 };
        }
    }
}
