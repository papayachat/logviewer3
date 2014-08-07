using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BootstrapSupport.HtmlHelpers;
using LogViewer3.Models;
using LogViewer3.Models.Repos;
using LogViewer3.Models.Repos.Impl;
using Models;

namespace BootstrapMvcSample.Controllers
{
    public class HomeController : BootstrapBaseController
    {
        private static List<HomeInputModel> _models = ModelIntializer.CreateHomeInputModels();
        private static ILogRespository logRespository = new LogRespository();

        public ActionResult Index(string site, string startDateTime)
        {
            if (string.IsNullOrWhiteSpace(site)) return View();

            DateTime dt;
            if (string.IsNullOrWhiteSpace(startDateTime))
            {
                DateTime now = DateTime.Now;
                dt = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            }
            else
            {
                dt = DateTime.Parse(startDateTime);
            }
           
            var logDataModelList = logRespository.LogSummary(site, dt);

            ViewBag.Site = Request.QueryString["site"];
            ViewBag.StartDateTime = string.IsNullOrWhiteSpace(Request.QueryString["startDateTime"])
                ? DateTime.Now.ToString("yyyy-MM-dd 00:00")
                : Request.QueryString["startDateTime"];
            return View(logDataModelList);
        }


        #region useless
        [HttpPost]
        public ActionResult Create(HomeInputModel model)
        {
            if (ModelState.IsValid)
            {
                model.Id = _models.Count==0?1:_models.Select(x => x.Id).Max() + 1;
                _models.Add(model);
                Success("Your information was saved!");
                return RedirectToAction("Index");
            }
            Error("there were some errors in your form.");
            return View(model);
        }

        public ActionResult Create()
        {
            return View(new HomeInputModel());
        }

        public ActionResult Delete(int id)
        {
            _models.Remove(_models.Get(id));
            Information("Your widget was deleted");
            if(_models.Count==0)
            {
                Attention("You have deleted all the models! Create a new one to continue the demo.");
            }
            return RedirectToAction("index");
        }
        public ActionResult Edit(int id)
        {
            var model = _models.Get(id);
            return View("Create", model);
        }
        [HttpPost]        
        public ActionResult Edit(HomeInputModel model,int id)
        {
            if(ModelState.IsValid)
            {
                _models.Remove(_models.Get(id));
                model.Id = id;
                _models.Add(model);
                Success("The model was updated!");
                return RedirectToAction("index");
            }
            return View("Create", model);
        }
        #endregion

        public ActionResult Details(int id)
        {
            var model = logRespository.LogDetail(id);
            
            return View(model);
        }

        public ActionResult List(string site,string message,string startDateTime,string endDateTime, int pageIndex)
        {
            int pageSize = 20;
            pageSize = 10;

            DateTime start, end;
            try
            {
                start = string.IsNullOrWhiteSpace(startDateTime) ? DateTime.Now.AddDays(-1) : DateTime.Parse(startDateTime);
                end = string.IsNullOrWhiteSpace(endDateTime) ? DateTime.Now : DateTime.Parse(endDateTime);
            }
            catch (Exception)
            {
                Error("输入的时间格式错误");
                return RedirectToAction("index", new{Site=site});
            }
            if (end < start)
            {
                return View();
            }
            if ((end - start).TotalDays > 10)
            {
                Error("时间间隔过大");
                return RedirectToAction("index", new { Site = site });
            }
            var list = logRespository.LogList(site, message, start, end, pageIndex, pageSize);
            var logCount = logRespository.LogCount(site, message, start, end);
            //MvcPaging.IPagedList<LogDataModel.LogDataDetail> pagedList = new MvcPaging.PagedList<LogDataModel.LogDataDetail>(list, pageIndex, pageSize, logCount);

            IPagedList<LogDataModel.LogDataDetail> pagedList = new PagedList<LogDataModel.LogDataDetail>(list, pageIndex, pageSize, logCount);


            ViewBag.Site = site;
            ViewBag.Message = message;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalItemCount = logCount;
            ViewBag.TotalPages = logCount%pageSize;
            ViewBag.StartDateTime = start.ToString("yyyy-MM-dd HH:mm");
            ViewBag.EndDateTime = end.ToString("yyyy-MM-dd HH:mm");
            return View(pagedList);
        }

        public ActionResult DashBoard()
        {
            return View();
        }

        public ActionResult Chart(string site, string startDateTime,string endDateTime,int freq)
        {
            var model = logRespository.Chart();
            return View(model);
        }

        public ActionResult ErrorCount(string startDateTime, string endDateTime, string viewName = "ErrorCount")
        {
            DateTime start, end;
            if (string.IsNullOrWhiteSpace(startDateTime))
            {
                DateTime now = DateTime.Now;
                start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            }
            else
            {
                start = DateTime.Parse(startDateTime);
            }
            if (string.IsNullOrWhiteSpace(endDateTime))
            {
                end = DateTime.Now;
            }
            else
            {
                end = DateTime.Parse(endDateTime);
            }
            var model = logRespository.CurrentLogCount(start, end);
            return View(viewName, model);
        }


    }
}
