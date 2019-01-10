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
                heartbeatlog("Heart Beat one day Start", filename);

                try
                {
                    PNProuctFamilyCache.LoadData();
                }
                catch (Exception ex) { }

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
                    ItemCostData.LoadCostData(this);
                }
                catch (Exception ex) { }

                try
                {
                    FsrShipData.RefreshShipData(this);
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

                try
                {
                    CapacityRawData.LoadCapacityRawData(this);
                }
                catch (Exception ex) { }

                try
                {
                    InventoryData.LoadInventoryTrend(this);
                    InventoryData.LoadInventoryDetail(this);
                }
                catch (Exception ex) { }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday
                    || DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
                {
                    try
                    {
                        WaferData.LoadWaferDataIn3Month(this);
                    }
                    catch (Exception ex) { }
                }

                try
                {
                    VcselRMAData.LoadVCSELRMA(this);
                }
                catch (Exception ex) { }

                heartbeatlog("Heart Beat one day end", filename);
            }//end only run once

            heartbeatlog("Heart Beat Start", filename);

            heartbeatlog("Heart Beat end", filename);

            return View();
        }

        public ActionResult TableCatch()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, ""))
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

            return @"http://"+EmailUtility.RetrieveCurrentMachineName()+":8088/userfiles/docs/" + datestring + "/" + fn;
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


        public ActionResult BoringSearch()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "BoringSearch"))
            {
                return RedirectToAction("Index", "Main");
            }

            var searchfield = SearchVM.SearchFields();
            searchfield.Insert(0, SEARCHFIELD.ALLFIELDS);

            var searchlist1 = CreateSelectList(searchfield, "");
            ViewBag.SearchFieldList1 = searchlist1;

            return View();
        }

        public JsonResult BoringSearchRange()
        {
            var searchfield = Request.Form["searchfield"];
            var srange = SearchVM.SearchRange(searchfield, this);
            var ret = new JsonResult();
            ret.Data = new
            {
                srange = srange
            };
            return ret;
        }
       
        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p, true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        public JsonResult BoringSearchData()
        {
            var field1 = Request.Form["field1"];
            var range1 = Request.Form["range1"];

            object obj1 = SearchVM.SearchData(field1, range1, this);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                obj1 = obj1
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

        public ActionResult LoadCapacityRawData()
        {
            CapacityRawData.LoadCapacityRawData(this);
            return View("HeartBeat");
        }

        public ActionResult LoadCostData()
        {
            ItemCostData.LoadCostData(this);
            return View("HeartBeat");
        }

        public ActionResult LoadInventoryData()
        {
            InventoryData.LoadInventoryTrend(this);
            InventoryData.LoadInventoryDetail(this);
            return View("HeartBeat");
        }

        public ActionResult LoadPNPFCache()
        {
            PNProuctFamilyCache.LoadData();
            return View("HeartBeat");
        }

        public ActionResult LoadScrapData()
        {
            ScrapData_Base.UpdateProduct();
            return View("HeartBeat");
        }

        public ActionResult RefreshShipData()
        {
            FsrShipData.RefreshShipData(this);
            return View("HeartBeat");
        }

        public ActionResult RefreshWaferData()
        {
            var vcselpndict = CfgUtility.LoadVcselPNConfig(this);
            WaferData.LoadWaferData("150814-40", vcselpndict);

            //WaferData.LoadAllWaferData(this);
            return View("HeartBeat");
        }

        public ActionResult RefreshVcselRMAData()
        {
            VcselRMAData.LoadVCSELRMA(this);
            return View("HeartBeat");
        }

        public ActionResult JOQuery()
        {
            return View();
        }

        public JsonResult JOQueryProducts()
        {
            var pfdict = PNProuctFamilyCache.PFPNDict();
            var pflist = pfdict.Keys.ToList();
            var newlist = new List<string>();
            foreach (var item in pflist)
            {
                if (item.ToUpper().Contains("PARALLEL"))
                {
                    newlist.Add(item);
                }
            }
            newlist.Sort();
            newlist.Add("10G Tunable BIDI");
            newlist.Add("T-XFP");
            newlist.Add("COHERENT");
            var ret = new JsonResult();
            ret.Data = new
            {
                pdlist = newlist
            };
            return ret;
        }

        public JsonResult JOQueryData()
        {
            var pdf = Request.Form["pdf"];
            var pn = Request.Form["pn"];
            var ssdate = Request.Form["sdate"];
            var sedate = Request.Form["edate"];

            var startdate = DateTime.Now;
            var enddate = DateTime.Now;

            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM-dd") + " 00:00:00").AddDays(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM-dd") + " 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 00:00:00").AddDays(1).AddSeconds(-1);
                }
            }
            else
            {
                enddate = DateTime.Now;
                startdate = DateTime.Now.AddDays(-7);
            }

            var title = "";
            var pncond = "";
            if (!string.IsNullOrEmpty(pdf))
            {
                var pnlist = PNProuctFamilyCache.GetPNListByPF4JO(pdf);
                pncond = "('" + string.Join("','", pnlist) + "')";
                title = pdf;
            }
            else
            {
                var pnlist = pn.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                pncond = "('" + string.Join("','", pnlist) + "')";
                title = pn;
            }

            var retdata = JOVM.QueryJO(title,pncond, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            if (retdata.Count == 0)
            {
                ret.Data = new
                {
                    success = false
                };
            }
            else
            {
                ret.Data = new
                {
                    success = true,
                    jodatalist = retdata[0],
                    joholddict = retdata[1],
                    chartdata = retdata[2]
                };
            }
            return ret;
        }

    }
}