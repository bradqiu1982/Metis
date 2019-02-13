using Prism.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Controllers
{
    public class YieldController : Controller
    {
        public ActionResult DepartmentYield()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "DepartmentYield"))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        private List<string> RetrieveQuarterFromYield(List<ProductYield> pdys)
        {
            var quarterdict = new Dictionary<string, bool>();
            foreach (var pdy in pdys)
            {
                foreach (var y in pdy.FinalYieldList)
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

        private JsonResult GetYieldTableAndChart(List<ProductYield> pdyieldlist, string title, bool fordepartment = false)
        {
            var preburnin = new Dictionary<string, Dictionary<string, int>>();
            var postburnin = new Dictionary<string, Dictionary<string, int>>();
            if (fordepartment)
            {
                preburnin = YieldVM.RetrieveBurnInData("'Pre Burn In_%'");
                postburnin = YieldVM.RetrieveBurnInData("'Post Burn In_%'");
            }

            var titlelist = new List<object>();
            titlelist.Add(title);
            titlelist.Add("");

            var quarterlist = RetrieveQuarterFromYield(pdyieldlist);
            titlelist.AddRange(quarterlist);


            var chartdatalist = new List<object>();
            var TestYieldList = new List<object>();
            var idx = 0;
            var contentlist = new List<object>();
            foreach (var pdy in pdyieldlist)
            {
                var chartxlist = new List<object>();
                var chartseriallist = new List<object>();
                var chartfpydata = new List<object>();
                var chartfydata = new List<object>();

                var linelist = new List<object>();
                if (fordepartment)
                {
                    linelist.Add("<a href='/Yield/ProductYield?productfaimly="+pdy.ProductFamily.Replace("+", "%2B")+"' target='_blank'>"+ pdy.ProductFamily + "</a>");
                }
                else
                {
                    if (string.IsNullOrEmpty(pdy.ProjectKey))
                    {
                        linelist.Add(pdy.ProductFamily);
                    }
                    else
                    {
                        linelist.Add("<a href='http://wuxinpi.china.ads.finisar.com/Project/ProjectDetail?ProjectKey=" + pdy.ProjectKey + "' target='_blank'>" + pdy.ProductFamily + "</a>");
                    }
                }

                if (string.Compare(pdy.ProductFamily, "PARALLEL", true) == 0 && fordepartment)
                {
                    linelist.Add("<span class='YFPY'>FPY</span><br><span class='YFY'>FY</span><br><span class='YINPUT'>INPUT</span><br><span class='YINPUT'>Pre BI</span><br><span class='YINPUT'>Post BI</span>");
                }
                else
                {
                    linelist.Add("<span class='YFPY'>FPY</span><br><span class='YFY'>FY</span><br><span class='YINPUT'>INPUT</span>");
                }


                var testyieldidx = 0;
                foreach (var qt in quarterlist)
                {
                    var matchidx = 0;
                    var matchflag = false;
                    foreach (var fy in pdy.FinalYieldList)
                    {
                        if (string.Compare(qt, fy.Quarter, true) == 0)
                        {
                            matchflag = true;
                            break;
                        }
                        matchidx += 1;
                    }

                    if (matchflag && pdy.FirstYieldList[matchidx].MaxInput > 0)
                    {
                        var id1 = idx + "-" + testyieldidx + "-0";
                        var id2 = idx + "-" + testyieldidx + "-1";
                        testyieldidx += 1;

                        if (string.Compare(pdy.ProductFamily, "PARALLEL", true) == 0 && fordepartment)
                        {
                            var pre = "";
                            var post = "";
                            if (preburnin.ContainsKey(qt))
                            {
                                var pass = preburnin[qt]["PASS"];
                                var fail = preburnin[qt]["FAILED"];
                                pre = Math.Round((double)pass / (double)(pass + fail) * 100.0, 2).ToString();
                            }
                            if (postburnin.ContainsKey(qt))
                            {
                                var pass = postburnin[qt]["PASS"];
                                var fail = postburnin[qt]["FAILED"];
                                post = Math.Round((double)pass / (double)(pass + fail) * 100.0, 2).ToString();
                            }

                            linelist.Add("<span class='YFPY YIELDDATA' myid='" + id1 + "'>" + pdy.FirstYieldList[matchidx].YieldVal 
                                + "</span><br><span class='YFY YIELDDATA' myid='" + id2 + "'>" + pdy.FinalYieldList[matchidx].YieldVal + "</span><br><span class='YINPUT'>" + pdy.FirstYieldList[matchidx].MaxInput + "</span><br>"
                                + "<span class='YINPUT'>" + pre + "</span><br><span class='YINPUT'>"+post+ "</span>");
                        }
                        else
                        {
                            linelist.Add("<span class='YFPY YIELDDATA' myid='"+id1+"'>" + pdy.FirstYieldList[matchidx].YieldVal + "</span><br><span class='YFY YIELDDATA' myid='" + id2+"'>" + pdy.FinalYieldList[matchidx].YieldVal+ "</span><br><span class='YINPUT'>" + pdy.FirstYieldList[matchidx].MaxInput + "</span>");
                        }

                        pdy.FirstYieldList[matchidx].TestYieldList.Sort(delegate (TestYieldVM obj1, TestYieldVM obj2) { return obj1.Yield.CompareTo(obj2.Yield); });
                        TestYieldList.Add(
                            new
                            {
                                id = id1,
                                testlist = pdy.FirstYieldList[matchidx].TestYieldList
                            }
                            );

                        pdy.FinalYieldList[matchidx].TestYieldList.Sort(delegate (TestYieldVM obj1, TestYieldVM obj2) { return obj1.Yield.CompareTo(obj2.Yield); });
                        TestYieldList.Add(
                            new
                            {
                                id = id2,
                                testlist = pdy.FinalYieldList[matchidx].TestYieldList
                            }
                            );

                        chartxlist.Add(pdy.FinalYieldList[matchidx].Quarter);
                        chartfpydata.Add(pdy.FirstYieldList[matchidx].YieldVal);
                        chartfydata.Add(pdy.FinalYieldList[matchidx].YieldVal);
                    }
                    else
                    {
                        linelist.Add(" ");
                    }
                }//end foreach

                contentlist.Add(linelist);

                chartseriallist.Add(new
                {
                    name = "FPY",
                    data = chartfpydata
                });
                chartseriallist.Add(new
                {
                    name = "FY",
                    data = chartfydata
                });
                var chartid = pdy.ProductFamily.Replace(".", "").Replace(" ", "")
                    .Replace("(","").Replace(")", "").Replace("#", "").Replace("+", "").ToLower()+"_id";
                chartdatalist.Add(new
                {
                    id = chartid,
                    fpytg = pdy.FPYTG,
                    fytg = pdy.FYTG,
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
                testyieldlist = TestYieldList
            };
            ret.MaxJsonLength = Int32.MaxValue;
            return ret;
        }

        public JsonResult DepartmentYieldData()
        {
            var pdyieldlist = YieldVM.RetrieveAllYield(this);
            return GetYieldTableAndChart(pdyieldlist, "Department",true);
        }

        public ActionResult ProductYield(string productfaimly)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ProductYield"))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.productfamily = "";
            if (!string.IsNullOrEmpty(productfaimly))
            { ViewBag.productfamily = productfaimly; }

            return View();
        }

        public JsonResult GetAllYieldProductList()
        {
            var pdlist = YieldVM.RetrieveAllProductList();
            var ret = new JsonResult();
            ret.Data = new
            {
                pdlist = pdlist
            };
            return ret;
        }

        public JsonResult ProductYieldData()
        {
            var pdf = Request.Form["pdf"];
            var yieldcfg = CfgUtility.LoadYieldConfig(this);
            var orderlist = yieldcfg["YIELDORDER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var sortedlist = new List<ProductYield>();
            var familydict = new Dictionary<string, bool>();

            var pdyieldlist = YieldVM.RetrieveProductYieldByYF(pdf,this);
            foreach (var od in orderlist)
            {
                foreach (var pdy in pdyieldlist)
                {
                    if (pdy.ProductFamily.Contains(od))
                    {
                        if (!familydict.ContainsKey(pdy.ProductFamily))
                        {
                            familydict.Add(pdy.ProductFamily, true);
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
                    if (pdy.ProductFamily.Contains(od))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    if (!familydict.ContainsKey(pdy.ProductFamily))
                    {
                        familydict.Add(pdy.ProductFamily, true);
                        sortedlist.Add(pdy);
                    }
                }
           }

            return GetYieldTableAndChart(sortedlist, "Product");
        }

        public ActionResult YieldTrend()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "YieldTrend"))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        public JsonResult YieldTrendData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var pdf = syscfg["YIELDTRENDPDTYPE"];
            var yieldcfg = CfgUtility.LoadYieldConfig(this);
            var orderlist = yieldcfg["YIELDORDER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var sortedlist = new List<ProductYield>();
            var familydict = new Dictionary<string, bool>();

            var pdyieldlist = YieldVM.RetrieveProductYieldByYF(pdf, this);
            foreach (var od in orderlist)
            {
                foreach (var pdy in pdyieldlist)
                {
                    if (pdy.ProductFamily.Contains(od))
                    {
                        if (!familydict.ContainsKey(pdy.ProductFamily))
                        {
                            familydict.Add(pdy.ProductFamily, true);
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
                    if (pdy.ProductFamily.Contains(od))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    if (!familydict.ContainsKey(pdy.ProductFamily))
                    {
                        familydict.Add(pdy.ProductFamily, true);
                        sortedlist.Add(pdy);
                    }
                }
            }

            return GetYieldTableAndChart(sortedlist, "Product");
        }

    }
}