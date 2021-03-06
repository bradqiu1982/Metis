﻿using System;
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

        private List<double> slop(List<double> alist, List<double> plist, double alow, double ahigh)
        {
            var ret = new List<double>();
            try
            {
                var aalist = new List<double>();
                var aplist = new List<double>();
                var idx = 0;
                foreach (var a in alist)
                {
                    if (a > alow && a < ahigh)
                    {
                        aalist.Add(a);
                        aplist.Add(plist[idx]);
                    }
                    idx++;
                }

                if (aalist.Count > 5)
                {
                    var res = MathNet.Numerics.Fit.Line(aalist.ToArray(), aplist.ToArray());
                    var a = res.Item1;
                    var b = res.Item2;

                    var y1list = new List<double>();
                    foreach (var x in aalist)
                    { y1list.Add(b*x+a); }
                    var r2 = MathNet.Numerics.GoodnessOfFit.RSquared(y1list, aplist);

                    ret.Add(b);ret.Add(r2);
                    return ret;
                }
            }
            catch (Exception ex) { }

            ret.Clear();
            return ret;
        }

        private double slopf2(string wholefilename, string f,List<double> alist, List<double> plist, double alow, double ahigh)
        {
            try
            {
                var aalist = new List<double>();
                var aplist = new List<double>();
                var idx = 0;
                foreach (var a in alist)
                {
                    if (a > alow && a < ahigh)
                    {
                        aalist.Add(a);
                        aplist.Add(plist[idx]);
                    }
                    idx++;
                }

                var sloplist = new List<string>();

                for(var jdx = 0;jdx < aalist.Count - 22;jdx++)
                {
                    var xlist = new List<double>();
                    var ylist = new List<double>();
                    for (var hdx = 0; hdx < 20; hdx++)
                    {
                        xlist.Add(aalist[jdx+hdx]);
                    }
                    for (var hdx = 0; hdx < 20; hdx++)
                    {
                        ylist.Add(aplist[jdx+hdx]);
                    }

                    var res = MathNet.Numerics.Fit.Line(xlist.ToArray(), ylist.ToArray());
                    sloplist.Add(res.Item2.ToString());
                }

                var slops = string.Join(",", sloplist);

                var content = "";
                if (System.IO.File.Exists(wholefilename))
                {
                    content = System.IO.File.ReadAllText(wholefilename);
                }
                content = content + Path.GetFileNameWithoutExtension(f).Substring(0, 17) + "," +slops + ",\r\n";
                System.IO.File.WriteAllText(wholefilename, content);

            }
            catch (Exception ex) { }
            return 0;
        }

        //public ActionResult GetSlope()
        //{
        //    var wholefilename = Server.MapPath("~/userfiles") + "\\" + "slop.txt";

        //    var fs = ExternalDataCollector.DirectoryEnumerateFiles(this, @"\\wux-engsys01\PlanningForCast\slop\Zurich\Pre");
        //    foreach (var f in fs)
        //    {
        //        if (!f.ToUpper().Contains(".TXT"))
        //        { continue; }

        //        var data = System.IO.File.ReadAllLines(f);
        //        var alist = new List<double>();
        //        var plist = new List<double>();

        //        var idx = 0;

        //        foreach (var line in data)
        //        {
        //            var s = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
        //            if ((int)s[0][0] > 47 && (int)s[0][0] < 58)
        //            {
        //                alist.Add(UT.O2D(s[1]));
        //                plist.Add(UT.O2D(s[2]));
        //                var t = UT.O2I(s[0]);
        //                if (t == 50)
        //                {
        //                    var slop1 = slop(alist, plist, 2, 6);
        //                    var slop2 = slop(alist, plist, 2, 6.5);
        //                    var slop3 = slop(alist, plist, 2, 8);
        //                    var slop4 = slop(alist, plist, 0, 10);

        //                    var content = "";
        //                    if (System.IO.File.Exists(wholefilename))
        //                    {
        //                        content = System.IO.File.ReadAllText(wholefilename);
        //                    }
        //                    content = content + Path.GetFileNameWithoutExtension(f).Substring(0, 7) + "_" + (idx++).ToString() + "," + slop1.ToString() + "," + slop2.ToString() + "," + slop3.ToString() + "," + slop4.ToString() + "\r\n";
        //                    System.IO.File.WriteAllText(wholefilename, content);

        //                    alist.Clear();
        //                    plist.Clear();
        //                }
        //            }

        //        }//end foreach
        //    }//end foreach

        //    return View("HeartBeat");
        //}

        public ActionResult GetSlope()
        {
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + "slop.txt";

            var fs = ExternalDataCollector.DirectoryEnumerateFiles(this, @"\\wux-engsys01\PlanningForCast\slop\rp00");
            foreach (var f in fs)
            {
                if (!f.ToUpper().Contains(".CSV"))
                { continue; }

                var data = ExcelReader.RetrieveDataFromExcel_CSV(f, null, 8);
                var alist = new List<double>();
                var plist = new List<double>();

                var idx = 0;
                foreach (var line in data)
                {
                    if (idx == 0)
                    { idx++; continue; }
                    alist.Add(UT.O2D(line[3]));
                    plist.Add(UT.O2D(line[6]));
                }//end foreach

                //var slop1 = slop(alist, plist, 3.99, 10.0);
                //var slop2 = slopf2( wholefilename,f,alist, plist, 2, 12);

                for (idx = 1; idx < 51; idx++)
                {
                    var low = 0 + idx * 0.1;
                    var high = 12 - idx * 0.1;

                    var val = slop(alist, plist, low, high);
                    if (val.Count > 0)
                    {
                        if (val[1] >= 0.999||idx == 50)
                        {
                            var content = "";
                            if (System.IO.File.Exists(wholefilename))
                            {
                                content = System.IO.File.ReadAllText(wholefilename);
                            }
                            content = content + Path.GetFileNameWithoutExtension(f).Substring(0, 17) + "," + UT.O2S(low) + "," + UT.O2S(high) + "," + UT.O2S(val[1]) + "," + UT.O2S(val[0]) + ",\r\n";
                            System.IO.File.WriteAllText(wholefilename, content);

                            break;
                        }
                    }
                }

            }//end foreach

            return View("HeartBeat");
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
                    heartbeatlog("ProductCostVM.RefreshFCost", filename);
                    ProductCostVM.RefreshFCost(this);
                }
                catch (Exception ex) { }

                //try
                //{
                //    heartbeatlog("PNBUMap.LoadPNBUData", filename);
                //    PNBUMap.LoadPNBUData(this);
                //}
                //catch (Exception ex) { }

                try
                {
                    heartbeatlog("OSASeriesData.LoadData", filename);
                    OSASeriesData.LoadData(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("PNProuctFamilyCache.LoadData", filename);
                    PNProuctFamilyCache.LoadData();
                }
                catch (Exception ex) { }

                
                try
                {
                    heartbeatlog("ExternalDataCollector.LoadPNPlannerData", filename);
                    ExternalDataCollector.LoadPNPlannerData(this);
                }
                catch (Exception ex) { }

                
                try
                {
                    heartbeatlog("ExternalDataCollector.LoadScrapData", filename);
                    ExternalDataCollector.LoadScrapData(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("ExternalDataCollector.LoadIEScrapBuget", filename);
                    ExternalDataCollector.LoadIEScrapBuget(this);
                }
                catch (Exception ex) { }

                //try
                //{
                //    heartbeatlog("ItemCostData.LoadCostData", filename);
                //    ItemCostData.LoadCostData(this);
                //}
                //catch (Exception ex) { }

                try
                {
                    heartbeatlog("FsrShipData.RefreshShipData", filename);
                    FsrShipData.RefreshShipData(this);
                }
                catch (Exception ex) { }

                try
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
                    {
                        heartbeatlog("CostCentScrapWarning.Waring", filename);
                        CostCentScrapWarning.Waring(this);
                    }
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("RMARAWData.LoadRMARawData", filename);
                    RMARAWData.LoadRMARawData(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("YieldRawData.LoadData", filename);
                    YieldRawData.LoadData(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("ModuleTestData.SendHydraWarningEmail", filename);
                    HYDRASummary.SendHydraWarningEmail(this);
                }
                catch (Exception ex) { }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                    || DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                {
                    try
                    {
                        heartbeatlog("YieldPreData.YieldPreScan", filename);
                        YieldPreData.YieldPreScan(this);
                        MachinePreData.MachinePreScan(this);
                        YieldPreData.LoadProjectKey();
                    }
                    catch (Exception ex) { }
                }

                try
                {
                    heartbeatlog("CapacityRawData.LoadCapacityRawData", filename);
                    CapacityRawData.LoadCapacityRawData(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("InventoryData.LoadInventoryTrend", filename);
                    InventoryData.LoadInventoryTrend(this);
                    InventoryData.LoadInventoryDetail(this);
                }
                catch (Exception ex) { }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday
                    || DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
                {
                    try
                    {
                        heartbeatlog("WaferData.LoadWaferDataIn3Month", filename);
                        WaferData.LoadWaferDataIn3Month(this);
                    }
                    catch (Exception ex) { }
                }

                try
                {
                    heartbeatlog("VcselRMAData.LoadVCSELRMA", filename);
                    VcselRMAData.LoadVCSELRMA(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("ExternalDataCollector.LoadIEHPU", filename);
                    ExternalDataCollector.LoadIEHPU(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("ModuleRevenue.LoadData", filename);
                    ModuleRevenue.LoadData(this);
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("ItemCostData.LoadMonthlyCostData", filename);
                    ItemCostData.LoadMonthlyCostData(this);
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

        public ActionResult RefreshHPU()
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


        public ActionResult LoadYieldRawData()
        {
            try
            {
                YieldRawData.LoadData(this);
            }
            catch (Exception ex) { }
            return View("HeartBeat");
        }

        public ActionResult SendHydraWarningEmail()
        {
            HYDRASummary.SendHydraWarningEmail(this);
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



        public ActionResult LoadPostRevenueData()
        {
            ModuleRevenue.LoadPostPrice(this);
            return View("HeartBeat");
        }

        public ActionResult LoadPostCostData()
        {
            ModuleRevenue.LoadPostCost(this);
            return View("HeartBeat");
        }

        public ActionResult CheckRevenue()
        {
            //var costdict = ItemCostData.RetrieveStandardCost();
            //var ret = ModuleRevenue.GetRevenueList("2019-07-01 00:00:00", "Tunable XFP Gen2", costdict,6.99);
            //var qret = ModuleRevenue.ToQuartRevenue(ret);
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
            WaferData.LoadWaferData("151723-10", vcselpndict);

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
            var dbs = new List<string>();
            dbs.Add("MES");
            dbs.Add("ATE");
            ViewBag.dblist = CreateSelectList(dbs, "");
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
            var dbstr = Request.Form["dbstr"];

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
                var pnlist = pn.Split(new string[] { ",", ";"," " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                pncond = "('" + string.Join("','", pnlist) + "')";
                title = pn;
            }

            var retdata = JOVM.QueryJO(title,pncond, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"),this, dbstr);
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
                    chartdata = retdata[2],
                    datafrom = dbstr
                };
            }
            return ret;
        }

        public ActionResult JOProgress(string jo)
        {
            ViewBag.defjo = "";
            if (!string.IsNullOrEmpty(jo))
            { ViewBag.defjo = jo; }
            return View();
        }

        public JsonResult JOProgressData()
        {
            var jo = Request.Form["jo"];
            var chartdata = JOVM.QueryJOProcess(jo,this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                chartdata = chartdata
            };
            return ret;
        }

        public ActionResult LoadUnitCost()
        {
            ExternalDataCollector.LoadProductCostData(this);
            return View("Index");
        }

        public ActionResult LoadFCost()
        {
            FCostModel.LoadDataFromFDB("Q1FY20", "1178834");
            return View("Index");
        }

        public ActionResult QueryCDP()
        {
            var sql = "select  * from wuxi.dc_final_tx where moduleserialnum = 'X2AA5DY'";
            var dbret = DBUtility.ExeCDPSqlWithRes(sql);
            return View("Index");
        }

        public ActionResult RefreshFCost()
        {
            ProductCostVM.RefreshFCost(this);
            return View("Index");
        }



        //public ActionResult ForecastAccuracy()
        //{
        //    var starttime = "2018-11-01 00:00:00";
        //    var endtime = "2019-11-01 00:00:00";
        //    var activeseries = PNBUMap.GetActiveSeries(starttime);
        //    foreach (var ser in activeseries)
        //    {
        //        ser.Accuracy = ShipForcastData.GetSeriesAccuracyVal(ser.Series, starttime, endtime);
        //    }
        //    return View("Index");
        //}

        public ActionResult LoadCostData()
        {
            ItemCostData.LoadCostData(this);
            return View("HeartBeat");
        }

        public ActionResult LoadModuleRevenueData()
        {
            ModuleRevenue.LoadData(this);
            return View("HeartBeat");
        }

        public ActionResult LoadMonthlyCostData()
        {
            ItemCostData.LoadMonthlyCostData(this);
            return View("HeartBeat");
        }

        public ActionResult LoadPNBUData()
        {
            PNBUMap.LoadPNBUData(this);
            return View("Index");
        }

        public ActionResult LoadForcastData()
        {
            ShipForcastData.LoadData(this);
            return View("Index");
        }

        public ActionResult LoadOSASeriesData()
        {
            OSASeriesData.LoadData(this);
            return View("Index");
        }

        public ActionResult LoadWSSForcast()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var forcastfolder = syscfg["FORCASTDATA"];
            ShipForcastData.LoadWSSData(this, forcastfolder);
            return View("Index");
        }

        public ActionResult LoadPLMData()
        {
            PLMMatrix.LoadData(this);
            return View("Index");
        }

    }
}