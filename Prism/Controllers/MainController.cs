using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;
using System.Net;
using System.Web.Routing;

namespace Prism.Controllers
{
    public class MainController : Controller
    {

        private static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        private void UserAuth()
        {
            string IP = Request.UserHostName;
            var compName = DetermineCompName(IP);
            ViewBag.compName = compName.ToUpper();
            //var glbcfg = CfgUtility.GetSysConfig(this);

            var usermap = MachineUserMap.RetrieveUserMap();

            if (usermap.ContainsKey(ViewBag.compName))
            {
                ViewBag.username = usermap[ViewBag.compName].Trim().ToUpper();
            }
            else
            {
                ViewBag.username = string.Empty;
            }
        }


        // GET: Main
        public ActionResult Index()
        {
            UserAuth();
            if (string.IsNullOrEmpty(ViewBag.username))
            {
                var valuedict = new RouteValueDictionary();
                valuedict.Add("url", "/Main/Index");
                return RedirectToAction("Welcome","Main", valuedict);
            }

            return View();
        }

        public ActionResult Welcome(string url)
        {
            ViewBag.url = url;
            return View();
        }

        public JsonResult UpdateMachineUserName()
        {
            UserAuth();
            var username = Request.Form["username"].ToUpper().Trim();
            MachineUserMap.AddMachineUserMap(ViewBag.compName, username);
            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
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

                if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                    || DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                {
                    try
                    {
                        YieldRawData.LoadData(this);
                    }
                    catch (Exception ex) { }

                    try
                    {
                        YieldPreData.YieldPreScan(this);
                        MachinePreData.MachinePreScan(this);
                        YieldPreData.LoadProjectKey();
                    }
                    catch (Exception ex) { }
                }

            }//end only run once

            heartbeatlog("Heart Beat Start", filename);

            heartbeatlog("Heart Beat end", filename);

            return View();
        }

        public ActionResult HeartBeat2()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            ExternalDataCollector.LoadIEHPU(this,syscfg["MANUALHPUQUARTER"]);
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


        public ActionResult LoadYieldRawData()
        {
            try
            {
                YieldRawData.LoadData(this);
            }
            catch (Exception ex) { }
            return View("HeartBeat");
        }

        public ActionResult ScanYieldRawData()
        {

            return View("HeartBeat");
        }

        public ActionResult ReviewPallelYield()
        {
            return View("HeartBeat");
        }

        public ActionResult GetTableData()
        {
            var web = new Website2Data("http://wuxinpi.china.ads.finisar.com/CustomerData/ReviewIQEBackupData", @"//table[@id='pndatatable']");
            web.GetData();
            return View("HeartBeat");
        }
    }
}