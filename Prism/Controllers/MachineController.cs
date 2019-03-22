using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;

namespace Prism.Controllers
{
    public class MachineController : Controller
    {
        public ActionResult DepartmentMachine()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "DepartmentMachine"))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        private List<string> RetrieveQuarterFromYield(List<List<MachineRate>> pdms)
        {
            var quarterdict = new Dictionary<string, bool>();
            foreach (var pdm in pdms)
            {
                foreach (var y in pdm)
                {
                    if (!quarterdict.ContainsKey(y.Quarter))
                    {
                        quarterdict.Add(y.Quarter, true);
                    }
                }
            }
            var qlist = quarterdict.Keys.ToList();
            qlist.Sort(delegate (string q1, string q2)
            {
                var qd1 = QuarterCLA.RetrieveDateFromQuarter(q1);
                var qd2 = QuarterCLA.RetrieveDateFromQuarter(q2);
                return qd1[0].CompareTo(qd2[0]);
            });
            return qlist;
        }

        private JsonResult GetMachineTableAndChart(List<List<MachineRate>> pdmachinelist, string title, bool withlink = false)
        {
            var titlelist = new List<object>();
            titlelist.Add(title);
            titlelist.Add("");

            var quarterlist = RetrieveQuarterFromYield(pdmachinelist);
            titlelist.AddRange(quarterlist);


            var chartdatalist = new List<object>();
            var MachineList = new List<object>();
            var WhichTestList = new List<object>();

            var idx = 0;
            var contentlist = new List<object>();
            foreach (var pdm in pdmachinelist)
            {
                if (pdm.Count == 0)
                { continue; }

                var chartxlist = new List<object>();
                var chartseriallist = new List<object>();
                var chartfpydata = new List<object>();
                var chartfydata = new List<object>();

                var linelist = new List<object>();
                if (withlink)
                {
                    linelist.Add("<a href='/Machine/ProductMachine?productfaimly=" + pdm[0].ProductFamily.Replace("+", "%2B") + "' target='_blank'>" + pdm[0].ProductFamily + "</a>");
                }
                else
                {
                    if (string.IsNullOrEmpty(pdm[0].ProjectKey))
                    {
                        linelist.Add(pdm[0].ProductFamily);
                    }
                    else
                    {
                        linelist.Add("<a href='http://wuxinpi.china.ads.finisar.com/Project/ProjectDetail?ProjectKey=" + pdm[0].ProjectKey + "' target='_blank'>" + pdm[0].ProductFamily + "</a>");
                    }
                }

                linelist.Add("<span class='YINPUT'>Machines</span><br><span class='YFPY'>Hours</span><br><span class='YFY'>Rate%</span>");


                var testidx = 0;
                foreach (var qt in quarterlist)
                {
                    var matchidx = 0;
                    var matchflag = false;
                    foreach (var mr in pdm)
                    {
                        if (string.Compare(qt, mr.Quarter, true) == 0)
                        {
                            matchflag = true;
                            break;
                        }
                        matchidx += 1;
                    }

                    if (matchflag && pdm[matchidx].MachineTimeList.Count > 0)
                    {
                        var id2 = idx + "-" + testidx + "-1";
                        var id1 = idx + "-" + testidx + "-0";
                        testidx += 1;

                        linelist.Add("<span class='YINPUT YIELDDATA' myid='" + id1 + "'>" + pdm[matchidx].MachineTimeList.Count + "</span><br><span class='YFPY YIELDDATA' myid='" + id1 + "'>" + (int)(pdm[matchidx].SpendTime/3600) + "</span><br><span class='YFY WTESTDATA' myid='" + id2 + "'>" + pdm[matchidx].Rate + "</span>");
                        pdm[matchidx].MachineTimeList.Sort(delegate (MachineSpendTime obj1, MachineSpendTime obj2) { return obj1.SpendTime.CompareTo(obj2.SpendTime); });

                        MachineList.Add(
                            new
                            {
                                id = id1,
                                mlist = pdm[matchidx].MachineTimeList
                            }
                            );

                        MachineList.Add(
                            new
                            {
                                id = id2,
                                mlist = MachineRate.GetTestMachineRate(pdm[matchidx].MachineTimeList)
                            }
                            );

                        chartxlist.Add(pdm[matchidx].Quarter);
                        chartfpydata.Add(pdm[matchidx].Rate);
                    }
                    else
                    {
                        linelist.Add(" ");
                    }
                }//end foreach

                contentlist.Add(linelist);

                chartseriallist.Add(new
                {
                    name = "Machine Usage Rate",
                    data = chartfpydata
                });

                var chartid = pdm[0].ProductFamily.Replace(".", "").Replace(" ", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";
                chartdatalist.Add(new
                {
                    id = chartid,
                    xlist = chartxlist,
                    series = chartseriallist
                });

                idx += 1;
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                tabletitle = titlelist,
                tablecontent = contentlist,
                chartdatalist = chartdatalist,
                machinelist = MachineList
            };
            ret.MaxJsonLength = Int32.MaxValue;
            return ret;
        }

        public JsonResult DepartmentMachineData()
        {
            var machinelist = MachineRate.RetrieveAllMachine(this);
            return GetMachineTableAndChart(machinelist, "Department", true);
        }

        public ActionResult ProductMachine(string productfaimly)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ProductMachine"))
            {
                return RedirectToAction("Index", "Main");
            }
            ViewBag.productfamily = "";
            if (!string.IsNullOrEmpty(productfaimly))
            { ViewBag.productfamily = productfaimly; }

            return View();
        }

        public JsonResult ProductMachineData()
        {
            var pdf = Request.Form["pdf"];
            var yieldcfg = CfgUtility.LoadYieldConfig(this);
            var orderlist = yieldcfg["YIELDORDER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var sortedlist = new List<List<MachineRate>>();
            var familydict = new Dictionary<string, bool>();

            var pdyieldlist = MachineRate.RetrieveProductMachineByYF(pdf, this);
            foreach (var od in orderlist)
            {
                foreach (var pdy in pdyieldlist)
                {
                    if (pdy[0].ProductFamily.Contains(od))
                    {
                        if (!familydict.ContainsKey(pdy[0].ProductFamily))
                        {
                            familydict.Add(pdy[0].ProductFamily, true);
                            sortedlist.Add(pdy);
                        }
                    }//end if
                }//end foreach
            }//end foreach

            foreach (var pdy in pdyieldlist)
            {
                var match = false;
                foreach (var od in orderlist)
                {
                    if (pdy[0].ProductFamily.Contains(od))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    if (!familydict.ContainsKey(pdy[0].ProductFamily))
                    {
                        familydict.Add(pdy[0].ProductFamily, true);
                        sortedlist.Add(pdy);
                    }
                }
            }

            return GetMachineTableAndChart(sortedlist, "Product");
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

        public ActionResult HydraMachineUsage(string deftester)
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var hydratesters = syscfg["HYDRATESTER"].Split(new string[] {";"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            hydratesters.Sort();
            ViewBag.hydratesterlist = CreateSelectList(hydratesters, deftester);
            return View();
        }



        private object GetHydraChart(string tester,DateTime startdate,List<HYDRASummary> datalist)
        {
            var retdata = HYDRASummary.CollectWorkingStatus(startdate,datalist);
            var workinglist =  (List<HYDRASummary>)retdata[0];
            var pendindlist =  (List<HYDRASummary>)retdata[1];
            var dataarray = new List<object>();
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMilliseconds;

            foreach (var item in workinglist)
            {
                var sm = item.StartDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds+offset;
                var em = item.EndDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds+offset;
                dataarray.Add(new {
                    start= sm,
                    end= em,
                    name= "Working",
                    color = "#082F52"
                });
            }

            foreach (var item in pendindlist)
            {
                var sm = item.StartDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds+offset;
                var em = item.EndDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds+offset;
                dataarray.Add(new
                {
                    start = sm,
                    end = em,
                    name = "Pending",
                    color = "#fa0"
                });
            }

            var series = new List<object>();
            series.Add(new {
                name = "Machine Status",
                data = dataarray
            });

            var id = tester.ToLower().Replace(" ", "_");
            var title = tester + " Machine Running Status";

            return new {
                id = id,
                title = title,
                series = series,
                workinglist = workinglist,
                pendindlist = pendindlist
            };
        }

        public JsonResult HydraMachineUsageData()
        {
            var tester = Request.Form["tester"];
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

                if (startdate < DateTime.Parse("2019-01-17 00:00:00"))
                { startdate = DateTime.Parse("2019-01-17 00:00:00"); }

                if (enddate < DateTime.Parse("2019-01-17 00:00:00"))
                { enddate = DateTime.Now; }
            }
            else
            {
                enddate = DateTime.Now;
                startdate = DateTime.Parse(enddate.AddDays(-7).ToString("yyyy-MM-dd") + " 00:00:00");
                if (startdate < DateTime.Parse("2019-01-17 00:00:00"))
                { startdate = DateTime.Parse("2019-01-17 00:00:00"); }
            }

            var chartlist = new List<object>();
            var hydradatalist = HYDRASummary.RetrieveHydraData(tester, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            if (hydradatalist.Count > 0)
            {
                chartlist.Add(GetHydraChart(tester,startdate,hydradatalist));
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new {
                success = true,
                chartlist = chartlist
            };
            return ret;
        }


        public ActionResult HydraMACRate()
        {
            var weeks = new string[] { "WEEKS", "4", "8", "12", "16", "20", "24"};
            var weeklist = CreateSelectList(weeks.ToList(), "");
            weeklist[0].Disabled = true;
            weeklist[0].Selected = true;
            ViewBag.weeklist = weeklist;

            return View();
        }

        private Dictionary<string, List<HYDRASummary>> HydraWeeklyRate()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var hydratesters = syscfg["HYDRATESTER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            hydratesters.Sort();

            var weeks = Convert.ToInt32(Request.Form["week"]);
            var previousdate = DateTime.Now.AddDays(-7 * weeks);
            var firstdate = previousdate;
            while (firstdate.DayOfWeek != DayOfWeek.Monday)
            {
                firstdate = firstdate.AddDays(1);
            }

            firstdate = DateTime.Parse(firstdate.ToString("yyyy-MM-dd") + " 00:00:00");
            var datelist = new List<DateTime>();
            
            while (firstdate < DateTime.Now)
            {
                datelist.Add(firstdate);
                firstdate = firstdate.AddDays(7);
            }

            var ret = new Dictionary<string, List<HYDRASummary>>();
            foreach (var tester in hydratesters)
            {
                foreach (var sd in datelist)
                {
                    var edate = sd.AddDays(7).AddSeconds(-1);
                    if (edate > DateTime.Now)
                    { edate = DateTime.Now; }

                    var hydrarate = HYDRASummary.RetrieveHydraRate(tester, sd.ToString("yyyy-MM-dd HH:mm:ss"),edate.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (hydrarate.Count > 0)
                    {
                        if (ret.ContainsKey(tester))
                        {
                            ret[tester].Add(hydrarate[0]);
                        }
                        else
                        {
                            var templist = new List<HYDRASummary>();
                            templist.Add(hydrarate[0]);
                            ret.Add(tester, templist);
                        }
                    }
                }//end for
            }//end foreach
            return ret;
        }

        private Dictionary<string,List<HYDRASummary>> HydraDailyRate()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var hydratesters = syscfg["HYDRATESTER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            hydratesters.Sort();


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

                if (startdate < DateTime.Parse("2019-01-17 00:00:00"))
                { startdate = DateTime.Parse("2019-01-17 00:00:00"); }

                if (enddate < DateTime.Parse("2019-01-17 00:00:00"))
                { enddate = DateTime.Now; }
            }
            else
            {
                enddate = DateTime.Now;
                startdate = DateTime.Parse(enddate.AddDays(-7).ToString("yyyy-MM-dd") + " 00:00:00");
                if (startdate < DateTime.Parse("2019-01-17 00:00:00"))
                { startdate = DateTime.Parse("2019-01-17 00:00:00"); }
            }

            var ret = new Dictionary<string, List<HYDRASummary>>();
            foreach (var tester in hydratesters)
            {
                var sd = startdate;
                for (; sd < enddate;)
                {
                    var hydrarate = HYDRASummary.RetrieveHydraRate(tester, sd.ToString("yyyy-MM-dd HH:mm:ss"), sd.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                    if (hydrarate.Count > 0)
                    {
                        if (ret.ContainsKey(tester))
                        {
                            ret[tester].Add(hydrarate[0]);
                        }
                        else
                        {
                            var templist = new List<HYDRASummary>();
                            templist.Add(hydrarate[0]);
                            ret.Add(tester, templist);
                        }
                    }
                    sd = sd.AddDays(1);
                }//end for
            }//end foreach
            return ret;
        }

        public object GetHydraRateChart(Dictionary<string, List<HYDRASummary>> srcdata)
        {
            var serialist = new List<object>();
            var xlist = new List<string>();
            foreach (var kv in srcdata)
            {
                var dataarray = new List<object>();
                foreach (var hdata in kv.Value)
                {
                    var temlist = new List<object>();
                    temlist.Add(hdata.StartDate.ToString("yyyy-MM-dd"));
                    if (!xlist.Contains(hdata.StartDate.ToString("yyyy-MM-dd")))
                    {
                        xlist.Add(hdata.StartDate.ToString("yyyy-MM-dd"));
                    }
                    temlist.Add(hdata.Rate);
                    dataarray.Add(temlist);
                }
                serialist.Add(new {
                    name = kv.Key,
                    data = dataarray
                });
            }

            xlist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + " 00:00:00");
                var d2 = DateTime.Parse(obj2 + " 00:00:00");
                return d1.CompareTo(d2);
            });

            return new {
                id = "hydrarateid",
                xlist = xlist,
                serial = serialist
            };
        }

        public JsonResult HydraMACRateData()
        {
            var colorlist = new string[] { "#0053a2", "#bada55" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();
            var week = Request.Form["week"];
            if (!string.IsNullOrEmpty(week))
            {
                var weeklydata = HydraWeeklyRate();
                var ratedata = new List<HYDRASummary>();
                var idx = 0;
                foreach (var kv in weeklydata)
                {
                    var color = colorlist[idx % colorlist.Count];
                    foreach (var item in kv.Value)
                    { item.TestStation = "<span style='background-color:" + color + "'>" + item.TestStation + "</span>"; }

                    ratedata.AddRange(kv.Value);
                    idx = idx + 1;
                }

                var chartdata = GetHydraRateChart(weeklydata);
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    chartdata = chartdata,
                    ratedata = ratedata
                };
                return ret;
            }
            else
            {
                var dailydata = HydraDailyRate();
                var ratedata = new List<HYDRASummary>();
                var idx = 0;
                foreach (var kv in dailydata)
                {
                    var color = colorlist[idx%colorlist.Count];
                    foreach (var item in kv.Value)
                    { item.TestStation = "<span style='background-color:"+color+"'>" + item.TestStation + "</span>"; }

                    ratedata.AddRange(kv.Value);
                    idx = idx + 1;
                }

                var chartdata = GetHydraRateChart(dailydata);
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new {
                    chartdata = chartdata,
                    ratedata = ratedata
                };
                return ret;
            }
        }


        public ActionResult MachineTestTime()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var whichtests = syscfg["MACHINETIMEWHICHTEST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ViewBag.whichtestlist = CreateSelectList(whichtests, "");
            return View();
        }

        private object GetMachineChart(DateTime startdate,DateTime enddate,List<string> machinelist,string whichtest,string title,string target)
        {
            var mcond = "('" + string.Join("','", machinelist) + "')";
            var datadict = ModuleTestData.GetMachineTestTime(mcond,whichtest, startdate, enddate);

            var mlist = datadict.Keys.ToList();
            mlist.Sort();

            var mdict = new Dictionary<string, List<List<object>>>();

            var rad = new System.Random(DateTime.Now.Second);
            var idx = 0.0;
            foreach (var m in mlist)
            {
                var datapair = new List<List<object>>();
                var jdx = 0;
                foreach (var val in datadict[m])
                {
                    var templist = new List<object>();
                    var x = 0.0;
                    if (jdx % 2 == 0)
                    { x = idx + (rad.NextDouble() / 5.0); }
                    else
                    { x = idx - (rad.NextDouble() / 5.0); }
                    templist.Add(x);
                    templist.Add(val);
                    datapair.Add(templist);

                    jdx = jdx + 1;
                }
                idx = idx + 1.0;

                mdict.Add(m, datapair);
            }

            var serial = new List<object>();
            foreach (var m in mlist)
            {
                serial.Add(new
                {
                    name = m,
                    data = mdict[m]
                });
            }

            return new
            {
                id = title.ToLower().Trim().Replace(" ", "_").Replace("-", "_") + "_id",
                title = title,
                mlist = mlist,
                serial = serial,
                target = target,
                whichtest = whichtest,
                startdate = startdate.ToString("yyyy-MM-dd HH:mm:ss"),
                enddate = enddate.ToString("yyyy-MM-dd HH:mm:ss"),
            };
        }

        public JsonResult MachineTestTimeData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var whichtest = Request.Form["whichtest"];
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;
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

            var chartdatalist = new List<object>();

            var hydratesterkey = whichtest + "-HYDRA";
            var singletesterkey = whichtest + "-SINGLE";
            if (syscfg.ContainsKey(hydratesterkey))
            {
                var machinelist = syscfg[hydratesterkey].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var target = syscfg[hydratesterkey + "-TIMETARGET"];
                chartdatalist.Add(GetMachineChart(startdate, enddate, machinelist,whichtest, "HYDRA MACHINE TEST TIME-"+whichtest,target));
            }
            if (syscfg.ContainsKey(singletesterkey))
            {
                var machinelist = syscfg[singletesterkey].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var target = syscfg[singletesterkey + "-TIMETARGET"];
                chartdatalist.Add(GetMachineChart(startdate, enddate, machinelist,whichtest, "SINGLE MACHINE TEST TIME-"+whichtest,target));
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                chartdatalist = chartdatalist,
            };
            return ret;
        }

        public JsonResult MachineTestTimeDetail()
        {
            var tester = Request.Form["tester"];
            var sec = Request.Form["sec"];
            var whichtest = Request.Form["whichtest"];
            var startdate = Request.Form["startdate"];
            var enddate = Request.Form["enddate"];

            var mdatalist = ModuleTestData.RetrieveModuleTestTimeDetail(tester, startdate, enddate, whichtest, sec);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                mdatalist = mdatalist
            };
            return ret;
        }

    }
}