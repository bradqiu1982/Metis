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

    }
}