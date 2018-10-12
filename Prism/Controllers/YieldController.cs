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
            return View();
        }

        public JsonResult DepartmentYieldData()
        {
            var pdyieldlist = YieldVM.RetrieveAllYield(this);
            var titlelist = new List<object>();
            titlelist.Add("department");
            titlelist.Add("");
            foreach (var y in pdyieldlist[0].FinalYieldList)
            {
                titlelist.Add(y.Quarter);
            }

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
                linelist.Add(pdy.ProductFamily);
                linelist.Add("<span class='YINPUT'>INPUT</span><br><span class='YFPY'>FPY</span><br><span class='YFY'>FY</span>");
                var jdx = 0;
                foreach (var fy in pdy.FinalYieldList)
                {
                    var id1 = idx + "-" + jdx + "-0";
                    var id2 = idx + "-" + jdx + "-1";
                    linelist.Add("<span class='YINPUT'>" + pdy.FirstYieldList[jdx].MaxInput+ "</span><br><span class='YFPY YIELDDATA' myid='"+id1+"'>" + pdy.FirstYieldList[jdx].YieldVal + "</span><br><span class='YFY YIELDDATA' myid='" + id2+"'>" + fy.YieldVal+"</span>");

                    pdy.FirstYieldList[jdx].TestYieldList.Sort(delegate (TestYieldVM obj1, TestYieldVM obj2) { return obj1.Yield.CompareTo(obj2.Yield); });
                    TestYieldList.Add(
                        new {
                            id = id1,
                            testlist = pdy.FirstYieldList[jdx].TestYieldList
                        }
                        );

                    pdy.FinalYieldList[jdx].TestYieldList.Sort(delegate (TestYieldVM obj1, TestYieldVM obj2) { return obj1.Yield.CompareTo(obj2.Yield); });
                    TestYieldList.Add(
                        new
                        {
                            id = id2,
                            testlist = pdy.FinalYieldList[jdx].TestYieldList
                        }
                        );

                    chartxlist.Add(fy.Quarter);
                    chartfpydata.Add(pdy.FirstYieldList[jdx].YieldVal);
                    chartfydata.Add(fy.YieldVal);

                    jdx += 1;
                }
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
                var chartid = pdy.ProductFamily.Replace(".", "").Replace(" ", "").ToLower()+"_id";
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
                testyieldlist = TestYieldList
            };
            return ret;
        }

        public ActionResult ProductYield()
        {
            return View();
        }
    }
}