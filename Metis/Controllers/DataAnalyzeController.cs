using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Metis.Models;
using System.Web.Routing;

namespace Metis.Controllers
{
    public class DataAnalyzeController : Controller
    {
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
                if (dp.Contains("LNCD")) continue;

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
                    generalscraprate.Add(Math.Round(item.generalscrap / item.output * 100.0, 2));
                    nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                }

                var onepjobj = new
                {
                    id = "department_line",
                    title = "Department " + fyear + " " + fquarter + " SCRAP",
                    xAxis = new { data = xlist },
                    maxdata = new { name = "Max", color = "#C9302C", data = 100, style = "solid" },
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
            var pjcodelist = ScrapData_Base.RetrievePJCodeByDepartment(x, defyear, defqrt);
            
            if (pjcodelist.Count > 0)
            {
                pjcodes = String.Join(";", pjcodelist);
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



        public JsonResult CostCenterScrapRateData()
        {
            var fyear = Request.Form["fyear"];
            var fquarter = Request.Form["fquarter"];
            var pj_no = Request.Form["pj_no"];
            var scrapratearray = new List<object>();

            if (string.Compare(fquarter, FinanceQuarter.CURRENTQx) == 0)
            {
                var now = DateTime.Now;
                fyear = ExternalDataCollector.GetFYearByTime(now);
                fquarter = ExternalDataCollector.GetFQuarterByTime(now);
            }

            var pjnolist = pj_no.Split(new string[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var pj in pjnolist)
            {
                var wklist = ScrapData_Base.RetrieveWeekListByPJ(pj, fyear, fquarter);
                if(wklist.Count == 0)
                { continue; }
                
                var onepjdata = ScrapData_Base.RetrievePJCodeQuarterData(pj, fyear, fquarter);
                var sumdata = SCRAPSUMData.GetSumDataFromRawDataByWeek(onepjdata);

                var outputiszero = false;
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
                        outputiszero = true;
                        break;
                    }
                    sumscraplist.Add(tempvm);
                }//end for

                if (outputiszero)
                { continue; }

                var xlist = new List<string>();
                var generalscrap = new List<double>();
                var nonchinascrap = new List<double>();
                var output = new List<double>();
                var generalscraprate = new List<double>();
                var nonchinascraprate = new List<double>();

                foreach (var item in sumscraplist)
                {
                    xlist.Add(item.key);
                    generalscrap.Add(Math.Round(item.generalscrap,2));
                    nonchinascrap.Add(Math.Round(item.nonchinascrap,2));
                    output.Add(Math.Round(item.output,2));
                    generalscraprate.Add(Math.Round(item.generalscrap / item.output * 100.0, 2));
                    nonchinascraprate.Add(Math.Round(item.nonchinascrap / item.output * 100.0, 2));
                }

                var onepjobj = new
                {
                    id = pj.Replace(" ", "_") + "_line",
                    title = pj +" "+fyear+" "+fquarter+" SCRAP",
                    xAxis = new { data = xlist },
                    maxdata = new { name = "Max", color = "#C9302C", data = 40, style = "solid" },
                    generalscraprate = new { name = "General Scrap Rate", data = generalscraprate },
                    nonchinascraprate = new { name = "Non-China Scrap Rate", data = nonchinascraprate },
                    generalscrap = new { name="General Scrap",data = generalscrap },
                    nonchinascrap = new { name = "Non-China Scrap", data = nonchinascrap },
                    output = new { name = "Output", data = output },
                    url=""
                };

                scrapratearray.Add(onepjobj);
            }//end foreach

            var ret = new JsonResult();
            ret.Data = new {
                success = true,
                scrapratearray = scrapratearray
            };
            return ret;
        }

        public ActionResult ProductScrapScrap(string defyear, string defqrt, string defpj)
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

            var syscfg = CfgUtility.GetSysConfig(this);
            ViewBag.carepjlist = "";
            if (!string.IsNullOrEmpty(defpj))
            { ViewBag.carepjlist = defpj; }

            return View();
        }

        public JsonResult ProjectNumAutoCompelete()
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