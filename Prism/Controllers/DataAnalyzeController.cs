using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;
using System.Web.Routing;

namespace Prism.Controllers
{
    public class DataAnalyzeController : Controller
    {
        public ActionResult HPUTrend()
        {
            ViewBag.defaultserial = "";
            var syscfg = CfgUtility.GetSysConfig(this);
            if (syscfg.ContainsKey("HPUTRENDSERIAL"))
            { ViewBag.defaultserial = syscfg["HPUTRENDSERIAL"]; }
            return View();
        }

        private List<double> GetHPUDataBySerial(string serial,List<string> xaxis,List<HPUMainData> srcdata)
        {
            var yieldhpulist = new List<double>();
            if (serial.Contains("-FG") || serial.Contains("- FG"))
            {
                foreach (var q in xaxis)
                {
                    var temphpu = 0.0;
                    foreach (var data in srcdata)
                    {
                        if (string.Compare(data.Serial.Replace("-SFG", "").Replace("- SFG", "").Replace("-FG", "").Replace("- FG", ""), serial.Replace("-FG", "").Replace("- FG", "")) == 0
                            && string.Compare(data.Quarter, q) == 0
                            && (data.Serial.Contains("-SFG") || data.Serial.Contains("- SFG")))
                        {
                            temphpu += 2.0 * Convert.ToDouble(data.YieldHPU);
                        }
                        if (string.Compare(data.Serial.Replace("-SFG", "").Replace("- SFG", "").Replace("-FG", "").Replace("- FG", ""), serial.Replace("-FG", "").Replace("- FG", "")) == 0
                            && string.Compare(data.Quarter, q) == 0
                            && (data.Serial.Contains("-FG") || data.Serial.Contains("- FG")))
                        {
                            temphpu += Convert.ToDouble(data.YieldHPU);
                        }
                    }
                    yieldhpulist.Add(Math.Round(temphpu,4));
                }
            }
            else
            {
                foreach (var q in xaxis)
                {
                    foreach (var data in srcdata)
                    {
                        if (string.Compare(data.Serial, serial) == 0 && string.Compare(data.Quarter, q) == 0)
                        {
                            yieldhpulist.Add(Math.Round(Convert.ToDouble(data.YieldHPU),4));
                        }
                    }
                }
            }

            return yieldhpulist;
        }

        private List<double> GetHPUReduction(List<double> yieldhpulist)
        {
            var hpureduction = new List<double>();
            hpureduction.Add(0.0);
            for (var idx = 1; idx < yieldhpulist.Count; idx++)
            {
                hpureduction.Add(Math.Round((yieldhpulist[idx - 1] - yieldhpulist[idx]) / yieldhpulist[idx - 1] * 100.0, 2));
            }
            return hpureduction;
        }

        public JsonResult HPUTrendData()
        {
            var serial = Request.Form["serial"];
            var srcdata = HPUMainData.RetrieveHPUDataBySerial(serial);
            var syscfg = CfgUtility.GetSysConfig(this);

            var hpuarray = new List<object>();
            try {
                if (srcdata.Count > 0)
                {
                    var seriallist = new List<string>();
                    var serialdict = new Dictionary<string, bool>();
                    foreach (var item in srcdata)
                    {
                        if (!item.Serial.Contains("-SFG") && !item.Serial.Contains("- SFG"))
                        {
                            if (!serialdict.ContainsKey(item.Serial))
                            {
                                serialdict.Add(item.Serial,true);
                                seriallist.Add(item.Serial);
                            }
                        }
                    }//get serials

                    foreach (var sitem in seriallist)
                    {
                        var xaxis = new List<string>();
                        foreach (var data in srcdata)
                        {
                            if (string.Compare(data.Serial, sitem) == 0)
                            {
                                xaxis.Add(data.Quarter);
                            }
                        }//get quarter x list

                        if (xaxis.Count == 1)
                        { continue; }

                        var yieldhpulist = GetHPUDataBySerial(sitem, xaxis, srcdata);
                        var hpureduction = GetHPUReduction(yieldhpulist);
                        

                        var maxhpu = 0.0;
                        foreach (var yhp in yieldhpulist)
                        { if (yhp > maxhpu) { maxhpu = yhp; } }

                        var maxhpureduction = 5.0;
                        foreach (var hrd in hpureduction)
                        { if (hrd > maxhpureduction) { maxhpureduction = hrd; } }
                        var minhpureduction = 0.0;
                        foreach (var hrd in hpureduction)
                        { if (hrd < minhpureduction) { minhpureduction = hrd; } }

                        var defaultguideline = 5.0;
                        if (syscfg.ContainsKey(sitem + "-GUIDELINE"))
                        {
                            defaultguideline = Convert.ToDouble(syscfg[sitem + "-GUIDELINE"]);
                        }

                        var hpuguideline = new { name = "HPU Reduction Guideline", color = "#C9302C", data = defaultguideline, style = "dash" };

                        var columncolors = new List<string>();
                        foreach (var hrd in hpureduction)
                        {
                            if (hrd < defaultguideline)
                            { columncolors.Add("#cc044d"); }
                            else
                            { columncolors.Add("#12cc92"); }
                        }

                        var title = sitem.Replace("-FG", "").Replace("- FG", "") + " HPU";

                        var oneobj = new
                        {
                            id = sitem.Replace(" ", "_") + "_line",
                            title = title,
                            xAxis = new { data = xaxis },
                            maxhpu = maxhpu,
                            maxhpureduction = maxhpureduction,
                            hpuguideline = hpuguideline,
                            columncolors = columncolors,
                            yieldhpu = new { name = "Yield HPU", data = yieldhpulist },
                            hpureduction = new { name = "HPU Reduction", data = hpureduction },
                            url = "/DataAnalyze/SerialHPU?defaultserial=" + sitem.Split(new string[] { "-FG", "- FG" },StringSplitOptions.RemoveEmptyEntries)[0]
                        };

                        hpuarray.Add(oneobj);

                    }//get chart data from each serial

                }
            } catch (Exception ex) { }




            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                hpuarray = hpuarray
            };
            return ret;
        }

        public ActionResult DepartmentHPU()
        {
            var productlines = HPUMainData.GetAllProductLines();
            ViewBag.productlines = CreateSelectList(productlines, "");

            var quarterlist = HPUMainData.GetAllQuarters();
            quarterlist.Insert(0, FinanceQuarter.CURRENTQx);
            ViewBag.quarterlist = CreateSelectList(quarterlist, "");

            return View();
        }

        public JsonResult DepartmentHPUData()
        {
            var productline = Request.Form["pdline"];
            var fquarter = Request.Form["quarter"];

            if (string.Compare(fquarter, FinanceQuarter.CURRENTQx) == 0)
            {
                var now = DateTime.Now;
                var year = ExternalDataCollector.GetFYearByTime(now);
                var quarter = ExternalDataCollector.GetFQuarterByTime(now);
                fquarter = year + " " + quarter;
            }

            var data = HPUMainData.RetrieveHPUData(productline,fquarter);
            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                data = data
            };
            return ret;
        }

        public ActionResult SerialHPU(string defaultserial)
        {
            ViewBag.defaultserial = "";
            if (!string.IsNullOrEmpty(defaultserial))
            {
                ViewBag.defaultserial = defaultserial;
            }
            return View();
        }

        public JsonResult GetAllSerial()
        {
            var slist = HPUMainData.RetrieveAllSerial();
            var ret = new JsonResult();
            ret.Data = new
            {
                data = slist
            };
            return ret;
        }

        public JsonResult SerialHPUData()
        {
            var serial = Request.Form["serial"];
            var data = HPUMainData.RetrieveHPUDataBySerial(serial);
            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                data = data
            };
            return ret;
        }

        public ActionResult PNHPU(string PNLink)
        {
            ViewBag.pnlink = "";
            if (!string.IsNullOrEmpty(PNLink))
            { ViewBag.pnlink = PNLink; }
            return View();
        }

        public JsonResult PNHPUPNLinkList()
        {
            var pnlist = PNHPUData.RetrievePNLinkList();
            var ret = new JsonResult();
            ret.Data = new {
                data = pnlist
            };
            return ret;
        }

        public JsonResult GetPNHPUData()
        {
            var pnlink = Request.Form["pnlink"];
            var data = PNHPUData.RetrieveHPUData(pnlink);
            var title = new List<string>();
            if (data.Count > 0)
            {
                title.AddRange(data[0]);
                data.RemoveAt(0);
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                title = title,
                data = data
            };
            return ret;
        }

        public ActionResult DepartmentScrap(string defyear, string defqrt,string defdepartment)
        {
            var year = "";
            if (!string.IsNullOrEmpty(defyear))
            { year = defyear; }
            var qrt = "";
            if (!string.IsNullOrEmpty(defqrt))
            { qrt = defqrt; }

            var fquarters = new List<string>();
            fquarters.Add(FinanceQuarter.CURRENTQx);
            fquarters.Add(FinanceQuarter.Q1);
            fquarters.Add(FinanceQuarter.Q2);
            fquarters.Add(FinanceQuarter.Q3);
            fquarters.Add(FinanceQuarter.Q4);
            ViewBag.fquarterlist = CreateSelectList(fquarters, qrt);

            var years = ScrapData_Base.GetYearList();
            ViewBag.fyearlist = CreateSelectList(years, year);

            ViewBag.Department = "";
            if (!string.IsNullOrEmpty(defdepartment)) {
                ViewBag.Department = defdepartment;
            }

            return View();
        }

        public JsonResult DepartmentScrapRateData()
        {
            var fyear = Request.Form["fyear"];
            var fquarter = Request.Form["fquarter"];
            var department = Request.Form["department"];
            var scrapratearray = new List<object>();

            if (string.Compare(fquarter, FinanceQuarter.CURRENTQx) == 0)
            {
                var now = DateTime.Now;
                fyear = ExternalDataCollector.GetFYearByTime(now);
                fquarter = ExternalDataCollector.GetFQuarterByTime(now);
            }

            var departmentlist = new List<string>();
            if (!string.IsNullOrEmpty(department))
            {
                departmentlist = department.Split(new string[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                departmentlist = ScrapData_Base.RetrieveDPByTime(fyear, fquarter);
            }

            var sumscraplist = new List<SCRAPSUMData>();
            foreach (var dp in departmentlist)
            {
                //if (dp.Contains("LNCD")) continue;

                var onepjdata = ScrapData_Base.RetrieveDPQuarterData(dp, fyear, fquarter);
                if (onepjdata.Count == 0)
                { continue; }
                var sdata = SCRAPSUMData.GetSumDataFromRawDataByDP(onepjdata);
                if (sdata.output == 0.0)
                { continue; }

                sumscraplist.Add(sdata);
            }//end foreach

            if (sumscraplist.Count > 0)
            {
                var maxYrate = 0.0;
                var maxYoutput = 0.0;

                var xlist = new List<string>();
                var generalscrap = new List<double>();
                var nonchinascrap = new List<double>();
                var output = new List<double>();
                var generalscraprate = new List<double>();
                var nonchinascraprate = new List<double>();

                foreach (var item in sumscraplist)
                {
                    xlist.Add(item.key);
                    generalscrap.Add(Math.Round(item.generalscrap, 2));
                    nonchinascrap.Add(Math.Round(item.nonchinascrap, 2));
                    output.Add(Math.Round(item.output, 2));
                    var grate = Math.Round(item.generalscrap / item.output * 100.0, 2);
                    if (grate > maxYrate)
                    { maxYrate = grate + 1.0; }
                    if (item.output > maxYoutput)
                    { maxYoutput = item.output; }

                    generalscraprate.Add(grate);
                    nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                }

                var onepjobj = new
                {
                    id = "department_line",
                    title = "Department " + fyear + " " + fquarter + " SCRAP",
                    xAxis = new { data = xlist },
                    maxYrate = maxYrate,
                    bugetscraprate = new { name = "Max", color = "#C9302C", data = maxYrate*2, style = "solid" },
                    bugetscrapval = new { name = "Max", color = "#C9302C", data = maxYoutput*2, style = "dash" },
                    generalscraprate = new { name = "General Scrap Rate", data = generalscraprate },
                    nonchinascraprate = new { name = "Non-China Scrap Rate", data = nonchinascraprate },
                    generalscrap = new { name = "General Scrap", data = generalscrap },
                    nonchinascrap = new { name = "Non-China Scrap", data = nonchinascrap },
                    output = new { name = "Output", data = output },
                    url = "/DataAnalyze/CostCenterOfOneDepart?x="
                };
                scrapratearray.Add(onepjobj);
            }


            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                scrapratearray = scrapratearray
            };
            return ret;
        }

        public ActionResult CostCenterOfOneDepart(string x, string defyear, string defqrt)
        {
            if (string.Compare(defqrt, FinanceQuarter.CURRENTQx) == 0)
            {
                var now = DateTime.Now;
                defyear = ExternalDataCollector.GetFYearByTime(now);
                defqrt = ExternalDataCollector.GetFQuarterByTime(now);
            }

            var pjcodes = "";
            var xlist = x.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var ix in xlist)
            {
                var pjcodelist = ScrapData_Base.RetrievePJCodeByDepartment(ix, defyear, defqrt);
                if (pjcodelist.Count > 0)
                {
                    pjcodes += String.Join(";", pjcodelist)+";";
                }
            }

            var dict = new RouteValueDictionary();
            dict.Add("defyear", defyear);
            dict.Add("defqrt", defqrt);
            dict.Add("defpj", pjcodes);
            return RedirectToAction("CostCenterScrap", "DataAnalyze", dict);
        }

        public ActionResult CostCenterScrap(string defyear,string defqrt,string defpj)
        {
            var year = "";
            if (!string.IsNullOrEmpty(defyear))
            { year = defyear; }
            var qrt = "";
            if (!string.IsNullOrEmpty(defqrt))
            { qrt = defqrt; }

            var fquarters = new List<string>();
            fquarters.Add(FinanceQuarter.CURRENTQx);
            fquarters.Add(FinanceQuarter.Q1);
            fquarters.Add(FinanceQuarter.Q2);
            fquarters.Add(FinanceQuarter.Q3);
            fquarters.Add(FinanceQuarter.Q4);
            ViewBag.fquarterlist = CreateSelectList(fquarters,qrt);

            var years = ScrapData_Base.GetYearList();
            ViewBag.fyearlist = CreateSelectList(years, year);

            var syscfg = CfgUtility.GetSysConfig(this);
            ViewBag.carepjlist = "";
            if (!string.IsNullOrEmpty(defpj))
            { ViewBag.carepjlist = defpj; }
            else
            {
                if (syscfg.ContainsKey("SCRAPFOCUSPJ"))
                {
                    ViewBag.carepjlist = syscfg["SCRAPFOCUSPJ"];
                }
            }

            return View();
        }


        private static double ConvertToDouble(string val)
        {
            try
            {
                return Math.Round(Convert.ToDouble(val), 2);
            }
            catch (Exception ex) { return 0.0; }
        }

        public JsonResult CostCenterScrapRateData()
        {
            var fyear = Request.Form["fyear"];
            var fquarter = Request.Form["fquarter"];
            var costcenters = Request.Form["costcenter"];
            var scrapratearray = new List<object>();

            if (string.Compare(fquarter, FinanceQuarter.CURRENTQx) == 0)
            {
                var now = DateTime.Now;
                fyear = ExternalDataCollector.GetFYearByTime(now);
                fquarter = ExternalDataCollector.GetFQuarterByTime(now);
            }

            var iebugetdict = IEScrapBuget.RetrieveDataCentDict(fyear, fquarter);
            var copdmap = ScrapData_Base.CostCentProductMap();

            var costcenterlist = costcenters.Split(new string[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var co in costcenterlist)
            {
                var wklist = ScrapData_Base.RetrieveWeekListByPJ(co, fyear, fquarter);
                if(wklist.Count == 0)
                { continue; }
                
                var onepjdata = ScrapData_Base.RetrieveCostCenterQuarterData(co, fyear, fquarter);
                var sumdata = SCRAPSUMData.GetSumDataFromRawDataByWeek(onepjdata);

                //var outputiszero = false;
                var sumscraplist = new List<SCRAPSUMData>();
                for (var idx = 0; idx < wklist.Count; idx++)
                {
                    var tempvm = new SCRAPSUMData();
                    tempvm.key = wklist[0] + "~" + wklist[idx];

                    for (var jdx = 0; jdx <= idx; jdx++)
                    {
                        var wk = wklist[jdx];
                        if (sumdata.ContainsKey(wk))
                        {
                            tempvm.output += sumdata[wk].output;
                            tempvm.generalscrap += sumdata[wk].generalscrap;
                            tempvm.nonchinascrap += sumdata[wk].nonchinascrap;
                            tempvm.spcortscrap += sumdata[wk].spcortscrap;
                        }
                    }//end for
                    if (tempvm.output == 0.0)
                    {
                        //outputiszero = true;
                        continue;
                    }
                    sumscraplist.Add(tempvm);
                }//end for

                //if (outputiszero)
                //{ continue; }

                if (sumscraplist.Count > 0)
                {
                    var maxYrate = 0.0;
                    var maxYoutput = 0.0;

                    var bugetrate = 0.0;
                    var boutput = 0.0;
                    var bscrap = 0.0;

                    if (iebugetdict.ContainsKey(co))
                    {
                        
                        foreach (var item in iebugetdict[co])
                        {
                            boutput += ConvertToDouble(item.OutPut);
                            bscrap += ConvertToDouble(item.Scrap);
                        }
                        bugetrate = Math.Round(bscrap / boutput * 100.0, 2);

                        if (bugetrate > maxYrate)
                        { maxYrate = bugetrate+1.0; }

                        if (boutput > maxYoutput)
                        { maxYoutput = boutput; }
                    }

                    
                    var xlist = new List<string>();
                    var generalscrap = new List<double>();
                    var nonchinascrap = new List<double>();
                    var output = new List<double>();
                    var generalscraprate = new List<double>();
                    var nonchinascraprate = new List<double>();
                    var bugetscraprate = new { name = "Max", color = "#C9302C", data = 0.0, style = "solid" };
                    if (bugetrate != 0.0)
                    { bugetscraprate = new { name = "Max", color = "#C9302C", data = bugetrate, style = "solid" }; }

                    foreach (var item in sumscraplist)
                    {
                        xlist.Add(item.key);
                        generalscrap.Add(Math.Round(item.generalscrap,2));
                        nonchinascrap.Add(Math.Round(item.nonchinascrap,2));
                        output.Add(Math.Round(item.output,2));
                        var grate = Math.Round(item.generalscrap / item.output * 100.0, 2);
                        if (grate > maxYrate)
                        { maxYrate = grate + 1.0; }
                        if (item.output > maxYoutput)
                        { maxYoutput = item.output; }

                        generalscraprate.Add(grate);
                        nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                    }

                    var bugetscrapval = new { name = "Max", color = "#C9302C", data = maxYoutput * 2, style = "dash" };
                    if (bscrap != 0.0)
                    { bugetscrapval = new { name = "Max", color = "#C9302C", data = Math.Round(bscrap,2), style = "dash" }; }

                    var title = co + " " + fyear + " " + fquarter + " SCRAP";
                    if (copdmap.ContainsKey(co))
                    {
                        title = co+" "+copdmap[co] + " " + fyear + " " + fquarter + " SCRAP";
                    }

                    var onepjobj = new
                    {
                        id = co.Replace(" ", "_") + "_line",
                        title = title,
                        xAxis = new { data = xlist },
                        maxYrate = maxYrate,
                        bugetscraprate = bugetscraprate,
                        bugetscrapval = bugetscrapval,
                        generalscraprate = new { name = "General Scrap Rate", data = generalscraprate },
                        nonchinascraprate = new { name = "Non-China Scrap Rate", data = nonchinascraprate },
                        generalscrap = new { name="General Scrap",data = generalscrap },
                        nonchinascrap = new { name = "Non-China Scrap", data = nonchinascrap },
                        output = new { name = "Output", data = output },
                        url= "/DataAnalyze/ProductScrap?defco="+co+"&x="
                    };

                    scrapratearray.Add(onepjobj);

                }//end if
            }//end foreach

            var ret = new JsonResult();
            ret.Data = new {
                success = true,
                scrapratearray = scrapratearray
            };
            return ret;
        }

        public ActionResult ProductScrap(string defyear, string defqrt, string defco)
        {
            var year = "";
            if (!string.IsNullOrEmpty(defyear))
            { year = defyear; }
            var qrt = "";
            if (!string.IsNullOrEmpty(defqrt))
            { qrt = defqrt; }

            var fquarters = new List<string>();
            fquarters.Add(FinanceQuarter.CURRENTQx);
            fquarters.Add(FinanceQuarter.Q1);
            fquarters.Add(FinanceQuarter.Q2);
            fquarters.Add(FinanceQuarter.Q3);
            fquarters.Add(FinanceQuarter.Q4);
            ViewBag.fquarterlist = CreateSelectList(fquarters, qrt);

            var years = ScrapData_Base.GetYearList();
            ViewBag.fyearlist = CreateSelectList(years, year);

            ViewBag.carepjlist = "";
            if (!string.IsNullOrEmpty(defco))
            { ViewBag.carepjlist = defco; }

            return View();
        }

        private void ProductBugetRate(string co, Dictionary<string, PNPlannerCodeMap> pnplannermap
            , Dictionary<string, List<IEScrapBuget>> iebugetdict, Dictionary<string, SCRAPSUMData> productbuget)
        {
                if (iebugetdict.ContainsKey(co))
                {
                    foreach (var item in iebugetdict[co])
                    {
                        var product = item.PN;
                        if (pnplannermap.ContainsKey(product))
                        {
                            var pnitem = pnplannermap[product];
                            if (!string.IsNullOrEmpty(pnitem.PJName))
                            { product = pnitem.PJName; }
                            else
                            { product = pnitem.PlannerCode; }
                        }

                        if (productbuget.ContainsKey(product))
                        {
                            var tempvm = productbuget[product];
                            tempvm.output += ConvertToDouble(item.OutPut);
                            tempvm.generalscrap += ConvertToDouble(item.Scrap);
                        }
                        else
                        {
                            var tempvm = new SCRAPSUMData();
                            tempvm.output = ConvertToDouble(item.OutPut);
                            tempvm.generalscrap = ConvertToDouble(item.Scrap);
                            productbuget.Add(product, tempvm);
                        }
                    }//end foreach
                }//end if
        }

        public JsonResult ProductScrapRateData()
        {
            var fyear = Request.Form["fyear"];
            var fquarter = Request.Form["fquarter"];
            var costcenters = Request.Form["costcenter"];
            var scrapratearray = new List<object>();

            if (string.Compare(fquarter, FinanceQuarter.CURRENTQx) == 0)
            {
                var now = DateTime.Now;
                fyear = ExternalDataCollector.GetFYearByTime(now);
                fquarter = ExternalDataCollector.GetFQuarterByTime(now);
            }

            var iebugetdict = IEScrapBuget.RetrieveDataCentDict(fyear, fquarter);

            var costcentlist = costcenters.Split(new string[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var co in costcentlist)
            {
                var pnplannermap = PNPlannerCodeMap.RetrieveAllMaps();
                var productbuget = new Dictionary<string, SCRAPSUMData>();
                ProductBugetRate(co, pnplannermap, iebugetdict, productbuget);

                var productdatadict = ScrapData_Base.RetrieveProductQuarterDataFromCoByPNMAP(co, fyear, fquarter, pnplannermap);
                foreach (var kv in productdatadict)
                {
                    var pd = kv.Key;
                    var onepddata = kv.Value;
                    var sumdata = SCRAPSUMData.GetSumDataFromRawDataByWeek(onepddata);

                    var wklist = sumdata.Keys.ToList();
                    if (wklist.Count == 0)
                    { continue; }
                    wklist.Sort(delegate (string obj1, string obj2)
                    {
                        var i1 = Convert.ToInt32(obj1.Substring(obj1.Length - 2));
                        var i2 = Convert.ToInt32(obj2.Substring(obj2.Length - 2));
                        return i1.CompareTo(i2);
                    });

                    //var outputiszero = false;
                    var sumscraplist = new List<SCRAPSUMData>();
                    for (var idx = 0; idx < wklist.Count; idx++)
                    {
                        var tempvm = new SCRAPSUMData();
                        tempvm.key = wklist[0] + "~" + wklist[idx];

                        for (var jdx = 0; jdx <= idx; jdx++)
                        {
                            var wk = wklist[jdx];
                            if (sumdata.ContainsKey(wk))
                            {
                                tempvm.output += sumdata[wk].output;
                                tempvm.generalscrap += sumdata[wk].generalscrap;
                                tempvm.nonchinascrap += sumdata[wk].nonchinascrap;
                                tempvm.spcortscrap += sumdata[wk].spcortscrap;
                            }
                        }//end for

                        if (tempvm.output == 0.0)
                        {
                            continue;
                        }
                        sumscraplist.Add(tempvm);
                    }//end for


                    if (sumscraplist.Count > 0)
                    {
                        var maxYrate = 0.0;
                        var maxYoutput = 0.0;

                        var xlist = new List<string>();
                        var generalscrap = new List<double>();
                        var nonchinascrap = new List<double>();
                        var output = new List<double>();
                        var generalscraprate = new List<double>();
                        var nonchinascraprate = new List<double>();

                        var bugetrate = 0.0;
                        var boutput = 0.0;
                        var bscrap = 0.0;

                        if (productbuget.ContainsKey(pd))
                        {
                            boutput = productbuget[pd].output;
                            bscrap = productbuget[pd].generalscrap;

                            bugetrate = Math.Round(bscrap / boutput * 100.0, 2);
                            if (bugetrate > maxYrate)
                            { maxYrate = bugetrate + 1.0; }

                            if (boutput > maxYoutput)
                            { maxYoutput = boutput; }
                        }

                        var bugetscraprate = new { name = "Max", color = "#C9302C", data = 0.0, style = "solid" };
                        if (bugetrate != 0.0)
                        { bugetscraprate = new { name = "Max", color = "#C9302C", data = bugetrate, style = "solid" }; }

                        foreach (var item in sumscraplist)
                        {
                            xlist.Add(item.key);
                            generalscrap.Add(Math.Round(item.generalscrap, 2));
                            nonchinascrap.Add(Math.Round(item.nonchinascrap, 2));
                            output.Add(Math.Round(item.output, 2));
                            var grate = Math.Round(item.generalscrap / item.output * 100.0, 2);
                            if (grate > maxYrate)
                            { maxYrate = grate + 1.0; }
                            if (item.output > maxYoutput)
                            { maxYoutput = item.output; }

                            generalscraprate.Add(grate);
                            nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                        }

                        var bugetscrapval = new { name = "Max", color = "#C9302C", data = maxYoutput * 2, style = "dash" };
                        if (bscrap != 0.0)
                        { bugetscrapval = new { name = "Max", color = "#C9302C", data = Math.Round(bscrap,2), style = "dash" }; }

                        var onepjobj = new
                        {
                            id = pd.Replace(" ", "_") + "_line",
                            title = pd + " " + fyear + " " + fquarter + " SCRAP",
                            xAxis = new { data = xlist },
                            maxYrate = maxYrate,
                            bugetscraprate = bugetscraprate,
                            bugetscrapval = bugetscrapval,
                            generalscraprate = new { name = "General Scrap Rate", data = generalscraprate },
                            nonchinascraprate = new { name = "Non-China Scrap Rate", data = nonchinascraprate },
                            generalscrap = new { name = "General Scrap", data = generalscrap },
                            nonchinascrap = new { name = "Non-China Scrap", data = nonchinascrap },
                            output = new { name = "Output", data = output },
                            url = ""
                        };

                        scrapratearray.Add(onepjobj);
                    }//end if
                }//end foreach
            }//end foreach

            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                scrapratearray = scrapratearray
            };
            return ret;
        }

        public JsonResult CostCenterAutoCompelete()
        {
            var vlist = ScrapData_Base.GetProjectCodeList();
            var ret = new JsonResult();
            ret.Data = new { data = vlist };
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



    }
}