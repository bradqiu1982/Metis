using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;
using System.Web.Routing;
using System.Net;

namespace Prism.Controllers
{
    public class ScrapController : Controller
    {
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

        public ActionResult DepartmentScrap(string defyear, string defqrt, string defdepartment)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }


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
            if (!string.IsNullOrEmpty(defdepartment))
            {
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
                var minYrate = 0.0;
                var maxYrate = 0.0;
                var maxYoutput = 0.0;

                var xlist = new List<string>();
                var generalscrap = new List<double>();
                var nonchinascrap = new List<double>();
                var totlescrap = new List<double>();

                var output = new List<double>();
                var generalscraprate = new List<double>();
                var nonchinascraprate = new List<double>();
                var totlescraprate = new List<double>();


                foreach (var item in sumscraplist)
                {
                    xlist.Add(item.key);
                    generalscrap.Add(Math.Round(item.generalscrap, 2));
                    nonchinascrap.Add(Math.Round(item.nonchinascrap, 2));
                    var totle = Math.Round(item.generalscrap, 2) + Math.Round(item.nonchinascrap, 2);
                    totlescrap.Add(totle);

                    output.Add(Math.Round(item.output, 2));
                    var trate = Math.Round(totle / item.output * 100.0, 2);
                    if (trate > maxYrate)
                    { maxYrate = trate + 1.0; }
                    if (item.output > maxYoutput)
                    { maxYoutput = item.output; }

                    totlescraprate.Add(trate);

                    var grate = Math.Round(item.generalscrap / item.output * 100.0, 2);
                    if (minYrate > grate)
                    { minYrate = grate-1; }
                    generalscraprate.Add(grate);
                    nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                }
                var chartlist = new List<object>();
                chartlist.Add(new { name = "Non-China Scrap Rate", data = nonchinascraprate, type = "line", yAxis = 0, color = "#90ed7d" });
                chartlist.Add(new { name = "General Scrap Rate", data = generalscraprate, type = "line", yAxis = 0, color = "#f7a35c" });
                chartlist.Add(new { name = "Totle Scrap Rate", data = totlescraprate, type = "line", yAxis = 0, color = "#8085e9" });
                chartlist.Add(new { name = "Non-China Scrap", data = nonchinascrap, type = "column", yAxis = 1, color = "#90ed7d" });
                chartlist.Add(new { name = "General Scrap", data = generalscrap, type = "column", yAxis = 1, color = "#f7a35c" });
                chartlist.Add(new { name = "Totle Scrap", data = totlescrap, type = "column", yAxis = 1, color = "#8085e9" });
                chartlist.Add(new { name = "Output", data = output, type = "column", yAxis = 1, color = "#f15c80" });

                var onepjobj = new
                {
                    id = "department_line",
                    title = "Department " + fyear + " " + fquarter + " SCRAP",
                    xAxis = new { data = xlist },
                    minYrate = minYrate,
                    maxYrate = maxYrate,
                    bugetscraprate = new { name = "Max", color = "#C9302C", data = maxYrate * 2, style = "solid" },
                    bugetscrapval = new { name = "Max", color = "#C9302C", data = maxYoutput * 2, style = "dash" },
                    chartlist = chartlist,
                    url = "/Scrap/CostCenterOfOneDepart?x="
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
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

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
                    pjcodes += String.Join(";", pjcodelist) + ";";
                }
            }

            var dict = new RouteValueDictionary();
            dict.Add("defyear", defyear);
            dict.Add("defqrt", defqrt);
            dict.Add("defpj", pjcodes);
            return RedirectToAction("CostCenterScrap", "Scrap", dict);
        }

        public ActionResult CostCenterScrap(string defyear, string defqrt, string defpj)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

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
                if (wklist.Count == 0)
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
                    var minYrate = 0.0;
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
                        { maxYrate = bugetrate + 1.0; }

                        if (boutput > maxYoutput)
                        { maxYoutput = boutput; }
                    }


                    var xlist = new List<string>();
                    var generalscrap = new List<double>();
                    var nonchinascrap = new List<double>();
                    var totlescrap = new List<double>();

                    var output = new List<double>();
                    var generalscraprate = new List<double>();
                    var nonchinascraprate = new List<double>();
                    var totlescraprate = new List<double>();

                    var bugetscraprate = new { name = "Max", color = "#C9302C", data = 0.0, style = "solid" };
                    if (bugetrate != 0.0)
                    { bugetscraprate = new { name = "Max", color = "#C9302C", data = bugetrate, style = "solid" }; }

                    foreach (var item in sumscraplist)
                    {
                        xlist.Add(item.key);
                        generalscrap.Add(Math.Round(item.generalscrap, 2));
                        nonchinascrap.Add(Math.Round(item.nonchinascrap, 2));
                        var totle = Math.Round(item.generalscrap, 2) + Math.Round(item.nonchinascrap, 2);
                        totlescrap.Add(totle);

                        output.Add(Math.Round(item.output, 2));
                        var trate = Math.Round(totle / item.output * 100.0, 2);
                        if (trate > maxYrate)
                        { maxYrate = trate + 1.0; }
                        if (item.output > maxYoutput)
                        { maxYoutput = item.output; }

                        totlescraprate.Add(trate);

                        var grate = Math.Round(item.generalscrap / item.output * 100.0, 2);
                        if (minYrate > grate)
                        { minYrate = grate-1; }
                        generalscraprate.Add(grate);
                        nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                    }

                    var bugetscrapval = new { name = "Max", color = "#C9302C", data = maxYoutput * 2, style = "dash" };
                    if (bscrap != 0.0)
                    { bugetscrapval = new { name = "Max", color = "#C9302C", data = Math.Round(bscrap, 2), style = "dash" }; }

                    var title = co + " " + fyear + " " + fquarter + " SCRAP";
                    if (copdmap.ContainsKey(co))
                    {
                        title = co + " " + copdmap[co] + " " + fyear + " " + fquarter + " SCRAP";
                    }

                    var chartlist = new List<object>();
                    chartlist.Add(new { name = "Non-China Scrap Rate", data = nonchinascraprate, type = "line", yAxis = 0, color = "#90ed7d" });
                    chartlist.Add(new { name = "General Scrap Rate", data = generalscraprate, type = "line", yAxis = 0, color = "#f7a35c" });
                    chartlist.Add(new { name = "Totle Scrap Rate", data = totlescraprate, type = "line", yAxis = 0, color = "#8085e9" });
                    chartlist.Add(new { name = "Non-China Scrap", data = nonchinascrap, type = "column", yAxis = 1, color = "#90ed7d" });
                    chartlist.Add(new { name = "General Scrap", data = generalscrap, type = "column", yAxis = 1, color = "#f7a35c" });
                    chartlist.Add(new { name = "Totle Scrap", data = totlescrap, type = "column", yAxis = 1, color = "#8085e9" });
                    chartlist.Add(new { name = "Output", data = output, type = "column", yAxis = 1, color = "#f15c80" });

                    var onepjobj = new
                    {
                        id = title.Replace(" ", "_") + "_line",
                        title = title,
                        xAxis = new { data = xlist },
                        minYrate = minYrate,
                        maxYrate = maxYrate,
                        bugetscraprate = bugetscraprate,
                        bugetscrapval = bugetscrapval,
                        chartlist = chartlist,
                        url = "/Scrap/ProductScrap?defco=" + co + "&x="
                    };

                    scrapratearray.Add(onepjobj);

                }//end if
            }//end foreach

            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                scrapratearray = scrapratearray
            };
            return ret;
        }

        public ActionResult ProductScrap(string defyear, string defqrt, string defco)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

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
                        var minYrate = 0.0;
                        var maxYrate = 0.0;
                        var maxYoutput = 0.0;

                        var xlist = new List<string>();
                        var generalscrap = new List<double>();
                        var nonchinascrap = new List<double>();
                        var totlescrap = new List<double>();
                        var output = new List<double>();
                        var generalscraprate = new List<double>();
                        var nonchinascraprate = new List<double>();
                        var totlescraprate = new List<double>();

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
                            var totle = Math.Round(item.generalscrap, 2) + Math.Round(item.nonchinascrap, 2);
                            totlescrap.Add(totle);

                            output.Add(Math.Round(item.output, 2));
                            var trate = Math.Round(totle / item.output * 100.0, 2);
                            if (trate > maxYrate)
                            { maxYrate = trate + 1.0; }
                            if (item.output > maxYoutput)
                            { maxYoutput = item.output; }

                            totlescraprate.Add(trate);

                            var grate = Math.Round(item.generalscrap / item.output * 100.0, 2);
                            if (minYrate > grate)
                            { minYrate = grate-1; }
                            generalscraprate.Add(grate);
                            nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                        }

                        var bugetscrapval = new { name = "Max", color = "#C9302C", data = maxYoutput * 2, style = "dash" };
                        if (bscrap != 0.0)
                        { bugetscrapval = new { name = "Max", color = "#C9302C", data = Math.Round(bscrap, 2), style = "dash" }; }

                        var title = pd.Replace(".","") + " " + fyear + " " + fquarter + " SCRAP";

                        var chartlist = new List<object>();
                        chartlist.Add(new { name = "Non-China Scrap Rate", data = nonchinascraprate, type = "line", yAxis = 0, color = "#90ed7d" });
                        chartlist.Add(new { name = "General Scrap Rate", data = generalscraprate, type = "line", yAxis = 0, color = "#f7a35c" });
                        chartlist.Add(new { name = "Totle Scrap Rate", data = totlescraprate, type = "line", yAxis = 0, color = "#8085e9" });
                        chartlist.Add(new { name = "Non-China Scrap", data = nonchinascrap, type = "column", yAxis = 1, color = "#90ed7d" });
                        chartlist.Add(new { name = "General Scrap", data = generalscrap, type = "column", yAxis = 1, color = "#f7a35c" });
                        chartlist.Add(new { name = "Totle Scrap", data = totlescrap, type = "column", yAxis = 1, color = "#8085e9" });
                        chartlist.Add(new { name = "Output", data = output, type = "column", yAxis = 1, color = "#f15c80" });

                        var onepjobj = new
                        {
                            id = title.Replace(" ", "_") + "_line",
                            title = title,
                            xAxis = new { data = xlist },
                            minYrate = minYrate,
                            maxYrate = maxYrate,
                            bugetscraprate = bugetscraprate,
                            bugetscrapval = bugetscrapval,
                            chartlist = chartlist,
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



        public ActionResult ScrapTrend()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

            return View();
        }

        public JsonResult ScrapTrendData()
        {
            var scrapratearray = new List<object>();
            var syscfg = CfgUtility.GetSysConfig(this);
            var products = syscfg["SCRAPTRENDPD"];

            var productlist = products.Split(new string[] {";"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var prod in productlist)
            {
                //var pnplannermap = PNPlannerCodeMap.RetrieveAllMaps();
                var productdatadict = ScrapData_Base.RetrieveProductDataByPD(prod);
                foreach (var kv in productdatadict)
                {
                    var pd = kv.Key;
                    var onepddata = kv.Value;
                    if (onepddata.Count == 0)
                    { continue; }

                    var sumdata = SCRAPSUMData.GetSumDataFromRawDataByQuarter(onepddata);

                    var qlist = sumdata.Keys.ToList();
                    qlist.Sort(delegate (string obj1, string obj2)
                    {
                        var i1 = QuarterCLA.RetrieveDateFromQuarter(obj1)[0];
                        var i2 = QuarterCLA.RetrieveDateFromQuarter(obj2)[0];
                        return i1.CompareTo(i2);
                    });

                    //var outputiszero = false;
                    var sumscraplist = new List<SCRAPSUMData>();
                    foreach (var q in qlist)
                    {
                        sumscraplist.Add(sumdata[q]);
                    }


                    if (sumscraplist.Count > 0)
                    {
                        var minYrate = 0.0;
                        var maxYrate = 0.0;
                        var maxYoutput = 0.0;

                        var xlist = new List<string>();
                        var generalscrap = new List<double>();
                        var nonchinascrap = new List<double>();
                        var totlescrap = new List<double>();
                        var output = new List<double>();
                        var generalscraprate = new List<double>();
                        var nonchinascraprate = new List<double>();
                        var totlescraprate = new List<double>();

                        foreach (var item in sumscraplist)
                        {
                            xlist.Add(item.key);
                            generalscrap.Add(Math.Round(item.generalscrap, 2));
                            nonchinascrap.Add(Math.Round(item.nonchinascrap, 2));
                            var totle = Math.Round(item.generalscrap, 2) + Math.Round(item.nonchinascrap, 2);
                            totlescrap.Add(totle);

                            output.Add(Math.Round(item.output, 2));
                            var trate = Math.Round(totle / item.output * 100.0, 2);
                            if (trate > maxYrate)
                            { maxYrate = trate + 1.0; }
                            if (item.output > maxYoutput)
                            { maxYoutput = item.output; }

                            totlescraprate.Add(trate);

                            var grate = Math.Round(item.generalscrap / item.output * 100.0, 2);
                            if (minYrate > grate)
                            { minYrate = grate-1; }
                            generalscraprate.Add(grate);
                            nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                        }

                        var bugetscraprate = new { name = "Max", color = "#C9302C", data = maxYrate * 2, style = "solid" };
                        var bugetscrapval = new { name = "Max", color = "#C9302C", data = maxYoutput * 2, style = "dash" };

                        var title = pd + " SCRAP Trend";

                        var chartlist = new List<object>();
                        chartlist.Add(new { name = "Non-China Scrap Rate", data = nonchinascraprate, type = "line", yAxis = 0, color = "#90ed7d" });
                        chartlist.Add(new { name = "General Scrap Rate", data = generalscraprate, type = "line", yAxis = 0, color = "#f7a35c" });
                        chartlist.Add(new { name = "Totle Scrap Rate", data = totlescraprate, type = "line", yAxis = 0, color = "#8085e9" });
                        chartlist.Add(new { name = "Non-China Scrap", data = nonchinascrap, type = "column", yAxis = 1, color = "#90ed7d" });
                        chartlist.Add(new { name = "General Scrap", data = generalscrap, type = "column", yAxis = 1, color = "#f7a35c" });
                        chartlist.Add(new { name = "Totle Scrap", data = totlescrap, type = "column", yAxis = 1, color = "#8085e9" });
                        chartlist.Add(new { name = "Output", data = output, type = "column", yAxis = 1, color = "#f15c80" });

                        var onepjobj = new
                        {
                            id = title.Replace(" ", "_") + "_line",
                            title = title,
                            xAxis = new { data = xlist },
                            minYrate = minYrate,
                            maxYrate = maxYrate,
                            bugetscraprate = bugetscraprate,
                            bugetscrapval = bugetscrapval,
                            chartlist = chartlist,
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

    }
}