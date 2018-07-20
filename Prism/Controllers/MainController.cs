using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;

namespace Prism.Controllers
{
    public class MainController : Controller
    {
        // GET: Main
        public ActionResult Index()
        {
            return View();
        }

        private void heartbeatlog(string msg,string filename)
        {
            try
            {
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = "";
                if (System.IO.File.Exists(wholefilename))
                {
                    content = System.IO.File.ReadAllText(wholefilename);
                }
                content = content + msg + " @ " + DateTime.Now.ToString() + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }
        }

        public ActionResult HeartBeat()
        {
            var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
            if (!System.IO.File.Exists(wholefilename))
            {
                try
                {
                    ExternalDataCollector.LoadPNPlannerData(this);
                }
                catch (Exception ex) { }

                try
                {
                    ExternalDataCollector.LoadScrapData(this);
                }
                catch (Exception ex) { }

                try
                {
                    ExternalDataCollector.LoadIEScrapBuget(this);
                }
                catch (Exception ex) { }

                try
                {
                    ExternalDataCollector.LoadIEHPU(this);
                }
                catch (Exception ex) { }

                try
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
                    {
                        CostCentScrapWarning.Waring(this);
                    }
                }
                catch (Exception ex) { }
            }//end only run once

            heartbeatlog("Heart Beat Start", filename);

            return View();
        }

        public ActionResult HeartBeat2()
        {
            ExternalDataCollector.LoadIEHPU(this);
            return View("HeartBeat");
        }


        public ActionResult SendScrapReport()
        {
            try
            {
               CostCentScrapWarning.Waring(this);
            }
            catch (Exception ex) { }
            return View("HeartBeat");
        }


    }
}