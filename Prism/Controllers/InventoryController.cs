using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;
using System.Reflection;

namespace Prism.Controllers
{
    public class InventoryController : Controller
    {
        public ActionResult DepartmentInventory()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "DepartmentInventory"))
            {
                return RedirectToAction("Index", "Main");
            }

            return View();
        }

        private JsonResult GetInventoryDataChart(List<InventoryData> inventorydata, bool fordepartment = true)
        {
            //var colorlist = new string[] { "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
            //    , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
            //    , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();
            var colorlist = new string[] { "#749dd2", "#3c92ba", "#645c87","#ff4500","#a9302e","#84a370", "#ebc843",  "#EB6100", "#E4007F"
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
                    linelist.Add("<a href='/Inventory/ProductInventory?producttype=" + Url.Encode(pd) + "' target='_blank'>" + pd + "</a>");
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

        public JsonResult DepartmentDetailDataDP()
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


        public ActionResult ProductInventory(string producttype)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ProductInventory"))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.producttype = "";
            if (!string.IsNullOrEmpty(producttype))
            { ViewBag.producttype = producttype; }
            return View();
        }

        public JsonResult ProductInventoryData()
        {
            var prodtype = Request.Form["prodtype"];
            var prod = Request.Form["prod"];

            var ivtdatalist = new List<InventoryData>();

            var standardpd = false;
            var standardpdlist = InventoryData.RetrieveAllPF();
            foreach (var stdpd in standardpdlist)
            {
                if (string.Compare(prodtype, stdpd) == 0)
                {
                    standardpd = true;
                    prod = prodtype;
                }
            }

            if (!string.IsNullOrEmpty(prod))
            {
                if (standardpd)
                {
                    var pdlist = prod.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    ivtdatalist = InventoryData.RetrieveDetailDataByStandardPD(pdlist);
                }
                else
                {
                    var pdlist = prod.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    ivtdatalist = InventoryData.RetrieveDetailDataByPD(pdlist);
                }
            }
            else
            {
                ivtdatalist = InventoryData.RetrieveDetailDataByDP(prodtype,null);
            }
            return GetInventoryDataChart(ivtdatalist,false);
        }

        public JsonResult GetAllProductList()
        {
            var pdlist = InventoryData.GetAllProductList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                pdlist = pdlist
            };
            return ret;
        }

        public ActionResult ProductCost()
        {
            ViewBag.AUTH = false;
            var usermap = MachineUserMap.GetUserInfo(Request.UserHostName,"COST");
            if (!string.IsNullOrEmpty(usermap.username))
            {
                ViewBag.AUTH = true;
            }
            ViewBag.Level = usermap.level;
            return View();
        }

        public JsonResult ProductCostPMList()
        {
            var pmlist = ProductCostVM.PMList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                pmlist = pmlist
            };
            return ret;
        }

        public JsonResult ProductCostPNList()
        {
            var pnlist = ProductCostVM.PNList();
            var pnfamdict = PNProuctFamilyCache.PNPFDict();
            var famdict = new Dictionary<string, bool>();
            foreach (var pn in pnlist)
            {
                if (pnfamdict.ContainsKey(pn)) {
                    if (!famdict.ContainsKey(pnfamdict[pn]))
                    { famdict.Add(pnfamdict[pn],true); }
                }
            }

            pnlist.AddRange(famdict.Keys);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                pnlist = pnlist
            };
            return ret;
        }


        private object SolveProductCost(Dictionary<string, List<ProductCostVM>> pdcost)
        {
            var paramlist = ProductCostVM.GetCostFields();
            var parammap = ProductCostVM.GetCostFieldNameMap();

            PropertyInfo[] properties = typeof(ProductCostVM).GetProperties();
            var properlist = new List<PropertyInfo>();
            foreach (var param in paramlist)
            {
                foreach (var proper in properties)
                {
                    if (string.Compare(param, proper.Name) == 0)
                    {
                        properlist.Add(proper);
                    }
                }
            }

            var ret = new List<object>();
            foreach (var kv in pdcost)
            {
                var table = new List<List<string>>();
                foreach (var proper in properlist)
                {
                    var line = new List<string>();
                    line.Add(parammap[proper.Name]);
                    foreach (var cost in kv.Value)
                    {
                        var val = proper.GetValue(cost);
                        if (string.Compare(proper.Name, "QuarterType") == 0)
                        {
                            line.Add(UT.O2S(val).ToUpper().Replace("WUXI","").Replace("MATERIAL","M").Replace("UPDATE",""));
                        }
                        else if (string.Compare(proper.Name, "Yield") == 0
                            || string.Compare(proper.Name, "LobEff") == 0)
                        {
                             line.Add(Math.Round(UT.O2D(val) * 100.0, 0).ToString() + "%");
                        }
                        else if (proper.Name.Contains("HPU"))
                        {
                            line.Add(Math.Round(UT.O2D(val), 2).ToString());
                        }
                        else if (string.Compare(proper.Name, "Qty") == 0
                            || string.Compare(proper.Name, "ASP") == 0)
                        {
                            line.Add(Math.Round(UT.O2D(val), 0).ToString());
                        }
                        else
                        {
                            line.Add("$ "+Math.Round(UT.O2D(val), 2).ToString());
                        }
                    }//end foreach
                    table.Add(line);
                }

                var chart1xlist = new List<object>();
                var epcostdatalist = new List<object>();
                var fcostdatalist = new List<object>();
                var fmcostdict = new Dictionary<string, double>();
                foreach (var cost in kv.Value)
                {
                    var epdatapair = new List<object>();
                    if (cost.QuarterType.ToUpper().Contains("EP"))
                    {
                        chart1xlist.Add(cost.Quarter);
                        epdatapair.Add(cost.Quarter);
                        epdatapair.Add(Math.Round(UT.O2D(cost.UMCost),2));
                        epcostdatalist.Add(epdatapair);
                    }

                    var fdatapaire = new List<object>();
                    if (cost.QuarterType.ToUpper().Contains("F") 
                        && !cost.QuarterType.ToUpper().Contains("MATE"))
                    {
                        fdatapaire.Add(cost.Quarter);
                        fdatapaire.Add(Math.Round(UT.O2D(cost.UMCost), 2));
                        fcostdatalist.Add(fdatapaire);
                    }

                    if (cost.QuarterType.ToUpper().Contains("MATE"))
                    {
                        fmcostdict.Add(cost.Quarter, Math.Round(UT.O2D(cost.UMCost), 2));
                    }
                }//end foreach

                foreach (var fcostpair in fcostdatalist)
                {
                    if (fmcostdict.ContainsKey((string)((List<object>)fcostpair)[0]))
                    {
                        ((List<object>)fcostpair)[1] = fmcostdict[(string)((List<object>)fcostpair)[0]];
                    }
                }

                var chart1chartseris = new List<object>();
                chart1chartseris.Add(new
                {
                    name = "COST(EP)",
                    type = "column",
                    data = epcostdatalist,
                    color = "#d91919",
                    maxPointWidth = 40
                });
                chart1chartseris.Add(new
                {
                    name = "COST(F)",
                    type = "column",
                    data = fcostdatalist,
                    color = "#193979",
                    maxPointWidth = 40
                });

                var chart1 = new {
                    title = kv.Key+" COST ROADMAP",
                    xlist = chart1xlist,
                    chartseris = chart1chartseris
                };

                var fdataselect = new Dictionary<string, bool>();
                foreach (var cost in kv.Value)
                {
                    if (cost.QuarterType.ToUpper().Contains("F")
                        && !cost.QuarterType.ToUpper().Contains("MATE"))
                    {
                        if (!fdataselect.ContainsKey(cost.Quarter))
                        {
                            fdataselect.Add(cost.Quarter, true);
                        }
                    }
                }

                var chart2xlist = new List<object>();
                var bomlist = new List<object>();
                var dllist = new List<object>();
                var scraplist = new List<object>();
                var overheadlist = new List<object>();

                var umcostlist = new List<object>();
                var asplist = new List<object>();
                var crossmargin = new List<object>();

                foreach (var tempcost in kv.Value)
                {
                    ProductCostVM cost = null;
                    if (fdataselect.ContainsKey(tempcost.Quarter))
                    {
                        if (tempcost.QuarterType.ToUpper().Contains("F")
                        && !tempcost.QuarterType.ToUpper().Contains("MATE"))
                        { cost = tempcost; }
                    }
                    else
                    {
                        if (tempcost.QuarterType.ToUpper().Contains("EP"))
                        { cost = tempcost; }
                    }

                    if (cost != null)
                    {
                        chart2xlist.Add(cost.Quarter);
                        bomlist.Add(Math.Round(UT.O2D(cost.BOM), 2));
                        dllist.Add(Math.Round(UT.O2D(cost.LabFOther)+ UT.O2D(cost.DLFG) + UT.O2D(cost.DLSFG), 2));
                        scraplist.Add(Math.Round(UT.O2D(cost.SMFG) + UT.O2D(cost.SMSFG), 2));
                        overheadlist.Add(Math.Round(UT.O2D(cost.UMCost) - UT.O2D(cost.VairableCost), 2));

                        umcostlist.Add(Math.Round(UT.O2D(cost.UMCost), 2));
                        asplist.Add(Math.Round(UT.O2D(cost.ASP), 2));
                        crossmargin.Add(Math.Round((UT.O2D(cost.ASP) - UT.O2D(cost.UMCost)) / UT.O2D(cost.ASP)*100,2));
                    }
                }

                var chart2chartseris = new List<object>();

                chart2chartseris.Add(new
                {
                    name = "Overhead(VCSEL/PD)",
                    type = "column",
                    data = overheadlist,
                    color = "#d91919",
                    maxPointWidth = 40,
                    yAxis = 0
                });

                chart2chartseris.Add(new
                {
                    name = "Scrap",
                    type = "column",
                    data = scraplist,
                    color = "#bc2226",
                    maxPointWidth = 40,
                    yAxis = 0
                });

                chart2chartseris.Add(new
                {
                    name = "DL",
                    type = "column",
                    data = dllist,
                    color = "#8c2025",
                    maxPointWidth = 40,
                    yAxis = 0
                });

                chart2chartseris.Add(new
                {
                    name = "BOM",
                    type = "column",
                    data = bomlist,
                    color = "#451920",
                    maxPointWidth = 40,
                    yAxis = 0
                });

                chart2chartseris.Add(new
                {
                    name = "Unit Mfg Cost",
                    type = "line",
                    data = umcostlist,
                    color = "#7030a0",
                    yAxis = 0
                });

                chart2chartseris.Add(new
                {
                    name = "Crosss Margin",
                    type = "line",
                    data = crossmargin,
                    color = "#5dc1f4",
                    yAxis = 1
                });

                chart2chartseris.Add(new
                {
                    name = "ASP",
                    type = "line",
                    data = asplist,
                    color = "#1c2bbe",
                    yAxis = 0,
                    visible = false
                });

                var chart2 = new
                {
                    title = kv.Key + " MFG COST TREND",
                    xlist = chart2xlist,
                    chartseris = chart2chartseris
                };

                var pm = "";
                if (kv.Value.Count > 0)
                { pm = kv.Value[0].PM; }
                ret.Add(new
                {
                    pn = kv.Key,
                    pm = pm,
                    table = table,
                    chart1 = chart1,
                    chart2 = chart2
                });
            }

            return ret;
        }

        //public JsonResult ProductCostPMData()
        //{
        //    var usermap = MachineUserMap.GetUserInfo(Request.UserHostName);
        //    var pdcost = ProductCostVM.GetProdctCostData(usermap.username, "");
        //    var datalist = SolveProductCost(pdcost);

        //    var ret = new JsonResult();
        //    ret.MaxJsonLength = Int32.MaxValue;
        //    ret.Data = new
        //    {
        //        datalist = datalist
        //    };
        //    return ret;
        //}

        private Dictionary<string, bool> GetAuthorizatedPN()
        {
            var usermap = MachineUserMap.GetUserInfo(Request.UserHostName, "COST");
            var allowedpndict = new Dictionary<string, bool>();
            if (usermap.ugroup.ToUpper().Contains("COSTIPOH"))
            {
                var ipohpns = CfgUtility.GetSysConfig(this)["COSTIPOH"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var ipn in ipohpns)
                {
                    if (!allowedpndict.ContainsKey(ipn))
                    { allowedpndict.Add(ipn, true); }
                }
            }
            else if (usermap.ugroup.ToUpper().Contains("COSTPRIV"))
            {
                var syscfg = CfgUtility.GetSysConfig(this);
                if (syscfg.ContainsKey(usermap.username+"_COST"))
                {
                    var sppns = CfgUtility.GetSysConfig(this)[usermap.username+ "_COST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ipn in sppns)
                    {
                        if (!allowedpndict.ContainsKey(ipn))
                        { allowedpndict.Add(ipn, true); }
                    }
                }
                else
                {
                    var sppns = ProductCostVM.PNList(usermap.username);
                    foreach (var ipn in sppns)
                    {
                        if (!allowedpndict.ContainsKey(ipn))
                        { allowedpndict.Add(ipn, true); }
                    }
                }
            }

            return allowedpndict;
        }
        
        public JsonResult ProductCostQuery()
        {
            var pm = Request.Form["pm"];
            var pd = Request.Form["pd"];

            var famdpnict = PNProuctFamilyCache.PFPNDict();
            var pnlist = new List<string>();
            if (!string.IsNullOrEmpty(pd))
            {
                pm = "";
                if (famdpnict.ContainsKey(pd))
                {
                    pnlist.AddRange(famdpnict[pd]);
                }
                else
                {
                    pnlist.Add(pd);
                }
            }

            var pdcost = ProductCostVM.GetProdctCostData(pm, pnlist);
            var allowedpndict = GetAuthorizatedPN();
            var filtercost = new Dictionary<string, List<ProductCostVM>>();
            foreach (var kv in pdcost)
            {
                if (allowedpndict.Count > 0)
                {
                    if (!allowedpndict.ContainsKey(kv.Key))
                    { continue; }
                }

                filtercost.Add(kv.Key, kv.Value);
            }

            var datalist = SolveProductCost(filtercost);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                datalist = datalist
            };
            return ret;
        }

        public JsonResult UpdateProductCost()
        {
            ExternalDataCollector.LoadProductCostData(this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true
            };
            return ret;
        }


    }
}