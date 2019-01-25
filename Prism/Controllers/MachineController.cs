﻿using System;
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
                    color = "#90ed7d"
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
            ret.Data = new {
                success = true,
                chartlist = chartlist
            };
            return ret;
        }

    }
}