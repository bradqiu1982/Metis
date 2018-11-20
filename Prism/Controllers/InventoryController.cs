using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;

namespace Prism.Controllers
{
    public class InventoryController : Controller
    {
        public ActionResult DepartmentInventory()
        {
            return View();
        }

        private JsonResult GetInventoryDataChart(List<InventoryData> inventorydata, bool fordepartment = true)
        {
            var colorlist = new string[] { "#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

            var quarterdict = new Dictionary<string, bool>();
            var pddict = new Dictionary<string, Dictionary<string, InventoryData>>();
            foreach (var cd in inventorydata)
            {
                if (!quarterdict.ContainsKey(cd.Quarter))
                { quarterdict.Add(cd.Quarter, true); }

                var pd = cd.Department;
                if (!fordepartment)
                { pd = cd.Product; }

                if (pddict.ContainsKey(pd))
                {
                    var qdict = pddict[pd];
                    if (qdict.ContainsKey(cd.Quarter))
                    {
                    }
                    else
                    {
                        qdict.Add(cd.Quarter, cd);
                    }
                }
                else
                {
                    var qdict = new Dictionary<string, InventoryData>();
                    qdict.Add(cd.Quarter, cd);
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
            if (fordepartment)
            {
                var syscfg = CfgUtility.GetSysConfig(this);
                var pdordlist = syscfg["INVENTORYDEPARTMENTORD"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                pdordlist.Reverse();
                foreach (var p in pdordlist)
                {
                    pdlist.Remove(p);
                }
                foreach (var p in pdordlist)
                {
                    pdlist.Insert(0, p);
                }
            }

            var contentlist = new List<object>();
            var chartdatalist = new List<object>();
            foreach (var pd in pdlist)
            {
                var seriallist = new List<object>();

                var xlist = new List<string>();
                var cogslist = new List<double>();
                var inventorylist = new List<double>();
                var turnslist = new List<double>();

                var chartid = "inventory_" + pd.Replace(".", "").Replace(" ", "").Replace("/", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";


                var linelist = new List<object>();
                if (fordepartment)
                {
                    linelist.Add("<a href='/Product/ProductInventory?producttype=" + Url.Encode(pd) + "' target='_blank'>" + pd + "</a>");
                }
                else
                {
                    linelist.Add(pd);
                }

                linelist.Add("<span class='YFPY'>COSG</span><br><span class='YFY'>Inventory</span><br><span class='YINPUT'>Inventory Turns</span>");

                var qtdata = pddict[pd];
                foreach (var q in qlist)
                {
                    if (qtdata.ContainsKey(q))
                    {
                        linelist.Add("<span class='YFPY YIELDDATA' pd='" + pd + "' qt='" + q + "'>" + qtdata[q].COGS + "</span><br><span class='YFY YIELDDATA' pd='" + pd + "' qt='" + q + "'>" + qtdata[q].Inventory + "</span><br><span class='YINPUT'>" + qtdata[q].InventoryTurns + "</span>");
                        xlist.Add(q);
                        cogslist.Add(qtdata[q].COGS);
                        inventorylist.Add(qtdata[q].Inventory);
                        turnslist.Add(qtdata[q].InventoryTurns);
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
                        name = "COGS",
                        data = cogslist,
                        color = colorlist[0]
                    });
                    seriallist.Add(new
                    {
                        type = "column",
                        name = "Inventory",
                        data = inventorylist,
                        color = colorlist[1]
                    });
                    seriallist.Add(new
                    {
                        type = "line",
                        name = "Inventory Turns",
                        data = turnslist,
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
                chartdatalist = chartdatalist
            };
            return ret;
        }

        public JsonResult DepartmentInventoryData()
        {
            var inventorydata = InventoryData.RetrieveAllTrendData();
            return GetInventoryDataChart(inventorydata);
        }

        public JsonResult DepartmentDetailData()
        {
            var pd = Request.Form["pd"];
            var qt = Request.Form["qt"];
            var invtdata = InventoryData.RetrieveDetailDataByDP(pd, qt);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new {
                invtdata = invtdata
            };
            return ret;
        }


    }
}