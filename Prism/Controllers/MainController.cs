using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;
using System.Net;
using System.Web.Routing;
using System.Text;
using System.IO;

namespace Prism.Controllers
{
    public class MainController : Controller
    {

        // GET: Main
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult Welcome(string url)
        //{
        //    ViewBag.url = url;
        //    return View();
        //}

        //public JsonResult UpdateMachineUserName()
        //{
        //    var username = Request.Form["username"].ToUpper().Trim();
        //    MachineUserMap.AddMachineUserMap(ViewBag.compName, username);
        //    var ret = new JsonResult();
        //    ret.Data = new { sucess = true };
        //    return ret;
        //}

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

                try
                {
                    RMARAWData.LoadRMARawData(this);
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

        public ActionResult TableCatch()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        private string DumpTableData(List<List<string>> lines)
        {

            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var fn = "Table_data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

             var sb = new StringBuilder(120 * lines.Count);
            foreach (var line in lines)
            {
                var linesb = new StringBuilder(120);
                foreach (var item in line)
                {
                    linesb.Append("\"" + item.Replace("\"", "") + "\",");
                }
                linesb.Append("\r\n");
                sb.Append(linesb.ToString());
            }

            if (sb.Length > 0)
            {
                var fw = System.IO.File.OpenWrite(filename);
                var CHUNK_STRING_LENGTH = 30000;
                while (sb.Length > CHUNK_STRING_LENGTH)
                {
                    var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString(0, CHUNK_STRING_LENGTH));
                    fw.Write(bt, 0, bt.Count());
                    sb.Remove(0, CHUNK_STRING_LENGTH);
                }

                var bt1 = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                fw.Write(bt1, 0, bt1.Count());
                fw.Close();
            }

            return @"http://"+EmailUtility.RetrieveCurrentMachineName()+"/userfiles/docs/" + datestring + "/" + fn;
        }

        private string Base642Str(string bcontent)
        {
            var ret = "";
            if (!string.IsNullOrEmpty(bcontent))
            {
                try
                {
                    string dummyData = bcontent.Trim().Replace(" ", "+");
                    if (dummyData.Length % 4 > 0)
                        dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');

                    var bytes = Convert.FromBase64String(dummyData);
                    ret = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch (Exception ex) { }
            }
            return ret;
        }

        public JsonResult GetDataFromPage()
        {
            var bhtmlcontent = Request.Form["content"];
            var htmlcontent = Base642Str(bhtmlcontent);
            var rawdata = Website2Data.Page2Data(htmlcontent, @"//table");
            if (rawdata.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.Data = new
                { success = false };
                return ret1;
            }

            var url = DumpTableData(rawdata);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                url = url
            };
            return ret;
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

        public ActionResult LoadRMARawData()
        {
            RMARAWData.LoadRMARawData(this);
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