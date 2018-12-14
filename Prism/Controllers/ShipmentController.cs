using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Prism.Models;
using System.Text;

namespace Prism.Controllers
{
    public class ShipmentController : Controller
    {


        public ActionResult ShipmentData()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.IsL9Employee = MachineUserMap.IsLxEmployee(Request.UserHostName, null, 10);
            return View();
        }

        ////<date,<customer,int>>
        private object GetShipmentChartData(Dictionary<string, Dictionary<string, double>> shipdata, Dictionary<string, int> vcselrmacntdict, Dictionary<string, int> allrmacntdict, string rate, string producttype)
        {
            var id = "shipdata_" + rate.Replace(".", "").Replace(" ", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";
            var shipdatelist = shipdata.Keys.ToList();
            shipdatelist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + "-01 00:00:00");
                var d2 = DateTime.Parse(obj2 + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

            var datecntdict = new Dictionary<string, double>();
            foreach (var kv in shipdata)
            {
                var totle = 0.0;
                foreach (var nd in kv.Value)
                {
                    totle = totle + nd.Value;
                }
                datecntdict.Add(kv.Key, totle);
            }

            var colorlist = new string[] { "#161525", "#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

            var namelist = shipdata[shipdatelist[0]].Keys.ToList();

            var custsumpair = new List<KeyValuePair<string, double>>();
            foreach (var name in namelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(shipdata[x][name]);
                }
                custsumpair.Add(new KeyValuePair<string, double>(name, namecnt.Sum()));
            }
            custsumpair.Sort(delegate (KeyValuePair<string, double> obj1, KeyValuePair<string, double> obj2)
            { return obj2.Value.CompareTo(obj1.Value); });
            var newnamelist = new List<string>();
            foreach (var item in custsumpair)
            {
                newnamelist.Add(item.Key);
            }
            if (newnamelist.Contains("OTHERS"))
            { newnamelist.Remove("OTHERS"); newnamelist.Insert(0, "OTHERS"); }


            var lastdidx = shipdatelist.Count - 1;
            var title = shipdatelist[0] + " ~ " + shipdatelist[lastdidx] + " " + producttype + " Shipment Distribution vs VCSEL DPPM (" + rate + ")";
            if (vcselrmacntdict.Count == 0)
            { title = shipdatelist[0] + " ~ " + shipdatelist[lastdidx] + " " + producttype + " Shipment Distribution  vs DPPM  (" + rate + ")"; }

            var xdata = new List<string>();
            var ydata = new List<object>();

            var cussumlist = new List<double>();

            foreach (var f_item in shipdatelist)
            {
                xdata.Add(f_item);
            }
            var xAxis = new { data = xdata };

            var yAxis = new
            {
                title = "Amount"
            };

            var cidx = 0;
            foreach (var name in newnamelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(shipdata[x][name]);
                }

                cussumlist.Add(namecnt.Sum());

                ydata.Add(new
                {
                    name = name,
                    data = namecnt,
                    color = colorlist[cidx % colorlist.Count]
                });
                cidx += 1;
            }

            var totalship = cussumlist.Sum();
            var customerrate = new List<string>();
            cidx = 0;
            foreach (var cs in cussumlist)
            {
                customerrate.Add(newnamelist[cidx] + ":" + Math.Round(cs / totalship * 100.0, 2) + "%");
                cidx += 1;
            }
            customerrate.Add(""); customerrate.Add(""); customerrate.Add("");

            if (vcselrmacntdict.Count > 0)
            {
                var ddata = new List<double>();
                foreach (var x in shipdatelist)
                {
                    if (vcselrmacntdict.ContainsKey(x))
                    {
                        ddata.Add(Math.Round((double)vcselrmacntdict[x] / datecntdict[x] * 1000000, 0));
                    }
                    else
                    {
                        ddata.Add(0.0);
                    }
                }
                ydata.Add(new
                {
                    name = "VCSEL RMA DPPM",
                    type = "line",
                    data = ddata,
                    yAxis = 1
                });
            }

            if (allrmacntdict.Count > 0)
            {
                var ddata = new List<double>();
                foreach (var x in shipdatelist)
                {
                    if (allrmacntdict.ContainsKey(x))
                    {
                        ddata.Add(Math.Round((double)allrmacntdict[x] / datecntdict[x] * 1000000, 0));
                    }
                    else
                    {
                        ddata.Add(0.0);
                    }
                }
                ydata.Add(new
                {
                    name = "ALL RMA DPPM",
                    type = "line",
                    data = ddata,
                    yAxis = 1
                });
            }

            return new
            {
                id = id,
                title = title,
                xAxis = xAxis,
                yAxis = yAxis,
                data = ydata,
                rate = rate,
                producttype = producttype,
                customerrate = customerrate
            };
        }


        private object GetShipmentQuarterChartData(Dictionary<string, Dictionary<string, double>> shipdata, Dictionary<string, int> allrmacntdict,string rate, string producttype)
        {
            var id = "qshipdata_" + rate.Replace(".", "").Replace(" ", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";
            var qshipdict = new Dictionary<string, Dictionary<string, double>>();
            foreach (var mkv in shipdata)
            {
                var q = QuarterCLA.RetrieveQuarterFromDate(DateTime.Parse(mkv.Key));
                if (qshipdict.ContainsKey(q))
                {
                    var cdict = qshipdict[q];
                    foreach (var ckv in mkv.Value)
                    {
                        if (cdict.ContainsKey(ckv.Key))
                        {
                            cdict[ckv.Key] += ckv.Value;
                        }
                        else
                        {
                            cdict.Add(ckv.Key, ckv.Value);
                        }
                    }
                }
                else
                {
                    var cdict = new Dictionary<string, double>();
                    foreach (var ckv in mkv.Value)
                    {
                        cdict.Add(ckv.Key, ckv.Value);
                    }
                    qshipdict.Add(q, cdict);
                }
            }

            var qrmacntdict = new Dictionary<string, int>();
            foreach (var rkv in allrmacntdict)
            {
                var q = QuarterCLA.RetrieveQuarterFromDate(DateTime.Parse(rkv.Key));
                if (qrmacntdict.ContainsKey(q))
                {
                    qrmacntdict[q] += rkv.Value;
                }
                else
                {
                    qrmacntdict.Add(q, rkv.Value);
                }
            }


            var shipdatelist = qshipdict.Keys.ToList();
            shipdatelist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = QuarterCLA.RetrieveDateFromQuarter(obj1)[0];
                var d2 = QuarterCLA.RetrieveDateFromQuarter(obj2)[0];
                return d1.CompareTo(d2);
            });


            var datecntdict = new Dictionary<string, double>();
            foreach (var kv in qshipdict)
            {
                var totle = 0.0;
                foreach (var nd in kv.Value)
                {
                    totle = totle + nd.Value;
                }
                datecntdict.Add(kv.Key, totle);
            }

            var colorlist = new string[] { "#161525", "#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

            var namelist = qshipdict[shipdatelist[0]].Keys.ToList();

            var custsumpair = new List<KeyValuePair<string, double>>();
            foreach (var name in namelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(qshipdict[x][name]);
                }
                custsumpair.Add(new KeyValuePair<string, double>(name, namecnt.Sum()));
            }
            custsumpair.Sort(delegate (KeyValuePair<string, double> obj1, KeyValuePair<string, double> obj2)
            { return obj2.Value.CompareTo(obj1.Value); });
            var newnamelist = new List<string>();
            foreach (var item in custsumpair)
            {
                newnamelist.Add(item.Key);
            }
            if (newnamelist.Contains("OTHERS"))
            { newnamelist.Remove("OTHERS"); newnamelist.Insert(0, "OTHERS"); }


            var lastdidx = shipdatelist.Count - 1;
            var title = shipdatelist[0] + " ~ " + shipdatelist[lastdidx] + " " + producttype + " Shipment Distribution vs DPPM (" + rate + ")";

            var xdata = new List<string>();
            var ydata = new List<object>();

            var cussumlist = new List<double>();

            foreach (var f_item in shipdatelist)
            {
                xdata.Add(f_item);
            }
            var xAxis = new { data = xdata };

            var yAxis = new
            {
                title = "Amount"
            };

            var cidx = 0;
            foreach (var name in newnamelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(qshipdict[x][name]);
                }

                cussumlist.Add(namecnt.Sum());

                ydata.Add(new
                {
                    name = name,
                    data = namecnt,
                    color = colorlist[cidx % colorlist.Count]
                });
                cidx += 1;
            }

            var totalship = cussumlist.Sum();
            var customerrate = new List<string>();
            cidx = 0;
            foreach (var cs in cussumlist)
            {
                customerrate.Add(newnamelist[cidx] + ":" + Math.Round(cs / totalship * 100.0, 2) + "%");
                cidx += 1;
            }
            customerrate.Add(""); customerrate.Add(""); customerrate.Add("");


            if (qrmacntdict.Count > 0)
            {
                var ddata = new List<double>();
                foreach (var x in shipdatelist)
                {
                    if (qrmacntdict.ContainsKey(x))
                    {
                        ddata.Add(Math.Round((double)qrmacntdict[x] / datecntdict[x] * 1000000, 0));
                    }
                    else
                    {
                        ddata.Add(0.0);
                    }
                }
                ydata.Add(new
                {
                    name = "QUARTER RMA DPPM",
                    type = "line",
                    data = ddata,
                    yAxis = 1
                });
            }

            return new
            {
                id = id,
                title = title,
                xAxis = xAxis,
                yAxis = yAxis,
                data = ydata,
                rate = rate,
                producttype = producttype,
                customerrate = customerrate
            };
        }


        private object GetOrderQtyChartData(Dictionary<string, Dictionary<string, double>> shipdata, string rate, string producttype)
        {
            var id = "orderdata_" + rate.Replace(".", "").Replace(" ", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";
            var shipdatelist = shipdata.Keys.ToList();
            shipdatelist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + "-01 00:00:00");
                var d2 = DateTime.Parse(obj2 + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

            var datecntdict = new Dictionary<string, double>();
            foreach (var kv in shipdata)
            {
                var totle = 0.0;
                foreach (var nd in kv.Value)
                {
                    totle = totle + nd.Value;
                }
                datecntdict.Add(kv.Key, totle);
            }

            var namelist = shipdata[shipdatelist[0]].Keys.ToList();

            var custsumpair = new List<KeyValuePair<string, double>>();
            foreach (var name in namelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(shipdata[x][name]);
                }
                custsumpair.Add(new KeyValuePair<string, double>(name, namecnt.Sum()));
            }
            custsumpair.Sort(delegate (KeyValuePair<string, double> obj1, KeyValuePair<string, double> obj2)
            { return obj2.Value.CompareTo(obj1.Value); });
            var newnamelist = new List<string>();
            foreach (var item in custsumpair)
            {
                newnamelist.Add(item.Key);
            }
            if (newnamelist.Contains("OTHERS"))
            { newnamelist.Remove("OTHERS"); newnamelist.Insert(0, "OTHERS"); }

            var lastdidx = shipdatelist.Count - 1;
            var title = shipdatelist[0] + " ~ " + shipdatelist[lastdidx] + " " + producttype + " Order QTY Distribution (" + rate + ")";
            var xdata = new List<string>();
            var ydata = new List<object>();
            var cussumlist = new List<double>();

            foreach (var f_item in shipdatelist)
            {
                xdata.Add(f_item);
            }
            var xAxis = new { data = xdata };

            var yAxis = new
            {
                title = "Amount"
            };

            var colorlist = new string[] { "#161525", "#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();
            var cidx = 0;
            foreach (var name in newnamelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(shipdata[x][name]);
                }
                cussumlist.Add(namecnt.Sum());
                ydata.Add(new
                {
                    name = name,
                    data = namecnt,
                    color = colorlist[cidx % colorlist.Count]
                });
                cidx += 1;
            }

            var totalship = cussumlist.Sum();
            var customerrate = new List<string>();
            cidx = 0;
            foreach (var cs in cussumlist)
            {
                customerrate.Add(newnamelist[cidx] + ":" + Math.Round(cs / totalship * 100.0, 2) + "%");
                cidx += 1;
            }
            customerrate.Add(""); customerrate.Add(""); customerrate.Add("");

            return new
            {
                id = id,
                title = title,
                xAxis = xAxis,
                yAxis = yAxis,
                data = ydata,
                rate = rate,
                customerrate = customerrate
            };
        }


        private object GetOTDChartData(List<FsrShipData> otddata, string rate, string producttype)
        {
            var id = "otddata_" + rate.Replace(".", "").Replace(" ", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";
            var title = producttype + " Monthly OTD Bills Distribution (" + rate + ")";

            var ordernumdict = new Dictionary<string, double>();
            var otdnumdict = new Dictionary<string, double>();
            var otddict = new Dictionary<string, double>();
            var orderdatadict = new Dictionary<string, List<FsrShipData>>();

            foreach (var data in otddata)
            {
                var orderdatekey = data.OPD.ToString("yyyy-MM");
                if (ordernumdict.ContainsKey(orderdatekey))
                { ordernumdict[orderdatekey] += 1; }
                else
                { ordernumdict.Add(orderdatekey, 1); }

                if (data.ShipDate <= data.OPD)
                {
                    if (otdnumdict.ContainsKey(orderdatekey))
                    { otdnumdict[orderdatekey] += 1; }
                    else
                    { otdnumdict.Add(orderdatekey, 1); }
                    data.OTD = "OTD";
                }

                if (orderdatadict.ContainsKey(orderdatekey))
                {
                    orderdatadict[orderdatekey].Add(data);
                }
                else
                {
                    var templist = new List<FsrShipData>();
                    templist.Add(data);
                    orderdatadict.Add(orderdatekey, templist);
                }
            }

            var orderkeys = ordernumdict.Keys.ToList();
            foreach (var o in orderkeys)
            {
                if (otdnumdict.ContainsKey(o))
                { otddict.Add(o, Math.Round(otdnumdict[o] / ordernumdict[o] * 100.0, 2)); }
                else
                { otddict.Add(o, 0.0); }
            }

            orderkeys.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + "-01 00:00:00");
                var d2 = DateTime.Parse(obj2 + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

            var xdata = new List<string>();
            var ordernumchartdata = new List<double>();
            var otdchartdata = new List<double>();
            foreach (var k in orderkeys)
            {
                xdata.Add(k);
                ordernumchartdata.Add(ordernumdict[k]);
                otdchartdata.Add(otddict[k]);
            }

            var ordernumchart = new
            {
                type = "column",
                name = "orders bills",
                data = ordernumchartdata,
                yAxis = 1
            };

            var otdchart = new
            {
                type = "line",
                name = "OTD rate",
                data = otdchartdata,
                dataLabels = new
                {
                    enabled = true
                }
            };

            var chartdata = new List<object>();
            chartdata.Add(ordernumchart);
            chartdata.Add(otdchart);

            return new
            {
                id = id,
                title = title,
                xdata = xdata,
                chartdata = chartdata,
                orderdatadict = orderdatadict
            };
        }

        private object GetOTDChartDataByQTY(List<FsrShipData> otddata, string rate, string producttype)
        {
            var id = "otddata_" + rate + "_qty_id";
            var title = producttype + " Monthly OTD QTY Distribution (" + rate + ")";

            var ordernumdict = new Dictionary<string, double>();
            var otdnumdict = new Dictionary<string, double>();
            var otddict = new Dictionary<string, double>();
            var orderdatadict = new Dictionary<string, List<FsrShipData>>();

            foreach (var data in otddata)
            {
                var orderdatekey = data.OPD.ToString("yyyy-MM");
                if (ordernumdict.ContainsKey(orderdatekey))
                { ordernumdict[orderdatekey] += data.OrderQty; }
                else
                { ordernumdict.Add(orderdatekey, data.OrderQty); }

                if (data.ShipDate <= data.OPD)
                {
                    if (otdnumdict.ContainsKey(orderdatekey))
                    { otdnumdict[orderdatekey] += data.OrderQty; }
                    else
                    { otdnumdict.Add(orderdatekey, data.OrderQty); }
                    data.OTD = "OTD";
                }

                if (orderdatadict.ContainsKey(orderdatekey))
                {
                    orderdatadict[orderdatekey].Add(data);
                }
                else
                {
                    var templist = new List<FsrShipData>();
                    templist.Add(data);
                    orderdatadict.Add(orderdatekey, templist);
                }
            }

            var orderkeys = ordernumdict.Keys.ToList();
            foreach (var o in orderkeys)
            {
                if (otdnumdict.ContainsKey(o))
                { otddict.Add(o, Math.Round(otdnumdict[o] / ordernumdict[o] * 100.0, 2)); }
                else
                { otddict.Add(o, 0.0); }
            }

            orderkeys.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + "-01 00:00:00");
                var d2 = DateTime.Parse(obj2 + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

            var xdata = new List<string>();
            var ordernumchartdata = new List<double>();
            var otdchartdata = new List<double>();
            foreach (var k in orderkeys)
            {
                xdata.Add(k);
                ordernumchartdata.Add(ordernumdict[k]);
                otdchartdata.Add(otddict[k]);
            }

            var ordernumchart = new
            {
                type = "column",
                name = "orders qty",
                data = ordernumchartdata,
                yAxis = 1
            };

            var otdchart = new
            {
                type = "line",
                name = "OTD rate",
                data = otdchartdata,
                dataLabels = new
                {
                    enabled = true
                }
            };

            var chartdata = new List<object>();
            chartdata.Add(ordernumchart);
            chartdata.Add(otdchart);

            return new
            {
                id = id,
                title = title,
                xdata = xdata,
                chartdata = chartdata,
                orderdatadict = orderdatadict
            };
        }

        public JsonResult ShipmentDistribution()
        {
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
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            //<date,<customer,int>>
            var shipdata25g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var shipdata14g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var shipdataarray = new List<object>();
            if (shipdata25g.Count > 0)
            {
                var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
                //var allrmacntdict = ExternalDataCollector.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentChartData(shipdata25g, vcselrmacntdict, allrmacntdict, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
            }
            if (shipdata14g.Count > 0)
            {
                var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
                //var allrmacntdict = ExternalDataCollector.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentChartData(shipdata14g, vcselrmacntdict, allrmacntdict, "10G_14G", SHIPPRODTYPE.PARALLEL));
            }

            var allparallelship = FsrShipData.RetrieveShipDataByMonth("ALL", SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (allparallelship.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = RMADppmData.RetrieveParallelRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));
                shipdataarray.Add(GetShipmentChartData(allparallelship, vcselrmacntdict, allrmacntdict, "ALL", SHIPPRODTYPE.PARALLEL));
                shipdataarray.Add(GetShipmentQuarterChartData(allparallelship, allrmacntdict, "ALL", SHIPPRODTYPE.PARALLEL));
            }

            var tunableshipdata = FsrShipData.RetrieveShipDataByMonth("", SHIPPRODTYPE.TUNABLE,startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (tunableshipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = RMADppmData.RetrieveTunableRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));
                shipdataarray.Add(GetShipmentChartData(tunableshipdata, vcselrmacntdict, allrmacntdict, "tunable", SHIPPRODTYPE.TUNABLE));
                shipdataarray.Add(GetShipmentQuarterChartData(tunableshipdata, allrmacntdict, "tunable", SHIPPRODTYPE.TUNABLE));
            }

            var sfpwireshipdata = FsrShipData.RetrieveShipDataByMonth("", SHIPPRODTYPE.SFPWIRE, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (sfpwireshipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentChartData(sfpwireshipdata, vcselrmacntdict, allrmacntdict, SHIPPRODTYPE.SFPWIRE, SHIPPRODTYPE.SFPWIRE));
                shipdataarray.Add(GetShipmentQuarterChartData(sfpwireshipdata, allrmacntdict, SHIPPRODTYPE.SFPWIRE, SHIPPRODTYPE.SFPWIRE));
            }

            var linecardshipdata = FsrShipData.RetrieveLineCardShipDataByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (linecardshipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentQuarterChartData(linecardshipdata, allrmacntdict, "linecard", "LINECARD"));
            }

            var edfashipdata = FsrShipData.RetrieveShipDataByMonth("", SHIPPRODTYPE.RED_C, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (edfashipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentQuarterChartData(edfashipdata, allrmacntdict, "EDFA", SHIPPRODTYPE.RED_C));
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                shipdataarray = shipdataarray
            };
            return ret;
        }

        private void PrepareShipmentData(System.IO.FileStream fw, string ssdate, string sedate)
        {
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;

            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(ssdate);
                var edate = DateTime.Parse(sedate);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var shipdata = FsrShipData.RetrieveAllShipDataByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var sb = new StringBuilder(shipdata.Count * 200);
            sb.Append("ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,ShipDate,Original Promise Date,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum,VcselType,\r\n");
            foreach (var item in shipdata)
            {
                sb.Append("\"" + item.ShipID.Replace("\"", "") + "\"," + "\"" + item.ShipQty.ToString().Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
                    + "\"" + item.ProdDesc.Replace("\"", "") + "\"," + "\"" + item.MarketFamily.Replace("\"", "") + "\"," + "\"" + item.Configuration.Replace("\"", "") + "\","
                    + "\"" + item.ShipDate.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," + "\"" + item.OPD.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\","
                    + "\"" + item.CustomerNum.Replace("\"", "") + "\"," + "\"" + item.Customer1.Replace("\"", "") + "\"," + "\"" + item.Customer2.Replace("\"", "") + "\","
                    + "\"" + item.OrderedDate.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," + "\"" + item.DelieveNum.Replace("\"", "") + "\","
                    + "\"" + item.VcselType.Replace("\"", "") + "\",\r\n");
            }

            var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            fw.Write(bt, 0, bt.Count());
            sb.Clear();
        }

        public ActionResult DownloadShipmentData(string sdate, string edate)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!System.IO.Directory.Exists(imgdir))
            {
                System.IO.Directory.CreateDirectory(imgdir);
            }

            var fn = "shipment_" + sdate + "-" + edate + "_" + "_data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var fw = System.IO.File.OpenWrite(filename);
            PrepareShipmentData(fw, sdate, edate);
            fw.Close();


            return File(filename, "application/vnd.ms-excel", fn);
        }

        public JsonResult RetrieveVcselRMARawDataByMonth()
        {
            var datestr = Request.Form["datestr"];
            var rate = Request.Form["rate"];
            var sdate = datestr + "-01 00:00:00";
            var edate = DateTime.Parse(sdate).AddMonths(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
            var waferdatalist = VcselRMAData.RetrieveWaferRawDataByMonth(sdate, edate, rate);
            var ret = new JsonResult();
            ret.Data = new { waferdatalist = waferdatalist };
            return ret;
        }

        public JsonResult RetrieveRMARawDataByMonth()
        {
            var pdtype = Request.Form["pdtype"];
            var datestr = Request.Form["datestr"];
            var rmadatalist = new List<RMADppmData>();
            if (datestr.ToUpper().Contains("Q"))
            {
                var datelist = QuarterCLA.RetrieveDateFromQuarter(datestr);
                rmadatalist = RMADppmData.RetrieveRMARawDataByMonth(datelist[0].ToString("yyyy-MM-dd HH:mm:ss"), datelist[1].ToString("yyyy-MM-dd HH:mm:ss"), pdtype);
            }
            else
            {
                var sdate = datestr + "-01 00:00:00";
                var edate = DateTime.Parse(sdate).AddMonths(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                rmadatalist = RMADppmData.RetrieveRMARawDataByMonth(sdate, edate,pdtype);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new { rmadatalist = rmadatalist };
            return ret;
        }

        public ActionResult OTDData()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        public JsonResult OTDDistribution()
        {
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
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var otdarray = new List<object>();
            //var otd25g = FsrShipData.RetrieveOTDByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            //var otd14g = FsrShipData.RetrieveOTDByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            //if (otd25g.Count > 0)
            //{
            //    otdarray.Add(GetOTDChartData(otd25g, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
            //}
            //if (otd14g.Count > 0)
            //{
            //    otdarray.Add(GetOTDChartData(otd14g, "10G_14G", SHIPPRODTYPE.PARALLEL));
            //}
            var parallelorderdata = FsrShipData.RetrieveOTDByMonth( SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (parallelorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartData(parallelorderdata, "parallel", SHIPPRODTYPE.PARALLEL));
            }

            if (parallelorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartDataByQTY(parallelorderdata, "parallel", SHIPPRODTYPE.PARALLEL));
            }

            var tunableorderdata = FsrShipData.RetrieveOTDByMonth( SHIPPRODTYPE.TUNABLE, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (tunableorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartData(tunableorderdata, "tunable", SHIPPRODTYPE.TUNABLE));
            }

            if (tunableorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartDataByQTY(tunableorderdata, "tunable", SHIPPRODTYPE.TUNABLE));
            }


            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                otdarray = otdarray
            };
            return ret;

        }


        public ActionResult LBSDistribution()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }


        public JsonResult ShipmentLBSDistribution()
        {
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
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var capitaldict = CapitalData.LoadCapitialData(this);
            var WUXI = new CapitalData();
            WUXI.code = "CN";
            WUXI.ctname = "wux";
            WUXI.lat = 31.5653;
            WUXI.lon = 120.327;

            var chartarray = new List<object>();
            var parallelshiplbsdata = ShipLBSData.LoadShipdataLBS(SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"),this);
            var maxval = 2.0;
            var capitallist = new List<object>();


            foreach (var item in parallelshiplbsdata)
            {
                if (item.value > maxval)
                { maxval = item.value; }

                if (item.value > 1 && capitaldict.ContainsKey(item.code))
                {
                    capitallist.Add(new {
                        id = capitaldict[item.code].ctname,
                        lat = capitaldict[item.code].lat,
                        lon = capitaldict[item.code].lon,
                    });

                }
            }
            capitallist.Add(new
            {
                id = WUXI.ctname,
                lat = WUXI.lat,
                lon = WUXI.lon,
            });
            chartarray.Add(new
            {
                id = "ship_para_lbs_id",
                title = "Parallel Product Shipment Distribution",
                data = parallelshiplbsdata,
                capitallist = capitallist,
                maxval = maxval
            });

            var tunableshiplbsdata = ShipLBSData.LoadShipdataLBS(SHIPPRODTYPE.TUNABLE, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            maxval = 2.0;
            capitallist = new List<object>();
            foreach (var item in tunableshiplbsdata)
            {
                if (item.value > maxval)
                { maxval = item.value; }
                if (item.value > 1 && capitaldict.ContainsKey(item.code))
                {
                    capitallist.Add(new
                    {
                        id = capitaldict[item.code].ctname,
                        lat = capitaldict[item.code].lat,
                        lon = capitaldict[item.code].lon,
                    });

                }
            }
            capitallist.Add(new
            {
                id = WUXI.ctname,
                lat = WUXI.lat,
                lon = WUXI.lon,
            });
            chartarray.Add(new
            {
                id = "ship_tunable_lbs_id",
                title = "Tunable Product Shipment Distribution",
                data = tunableshiplbsdata,
                capitallist = capitallist,
                maxval = maxval
            });


            var sfpwireshiplbsdata = ShipLBSData.LoadShipdataLBS(SHIPPRODTYPE.SFPWIRE, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            maxval = 2.0;
            capitallist = new List<object>();
            foreach (var item in sfpwireshiplbsdata)
            {
                if (item.value > maxval)
                { maxval = item.value; }
                if (item.value > 1 && capitaldict.ContainsKey(item.code))
                {
                    capitallist.Add(new
                    {
                        id = capitaldict[item.code].ctname,
                        lat = capitaldict[item.code].lat,
                        lon = capitaldict[item.code].lon,
                    });

                }
            }
            capitallist.Add(new
            {
                id = WUXI.ctname,
                lat = WUXI.lat,
                lon = WUXI.lon,
            });
            chartarray.Add(new
            {
                id = "ship_sfpwire_lbs_id",
                title = "SFP+ Wire Product Shipment Distribution",
                data = sfpwireshiplbsdata,
                capitallist = capitallist,
                maxval = maxval
            });


            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                chartarray = chartarray
            };
            return ret;
        }

        public ActionResult OrderData()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

            return View();
        }

        public JsonResult OrderDistribution()
        {
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
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }


            var orderdata25g = FsrShipData.RetrieveOrderDataByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var orderdata14g = FsrShipData.RetrieveOrderDataByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var orderdataarray = new List<object>();
            if (orderdata25g.Count > 0)
            {
                orderdataarray.Add(GetOrderQtyChartData(orderdata25g, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
            }
            if (orderdata14g.Count > 0)
            {
                orderdataarray.Add(GetOrderQtyChartData(orderdata14g, "10G_14G", SHIPPRODTYPE.PARALLEL));
            }
            var tunableorderdata = FsrShipData.RetrieveOrderDataByMonth("", SHIPPRODTYPE.TUNABLE, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (tunableorderdata.Count > 0)
            {
                orderdataarray.Add(GetOrderQtyChartData(tunableorderdata, "tunable", SHIPPRODTYPE.TUNABLE));
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                orderdataarray = orderdataarray
            };
            return ret;
        }

        private int GetWorkingDays(DateTime d1, DateTime d2)
        {
            var days = 1;
            if (d1.DayOfWeek == DayOfWeek.Saturday
                || d1.DayOfWeek == DayOfWeek.Sunday)
            { days = 0; }

            var tempd = d1.AddDays(1);
            while (tempd < d2)
            {
                var m = Convert.ToInt32(tempd.ToString("MM"));
                var d = Convert.ToInt32(tempd.ToString("dd"));

                if (tempd.DayOfWeek == DayOfWeek.Saturday
                    || tempd.DayOfWeek == DayOfWeek.Sunday
                    ||(m == 10 && d < 8))
                {
                    tempd = tempd.AddDays(1);
                    continue;
                }
                days += 1;
                tempd = tempd.AddDays(1);
            }

            return days;
        }

        private object GetWorkLoadChart(List<RMADppmData> workloaddata, string mark, string producttype)
        {
            var wdict = new Dictionary<string, RMADppmData>();
            //combine same RMA
            foreach (var item in workloaddata)
            {
                if (string.Compare(item.IssueDateStr, "1982-05-06") == 0 || string.Compare(item.InitFARStr, "1982-05-06") == 0)
                { continue; }
                if (item.IssueOpenDate > item.InitFAR)
                { continue; }

                if (wdict.ContainsKey(item.RMANum))
                {
                    if (item.InitFAR > wdict[item.RMANum].InitFAR)
                    { wdict[item.RMANum].InitFAR = item.InitFAR; }
                }
                else
                {
                    var d = new RMADppmData();
                    d.RMANum = item.RMANum;
                    d.IssueOpenDate = item.IssueOpenDate;
                    d.InitFAR = item.InitFAR;
                    wdict.Add(item.RMANum,d);
                }
            }

            //collect RMA by Quarter
            var qdict = new Dictionary<string, List<RMADppmData>>();
            foreach (var kv in wdict)
            {
                var q = QuarterCLA.RetrieveQuarterFromDate(kv.Value.IssueOpenDate);
                if (qdict.ContainsKey(q))
                {
                    qdict[q].Add(kv.Value);
                }
                else
                {
                    var templist = new List<RMADppmData>();
                    templist.Add(kv.Value);
                    qdict.Add(q, templist);
                }
            }

            var qlist = qdict.Keys.ToList();
            qlist.Sort(delegate (string q1,string q2) {
                var d1 = QuarterCLA.RetrieveDateFromQuarter(q1);
                var d2 = QuarterCLA.RetrieveDateFromQuarter(q2);
                return d1[0].CompareTo(d2[0]);
            });

            //start to get chart data
            var rmacnt = new List<int>();
            var avgworkload = new List<double>();
            foreach (var q in qlist)
            {
                var wklist = qdict[q];
                rmacnt.Add(wklist.Count);
                var sumdays = 0.0;
                foreach (var wk in wklist)
                {
                    sumdays += GetWorkingDays(wk.IssueOpenDate,wk.InitFAR);
                }
                avgworkload.Add(Math.Round(sumdays / wklist.Count,2));
            }

            var id = "wkload_" + mark.Replace(".", "").Replace(" ", "")
                    .Replace("(", "").Replace(")", "").Replace("#", "").Replace("+", "").ToLower() + "_id";
            var title = mark + " RMA Quarter WorkLoad";

            var colorlist = new string[] { "#161525","#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

            var wkloadchart = new
            {
                type = "line",
                name = "Avg Spending Days",
                data = avgworkload,
                dataLabels = new
                {
                    enabled = true
                },
                color = colorlist[0]
            };
            var rmacntchart = new
            {
                type = "column",
                name = "RMA Count",
                data = rmacnt,
                yAxis = 1,
                color = colorlist[1]
            };
            var chartdata = new List<object>();
            chartdata.Add(rmacntchart);
            chartdata.Add(wkloadchart);

            return new
            {
                id = id,
                title = title,
                xdata = qlist,
                chartdata = chartdata,
                producttype = producttype
            };

        }

        public ActionResult RMAWorkLoad()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        public JsonResult RMAWorkLoadData()
        {
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
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var chartarray = new List<object>();
            var parallelrmadata = RMADppmData.RetrieveRMAWorkLoadDataByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), SHIPPRODTYPE.PARALLEL);
            var tunablermadata = RMADppmData.RetrieveRMAWorkLoadDataByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), SHIPPRODTYPE.TUNABLE);

            if (parallelrmadata.Count > 0)
            {
                chartarray.Add(GetWorkLoadChart(parallelrmadata, "PARALLEL", SHIPPRODTYPE.PARALLEL));
            }
            if (tunablermadata.Count > 0)
            {
                chartarray.Add(GetWorkLoadChart(tunablermadata, "TUNABLE", SHIPPRODTYPE.TUNABLE));
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                chartarray = chartarray
            };
            return ret;
        }


        public JsonResult RetrieveRMAWorkLoadDataByMonth()
        {
            var pdtype = Request.Form["pdtype"];
            var datestr = Request.Form["datestr"];
            var rmadatalist = new List<RMADppmData>();
            if (datestr.ToUpper().Contains("Q"))
            {
                var datelist = QuarterCLA.RetrieveDateFromQuarter(datestr);
                rmadatalist = RMADppmData.RetrieveRMAWorkLoadDataByMonth(datelist[0].ToString("yyyy-MM-dd HH:mm:ss"), datelist[1].ToString("yyyy-MM-dd HH:mm:ss"), pdtype);
            }
            else
            {
                var sdate = datestr + "-01 00:00:00";
                var edate = DateTime.Parse(sdate).AddMonths(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                rmadatalist = RMADppmData.RetrieveRMAWorkLoadDataByMonth(sdate, edate, pdtype);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new { rmadatalist = rmadatalist };
            return ret;
        }

        public ActionResult ShipOutputTrend()
        {
            return View();
        }

        public JsonResult ShipOutputTrendData()
        {
            var ssdate = Request.Form["sdate"];
            var sedate = Request.Form["edate"];
            if (string.IsNullOrEmpty(ssdate) || string.IsNullOrEmpty(sedate))
            {
                ssdate = "2018-05-01 00:00:00";
                sedate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                var startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                var enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                var sq = QuarterCLA.RetrieveQuarterFromDate(startdate);
                var eq = QuarterCLA.RetrieveQuarterFromDate(enddate);
                ssdate = QuarterCLA.RetrieveDateFromQuarter(sq)[0].ToString("yyyy-MM-dd HH:mm:ss");
                sedate = QuarterCLA.RetrieveDateFromQuarter(eq)[1].ToString("yyyy-MM-dd HH:mm:ss");
            }


            var chartlist = new List<object>();
            var shipdata = ScrapData_Base.RetrieveAllOutputData();
            chartlist.Add(ShipOutputChartData(shipdata,"scrapout_id", "Department Output"));

            shipdata = FsrShipData.RetrieveOutputData(this,ssdate,sedate);
            chartlist.Add(ShipOutputChartData(shipdata,"shipout_id", "Department Ship Output"));

            var ret = new JsonResult();
            ret.Data = new
            {
                chartlist = chartlist
            };
            return ret;
        }


        private object ShipOutputChartData(Dictionary<string,Dictionary<string,double>> shipdata,string id,string title)
        {
            var colorlist = new string[] { "#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

            
            var xlist = shipdata.Keys.ToList();
            var qdict = new Dictionary<string, bool>();
            foreach (var skv in shipdata)
            {
                foreach (var qkv in skv.Value)
                {
                    if (!qdict.ContainsKey(qkv.Key))
                    { qdict.Add(qkv.Key,true); }
                }
            }

            var qlist = qdict.Keys.ToList();
            qlist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = QuarterCLA.RetrieveDateFromQuarter(obj1)[0];
                var d2 = QuarterCLA.RetrieveDateFromQuarter(obj2)[0];
                return d1.CompareTo(d2);
            });

            var cidx = 0;
            var chartseris = new List<object>();
            foreach (var q in qlist)
            {
                var qdatalist = new List<double>();
                foreach (var skv in shipdata)
                {
                    if (skv.Value.ContainsKey(q))
                    {
                        qdatalist.Add(Math.Round(skv.Value[q],0));
                    }
                    else
                    {
                        qdatalist.Add(0.0);
                    }
                }
                chartseris.Add(new
                {
                    name = q,
                    type = "column",
                    data = qdatalist,
                    color = colorlist[cidx%colorlist.Count]
                });
                cidx++;
            }

            var chartdata = new
            {
                id = id,
                title = title,
                xlist = xlist,
                chartseris = chartseris
            };

            return chartdata;

        }

        public JsonResult AllOutputDetailData(string dp,string qt)
        {
            var shipoutlist = ScrapData_Base.RetrieveOutputData(dp, qt);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                shipoutlist = shipoutlist
            };
            return ret;
        }
        
        public JsonResult ShipOutputDetailData(string dp, string qt)
        {
            var startdate = QuarterCLA.RetrieveDateFromQuarter(qt)[0].ToString("yyyy-MM-dd HH:mm:ss");
            var enddate = QuarterCLA.RetrieveDateFromQuarter(qt)[1].ToString("yyyy-MM-dd HH:mm:ss");
            var shipoutlist = FsrShipData.RetrieveOutputDetailData(this, dp, startdate, enddate);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                shipoutlist = shipoutlist
            };
            return ret;
        }

    }
}