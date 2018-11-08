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
        public static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        public ActionResult ShipmentData()
        {
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, null, 9))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.IsL9Employee = MachineUserMap.IsLxEmployee(compName, null, 10);
            return View();
        }

        ////<date,<customer,int>>
        private object GetShipmentChartData(Dictionary<string, Dictionary<string, double>> shipdata, Dictionary<string, int> vcselrmacntdict, Dictionary<string, int> allrmacntdict, string rate, string producttype)
        {
            var id = "shipdata_" + rate + "_id";
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
                    color = colorlist[cidx]
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
            var id = "qshipdata_" + rate + "_id";
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
                    color = colorlist[cidx]
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
            var id = "orderdata_" + rate + "_id";
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
                    color = colorlist[cidx]
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
            var id = "otddata_" + rate + "_id";
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

            var tunableshipdata = FsrShipData.RetrieveShipDataByMonth("",SHIPPRODTYPE.OPTIUM,startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (tunableshipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = RMADppmData.RetrieveTunableRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));
                shipdataarray.Add(GetShipmentChartData(tunableshipdata, vcselrmacntdict, allrmacntdict, "tunable", SHIPPRODTYPE.OPTIUM));
                shipdataarray.Add(GetShipmentQuarterChartData(tunableshipdata, allrmacntdict, "tunable", SHIPPRODTYPE.OPTIUM));
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
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, null, 10))
            {
                return RedirectToAction("UserCenter", "User");
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
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, null, 9))
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
            var parallelorderdata = FsrShipData.RetrieveOTDByMonth("", SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (parallelorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartData(parallelorderdata, "parallel", SHIPPRODTYPE.PARALLEL));
            }

            if (parallelorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartDataByQTY(parallelorderdata, "parallel", SHIPPRODTYPE.PARALLEL));
            }

            var tunableorderdata = FsrShipData.RetrieveOTDByMonth("", SHIPPRODTYPE.OPTIUM, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (tunableorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartData(tunableorderdata, "tunable", SHIPPRODTYPE.OPTIUM));
            }

            if (tunableorderdata.Count > 0)
            {
                otdarray.Add(GetOTDChartDataByQTY(tunableorderdata, "tunable", SHIPPRODTYPE.OPTIUM));
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
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, null, 9))
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

            var tunableshiplbsdata = ShipLBSData.LoadShipdataLBS(SHIPPRODTYPE.OPTIUM, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
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
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, null, 9))
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
            var tunableorderdata = FsrShipData.RetrieveOrderDataByMonth("", SHIPPRODTYPE.OPTIUM, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (tunableorderdata.Count > 0)
            {
                orderdataarray.Add(GetOrderQtyChartData(tunableorderdata, "tunable", SHIPPRODTYPE.OPTIUM));
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


    }
}