using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogViewer3.Models.Enums;

namespace LogViewer3.Models.Repos
{
    public interface ILogRespository
    {
        LogDataModel.LogDataDetail LogDetail(int id);
        /// <summary>
        /// 错误日志按站点总览
        /// </summary>
        /// <param name="site">站点名</param>
        /// <param name="startDateTime">取这个时间点之后到当前时间的数据</param>
        /// <returns></returns>
        IList<LogDataModel.LogDataSummary> LogSummary(string site, DateTime startDateTime);

        IList<LogDataModel.LogDataDetail> LogList(string site, string message, DateTime startDateTime, DateTime endDateTime, int pageIndex, int pageSize);

        int LogCount(string site, string message, DateTime startDateTime, DateTime endDateTime);

        IList<LogStatusModel> CurrentLogCount(DateTime startDateTime, DateTime endDateTime);

        LogStatus CurrentStatus(int logCount, DateTime currentDateTime, TimeSpan timeSpan);
    }
}
