using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;

namespace Prism.Controllers
{
    public class CapacityController : Controller
    {
        public ActionResult DepartmentCapacity()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "DepartmentCapacity"))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        private JsonResult GetCapacityDataChart(List<CapacityRawData> capdatalist, bool fordepartment = true)
        {
            var colorlist = new string[] { "#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();
            var quarterdict = new Dictionary<string, bool>();
            var rawdatadict = new Dictionary<string, List<CapacityRawData>>();
            var pddict = new Dictionary<string, Dictionary<string, CapacityRawData>>();
            foreach (var cd in capdatalist)
            {

                if (!quarterdict.ContainsKey(cd.Quarter))
                { quarterdict.Add(cd.Quarter, true); }

                var pd = cd.ProductType;
                if (!fordepartment)
                { pd = cd.Product; }

                if (rawdatadict.ContainsKey(pd))
                { rawdatadict[pd].Add(cd); }
                else
                {
                    var templist = new List<CapacityRawData>();
                    templist.Add(cd);
                    rawdatadict.Add(pd, templist);
                }

                if (pddict.ContainsKey(pd))
                {
                    var qdict = pddict[pd];
                    if (qdict.ContainsKey(cd.Quarter))
                    {
                        qdict[cd.Quarter].MaxCapacity += cd.MaxCapacity;
                        qdict[cd.Quarter].ForeCast += cd.ForeCast;
                    }
                    else
                    {
                        var tempvm = new CapacityRawData();
                        tempvm.MaxCapacity = cd.MaxCapacity;
                        tempvm.ForeCast = cd.ForeCast;
                        qdict.Add(cd.Quarter, tempvm);
                    }
                }
                else
                {
                    var tempvm = new CapacityRawData();
                    tempvm.MaxCapacity = cd.MaxCapacity;
                    tempvm.ForeCast = cd.ForeCast;
                    var qdict = new Dictionary<string, CapacityRawData>();
                    qdict.Add(cd.Quarter, tempvm);
                    pddict.Add(pd, qdict);
                }
            }

            var qlist = quarterdict.Keys.ToList();
            qlist.Sort(delegate (string q1, string q2)
            {
                var qd1 = QuarterCLA.RetrieveDateFromQuarter(q1);
                var qd2 = QuarterCLA.RetrieveDateFromQuarter(q2);
                return qd1[0].CompareTo(qd2[0]);
            });

            var titlelist = new List<object>();
            titlelist.Add("Department");
            titlelist.Add("");
            titlelist.AddRange(qlist);

            var pdlist = pddict.Keys.ToList();
            var contentlist = new List<object>();
            var chartdatalist = new List<object>();
            foreach (var pd in pdlist)
            {
                var seriallist = new List<object>();

                var xlist = new List<string>();
                var mcaplist = new List<double>();
                var fclist = new List<double>();
                var gaplist = new List<double>();
                var ratelist = new List<double>();
                var chartid = "cap_" + pd.Replace(".", "").Replace(" ", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";


                var linelist = new List<object>();
                if (fordepartment)
                {
                    linelist.Add("<a href='/Capacity/ProductCapacity?producttype="+Url.Encode(pd)+"' target='_blank'>" + pd+"</a>");
                }
                else
                {
                    linelist.Add(pd);
                }
                
                linelist.Add("<span class='YFPY'>Max Capacity</span><br><span class='YFY'>Forecast</span><br><span class='YINPUT'>Cusume Rate %</span><br><span class='YINPUT'>Buffer</span>");
                
                var qtdata = pddict[pd];
                foreach (var q in qlist)
                {
                    if (qtdata.ContainsKey(q))
                    {
                        var BUFFTAG = "YINPUT";
                        if (qtdata[q].Usage > 90)
                        {
                            BUFFTAG = "NOBUFF";
                        }

                        linelist.Add("<span class='YFPY YIELDDATA' myid='" + pd+"'>"+qtdata[q].MaxCapacity+ "</span><br><span class='YFY YIELDDATA' myid='" + pd+ "'>" + qtdata[q].ForeCast + "</span><br><span class='"+ BUFFTAG + "'>" + qtdata[q].Usage + "</span><br><span class='"+ BUFFTAG + "'>" + qtdata[q].GAP + "</span>");
                        xlist.Add(q);
                        mcaplist.Add(qtdata[q].MaxCapacity);
                        fclist.Add(qtdata[q].ForeCast);
                        gaplist.Add(qtdata[q].GAP);
                        ratelist.Add(qtdata[q].Usage);
                    }
                    else
                    { linelist.Add(" "); }
                }
                contentlist.Add(linelist);

                if (xlist.Count > 0)
                {
                    seriallist.Add(new
                    {
                        type = "column",
                        name = "Max Capacity",
                        data = mcaplist,
                        color = colorlist[0]
                    });
                    seriallist.Add(new
                    {
                        type = "column",
                        name = "Forecast",
                        data = fclist,
                        color = colorlist[1]
                    });
                    seriallist.Add(new
                    {
                        type = "column",
                        name = "Buffer",
                        data = gaplist,
                        color = colorlist[2]
                    });
                    seriallist.Add(new
                    {
                        type = "line",
                        name = "Capacity Consume Rate",
                        data = ratelist,
                        color = colorlist[3],
                        yAxis = 1
                    });

                    chartdatalist.Add(new
                    {
                        id = chartid,
                        xlist = xlist,
                        series = seriallist,
                        pd = pd
                    });
                }
            }
            
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                tabletitle = titlelist,
                tablecontent = contentlist,
                rawdata = rawdatadict,
                chartdatalist = chartdatalist
            };
            return ret;
        }

        public JsonResult DepartmentCapacityData()
        {
            var capdatalist = CapacityRawData.RetrieveAllData();
            return GetCapacityDataChart(capdatalist);
        }

        public ActionResult ProductCapacity(string producttype)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ProductCapacity"))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.producttype = "";
            if (!string.IsNullOrEmpty(producttype))
            { ViewBag.producttype = producttype; }
            return View();
        }

        public JsonResult GetAllProductList()
        {
            var pdlist = CapacityRawData.GetAllProductList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                pdlist = pdlist
            };
            return ret;
        }

        public JsonResult ProductCapacityData()
        {
            var prodtype = Request.Form["prodtype"];
            var prod = Request.Form["prod"];

            var capdatalist = new List<CapacityRawData>();
            var allproduct = CapacityRawData.GetAllProductList();
            foreach (var pd in allproduct)
            {
                if (string.Compare(pd, prodtype) == 0)
                {
                    prod = prodtype;
                }
            }

            if (!string.IsNullOrEmpty(prod))
            {
                var pdlist = prod.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                capdatalist = CapacityRawData.RetrieveDataByProd(pdlist);
            }
            else
            {
                capdatalist = CapacityRawData.RetrieveDataByProductType(prodtype);
            }
            return GetCapacityDataChart(capdatalist,false);
        }

    }
}