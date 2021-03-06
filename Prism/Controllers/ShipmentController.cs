﻿using System;
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
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ShipmentData"))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.IsL9Employee = MachineUserMap.IsLxEmployee(Request.UserHostName, null, 10);
            return View();
        }

        ////<date,<customer,int>>
        private object GetShipmentChartData(Dictionary<string, Dictionary<string, double>> shipdata, Dictionary<string, int> vcselrmacntdict, Dictionary<string, int> allrmacntdict
            , string rate, string producttype, Dictionary<string, int> rma25gcntdict=null, Dictionary<string, int> rma10gcntdict=null)
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

            var colorlist = new string[] { "#161525", "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
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
                    namecnt.Add(shipdata[x][name]);
                }

                cussumlist.Add(namecnt.Sum());

                ydata.Add(new
                {
                    name = name,
                    data = namecnt,
                    color = colorlist[cidx % colorlist.Count],
                    visible = false,
                    maxPointWidth = 80
                });
                cidx += 1;
            }

            var totaldata = new List<double>();
            foreach (var x in shipdatelist)
            {
                totaldata.Add(datecntdict[x]);
            }
            ydata.Add(new
            {
                name = "Total Shipment",
                type = "column",
                data = totaldata,
                yAxis = 0,
                color= "#0053A2",
                maxPointWidth = 80
            });

            var totalship = cussumlist.Sum();
            var customerrate = new List<string>();
            cidx = 0;
            foreach (var cs in cussumlist)
            {
                customerrate.Add(newnamelist[cidx] + ":" + Math.Round(cs / totalship * 100.0, 2) + "%");
                cidx += 1;
            }
            customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add("");

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
                    yAxis = 1,
                    lineWidth = 2,
                    color= "#ac1911"//"#eb6100"ac1911
                });
            }

            if (rma25gcntdict!= null && rma25gcntdict.Count > 0)
            {
                var ddata = new List<double>();
                foreach (var x in shipdatelist)
                {
                    if (rma25gcntdict.ContainsKey(x))
                    {
                        ddata.Add(Math.Round((double)rma25gcntdict[x] / datecntdict[x] * 1000000, 0));
                    }
                    else
                    {
                        ddata.Add(0.0);
                    }
                }
                ydata.Add(new
                {
                    name = "25G RMA DPPM",
                    type = "line",
                    data = ddata,
                    yAxis = 1,
                    dashStyle="shortdash",
                    color = "#749dd2"
                });
            }

            if (rma10gcntdict != null && rma10gcntdict.Count > 0)
            {
                var ddata = new List<double>();
                foreach (var x in shipdatelist)
                {
                    if (rma10gcntdict.ContainsKey(x))
                    {
                        ddata.Add(Math.Round((double)rma10gcntdict[x] / datecntdict[x] * 1000000, 0));
                    }
                    else
                    {
                        ddata.Add(0.0);
                    }
                }
                ydata.Add(new
                {
                    name = "10G RMA DPPM",
                    type = "line",
                    data = ddata,
                    yAxis = 1,
                    dashStyle= "shortdash",
                    color = "#eb6100"//"#eb6100"ac1911
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


        private object GetShipmentQuarterChartData(Dictionary<string, Dictionary<string, double>> shipdata, Dictionary<string, int> allrmacntdict
            ,string rate, string producttype, Dictionary<string, int> rma25gcntdict = null, Dictionary<string, int> rma10gcntdict = null)
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

            var qrma25gcntdict = new Dictionary<string, int>();
            if (rma25gcntdict != null)
            {
                foreach (var rkv in rma25gcntdict)
                {
                    var q = QuarterCLA.RetrieveQuarterFromDate(DateTime.Parse(rkv.Key));
                    if (qrma25gcntdict.ContainsKey(q))
                    {
                        qrma25gcntdict[q] += rkv.Value;
                    }
                    else
                    {
                        qrma25gcntdict.Add(q, rkv.Value);
                    }
                }
            }

            var qrma10gcntdict = new Dictionary<string, int>();
            if (rma10gcntdict != null)
            {
                foreach (var rkv in rma10gcntdict)
                {
                    var q = QuarterCLA.RetrieveQuarterFromDate(DateTime.Parse(rkv.Key));
                    if (qrma10gcntdict.ContainsKey(q))
                    {
                        qrma10gcntdict[q] += rkv.Value;
                    }
                    else
                    {
                        qrma10gcntdict.Add(q, rkv.Value);
                    }
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

            var colorlist = new string[] { "#161525", "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
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
                    color = colorlist[cidx % colorlist.Count],
                    visible = false,
                    maxPointWidth = 80
                });
                cidx += 1;
            }

            var totaldata = new List<double>();
            foreach (var x in shipdatelist)
            {
                totaldata.Add(datecntdict[x]);
            }
            ydata.Add(new
            {
                name = "Total Shipment",
                type = "column",
                data = totaldata,
                yAxis = 0,
                color = "#0053A2",
                maxPointWidth = 80
            });

            var totalship = cussumlist.Sum();
            var customerrate = new List<string>();
            cidx = 0;
            foreach (var cs in cussumlist)
            {
                customerrate.Add(newnamelist[cidx] + ":" + Math.Round(cs / totalship * 100.0, 2) + "%");
                cidx += 1;
            }
            customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add("");


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
                    yAxis = 1,
                    lineWidth = 2,
                    color = "#ac1911"//"#eb6100"
                });
            }

            if (qrma25gcntdict.Count > 0)
            {
                var ddata = new List<double>();
                foreach (var x in shipdatelist)
                {
                    if (qrma25gcntdict.ContainsKey(x))
                    {
                        ddata.Add(Math.Round((double)qrma25gcntdict[x] / datecntdict[x] * 1000000, 0));
                    }
                    else
                    {
                        ddata.Add(0.0);
                    }
                }
                ydata.Add(new
                {
                    name = "QUARTER 25G DPPM",
                    type = "line",
                    data = ddata,
                    yAxis = 1,
                    dashStyle = "shortdash",
                    color = "#749dd2"
                });
            }

            if (qrma10gcntdict.Count > 0)
            {
                var ddata = new List<double>();
                foreach (var x in shipdatelist)
                {
                    if (qrma10gcntdict.ContainsKey(x))
                    {
                        ddata.Add(Math.Round((double)qrma10gcntdict[x] / datecntdict[x] * 1000000, 0));
                    }
                    else
                    {
                        ddata.Add(0.0);
                    }
                }
                ydata.Add(new
                {
                    name = "QUARTER 10G DPPM",
                    type = "line",
                    data = ddata,
                    yAxis = 1,
                    dashStyle = "shortdash",
                    color = "#eb6100"//"#eb6100"ac1911
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

            var colorlist = new string[] { "#161525", "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
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
                    color = colorlist[cidx % colorlist.Count],
                    maxPointWidth = 80
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
            customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add(""); customerrate.Add("");

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
            //var shipdata25g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            //var shipdata14g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var shipdataarray = new List<object>();
            //if (shipdata25g.Count > 0)
            //{
            //    var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
            //    var allrmacntdict = new Dictionary<string, int>();
            //    shipdataarray.Add(GetShipmentChartData(shipdata25g, vcselrmacntdict, allrmacntdict, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
            //}
            //if (shipdata14g.Count > 0)
            //{
            //    var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
            //    var allrmacntdict = new Dictionary<string, int>();
            //    shipdataarray.Add(GetShipmentChartData(shipdata14g, vcselrmacntdict, allrmacntdict, "10G_14G", SHIPPRODTYPE.PARALLEL));
            //}

            var allparallelship = FsrShipData.RetrieveShipDataByMonth("ALL", SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (allparallelship.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var rmacntdicts = RMADppmData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"),SHIPPRODTYPE.PARALLEL);
                var allrmacntdict = (Dictionary<string, int>)rmacntdicts[0];
                var rma25gcntdict = (Dictionary<string, int>)rmacntdicts[1];
                var rma10gcntdict = (Dictionary<string, int>)rmacntdicts[2];

                shipdataarray.Add(GetShipmentChartData(allparallelship, vcselrmacntdict, allrmacntdict, "ALL", SHIPPRODTYPE.PARALLEL,rma25gcntdict,rma10gcntdict));
                shipdataarray.Add(GetShipmentQuarterChartData(allparallelship, allrmacntdict, "ALL", SHIPPRODTYPE.PARALLEL, rma25gcntdict, rma10gcntdict));
            }

            var tunableshipdata = FsrShipData.RetrieveShipDataByMonth("", SHIPPRODTYPE.TUNABLE,startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (tunableshipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var rmacntdicts = RMADppmData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"),SHIPPRODTYPE.TUNABLE);
                var allrmacntdict = (Dictionary<string, int>)rmacntdicts[0];
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

            var coherentshipdata = FsrShipData.RetrieveShipDataByMonth("", "COHERENT", startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (coherentshipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentQuarterChartData(coherentshipdata, allrmacntdict, "COHERENT", "COHERENT"));
            }

            var linecardshipdata = FsrShipData.RetrieveLineCardShipDataByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (linecardshipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentQuarterChartData(linecardshipdata, allrmacntdict, "linecard", "LINECARD"));
            }

            var edfashipdata = FsrShipData.RetrieveShipDataByMonth("", SHIPPRODTYPE.EDFA, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (edfashipdata.Count > 0)
            {
                var vcselrmacntdict = new Dictionary<string, int>();
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentQuarterChartData(edfashipdata, allrmacntdict, "EDFA", SHIPPRODTYPE.EDFA));
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
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "DownloadShipmentData"))
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
            var rmadatalist = new List<RMASumData>();
            if (datestr.ToUpper().Contains("Q"))
            {
                var datelist = QuarterCLA.RetrieveDateFromQuarter(datestr);
                rmadatalist = RMASumData.RetrieveRMARawDataByMonth(datelist[0].ToString("yyyy-MM-dd HH:mm:ss"), datelist[1].ToString("yyyy-MM-dd HH:mm:ss"), pdtype,this);
            }
            else
            {
                var sdate = datestr + "-01 00:00:00";
                var edate = DateTime.Parse(sdate).AddMonths(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                rmadatalist = RMASumData.RetrieveRMARawDataByMonth(sdate, edate,pdtype,this);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new { rmadatalist = rmadatalist };
            return ret;
        }

        public ActionResult OTDData()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "OTDData"))
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
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "LBSDistribution"))
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
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "OrderData"))
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

            var colorlist = new string[] { "#ac1911","#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
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
                color = colorlist[1],
                maxPointWidth = 80
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
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "RMAWorkLoad"))
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
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ShipOutputTrend"))
            {
                return RedirectToAction("Index", "Main");
            }
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
            //var shipdata = ScrapData_Base.RetrieveAllOutputData();
            //chartlist.Add(ShipOutputChartData(shipdata,"scrapout_id", "Department Output"));

            var shipdata = FsrShipData.RetrieveOutputData(this,ssdate,sedate);
            chartlist.Add(ShipOutputChartData(shipdata,"shipout_id", "Department Ship Output"));

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                chartlist = chartlist
            };
            return ret;
        }


        private object ShipOutputChartData(Dictionary<string,Dictionary<string,double>> shipdata,string id,string title)
        {
            //var colorlist = new string[] { "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
            //    , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
            //    , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();
            var colorlist = new string[] {"#03519f", "#749dd2", "#3c92ba", "#645c87","#ff4500","#84a370", "#ebc843",  "#EB6100", "#E4007F"
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


        public ActionResult Forecast()
        {
            return View();
        }

        public ActionResult ForecastPLM()
        {
            return View();
        }

        public ActionResult ForecastMerge()
        {
            return View();
        }

        public JsonResult ForecastAccuracyData()
        {
            var starttime = "2018-11-01 00:00:00";
            var endtime = "2019-11-01 00:00:00";
            var startdate = Request.Form["startdate"];

            if (string.IsNullOrEmpty(startdate))
            {
                starttime = DateTime.Now.AddMonths(-12).ToString("yyyy-MM") + "-01 00:00:00";
                endtime = DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00";
            }
            else
            {
                var sdate = DateTime.Parse(Request.Form["startdate"]);
                var sdate1 = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                starttime = sdate1.ToString("yyyy-MM-dd HH:mm:ss");
                endtime = sdate1.AddMonths(12).ToString("yyyy-MM-dd HH:mm:ss");
            }

            var plmdict = PNBUMap.GetSeriesPLMDict();

            var wssdict = PNBUMap.GetWSSSeries();
            var osaseriesdict = OSASeriesData.GetOSASeriesDict();
            var activeseries = PNBUMap.GetActiveSeries(starttime,endtime);
            foreach (var ser in activeseries)
            {
                var shipfclist = new List<ShipForcastData>();
                ser.Accuracy = Math.Round(ShipForcastData.GetSeriesAccuracyVal(ser.Series, osaseriesdict,wssdict, starttime, endtime,shipfclist)*100.0,1);

                var shipqtylist = new List<int>();
                var fclist = new List<int>();
                foreach (var sf in shipfclist)
                {
                    shipqtylist.Add(sf.ShipCount);
                    fclist.Add((int)sf.FCount);
                }

                if (plmdict.ContainsKey(ser.Series.ToUpper()) && (shipqtylist.Average() >= 1000 || fclist.Average() >= 1000))
                {
                    ser.PLM = plmdict[ser.Series.ToUpper()];
                }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                accuracylist = activeseries
            };
            return ret;
        }

        public JsonResult ForecastMergeData()
        {
            var starttime = "2018-11-01 00:00:00";
            var endtime = "2019-11-01 00:00:00";
            var startdate = Request.Form["startdate"];

            if (string.IsNullOrEmpty(startdate))
            {
                starttime = DateTime.Now.AddMonths(-12).ToString("yyyy-MM") + "-01 00:00:00";
                endtime = DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00";
            }
            else
            {
                var sdate = DateTime.Parse(Request.Form["startdate"]);
                var sdate1 = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                starttime = sdate1.ToString("yyyy-MM-dd HH:mm:ss");
                endtime = sdate1.AddMonths(12).ToString("yyyy-MM-dd HH:mm:ss");
            }

            var spandict = new Dictionary<string, int>();
            var spancheckdict = new Dictionary<string, bool>();

            var wssdict = PNBUMap.GetWSSSeries();
            var osaseriesdict = OSASeriesData.GetOSASeriesDict();
            var activeseries = PNBUMap.GetActiveSeries(starttime, endtime);
            foreach (var ser in activeseries)
            {
                var shiplist = new List<ShipForcastData>();
                ser.Accuracy = Math.Round(ShipForcastData.GetSeriesAccuracyVal(ser.Series, osaseriesdict, wssdict, starttime, endtime,shiplist) * 100.0, 1);

                var k = ser.BU.ToUpper();
                if (spandict.ContainsKey(k))
                { spandict[k] += 1; }
                else
                { spandict.Add(k, 1); }

                if (!spancheckdict.ContainsKey(k))
                { spancheckdict.Add(k,false); }

                k = (ser.BU+":"+ser.ProjectGroup).ToUpper();
                if (spandict.ContainsKey(k))
                { spandict[k] += 1; }
                else
                { spandict.Add(k, 1); }

                if (!spancheckdict.ContainsKey(k))
                { spancheckdict.Add(k, false); }
            }

            var table = new List<List<string>>();
            foreach (var ser in activeseries)
            {
                var templine = new List<string>();

                var k = ser.BU.ToUpper();
                if (spandict.ContainsKey(k) && !spancheckdict[k])
                {
                    templine.Add(ser.BU + ":rowspan=\"" + spandict[k] + "\"");
                    spancheckdict[k] = true;
                }
                else
                { templine.Add(ser.BU + ":hide"); }

                k = (ser.BU + ":" + ser.ProjectGroup).ToUpper();
                if (spandict.ContainsKey(k) && !spancheckdict[k])
                {
                    templine.Add(ser.ProjectGroup + ":rowspan=\"" + spandict[k] + "\"");
                    spancheckdict[k] = true;
                }
                else
                { templine.Add(ser.ProjectGroup + ":hide"); }

                templine.Add(ser.Series);
                templine.Add(ser.Accuracy.ToString()+"%");

                table.Add(templine);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                table = table
            };
            return ret;
        }

        public JsonResult SeriesAccuracyData()
        {
            var starttime = "2018-11-01 00:00:00";
            var endtime = "2019-11-01 00:00:00";
            var startdate = Request.Form["startdate"];

            if (string.IsNullOrEmpty(startdate))
            {
                starttime = DateTime.Now.AddMonths(-12).ToString("yyyy-MM") + "-01 00:00:00";
                endtime = DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00";
            }
            else
            {
                var sdate = DateTime.Parse(Request.Form["startdate"]);
                var sdate1 = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                starttime = sdate1.ToString("yyyy-MM-dd HH:mm:ss");
                endtime = sdate1.AddMonths(12).ToString("yyyy-MM-dd HH:mm:ss");
            }

            var wssdict = PNBUMap.GetWSSSeries();
            var osaseriesdict = OSASeriesData.GetOSASeriesDict();
            var series = Request.Form["series"];
            var flist = ShipForcastData.GetSeriesAccuracy(series,osaseriesdict,wssdict, starttime, endtime);

            var tabletitle = new List<object>();
            tabletitle.Add("S&OP II Forecast");
            var fcastlist = new List<object>();
            fcastlist.Add("Forecast");
            var delievelist = new List<object>();
            delievelist.Add("Actual Delivery");

            var avglist = new List<double>();
            var avgslist = new List<object>();
            avgslist.Add("Average Pecentage Error");

            var meanlist = new List<object>();
            meanlist.Add("Mean Absolute Pertage Error");
            var facurracylist = new List<object>();
            facurracylist.Add("Forecast Accuracy");
            foreach (var f in flist)
            {
                tabletitle.Add(f.DataTime);
                fcastlist.Add(f.FCountStr);
                delievelist.Add(f.ShipCount);
                avglist.Add(f.Accuracy);
                avgslist.Add(Math.Round(100.0 * f.Accuracy, 1).ToString() + "%");
                meanlist.Add("");
                facurracylist.Add("");
            }
            meanlist[meanlist.Count - 1] = Math.Round(100.0 * avglist.Average(), 1).ToString() + "%";
            facurracylist[facurracylist.Count -1] = Math.Round(100.0 * (1-avglist.Average()), 1).ToString() + "%";
            //facurracylist[facurracylist.Count - 1] = Math.Round(100.0 * (avglist.Average()), 1).ToString() + "%";
            var tablecontent = new List<object>();
            tablecontent.Add(fcastlist);
            tablecontent.Add(delievelist);
            tablecontent.Add(avgslist);
            tablecontent.Add(meanlist);
            tablecontent.Add(facurracylist);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                tabletitle = tabletitle,
                tablecontent = tablecontent
            };
            return ret;
        }

        public ActionResult ShipMargin()
        {
            return View();
        }

        public JsonResult ShipMarginData()
        {
            var starttime = "2019-07-01 00:00:00";
            var stdate = DateTime.Parse(starttime);

            var datatype = Request.Form["datatype"];
            var startdate = Request.Form["startdate"];
            if (!string.IsNullOrEmpty(startdate))
            {
                starttime = "2018-04-01 00:00:00";
                stdate = DateTime.Parse(starttime);

                var sdate = DateTime.Parse(Request.Form["startdate"]);
                var sdate1 = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                if (sdate1 < stdate)
                { starttime = "2018-04-01 00:00:00"; }
                else
                {
                    var q = QuarterCLA.RetrieveQuarterFromDate(sdate1);
                    var qd = QuarterCLA.RetrieveDateFromQuarter(q)[0];
                    starttime = qd.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            stdate = DateTime.Parse(starttime);
            var qlist = new List<string>();
            while (stdate < DateTime.Now)
            {
                qlist.Add(QuarterCLA.RetrieveQuarterFromDate(stdate));
                stdate = stdate.AddMonths(3);
            }

            var syscfg = CfgUtility.GetSysConfig(this);

            var activeseries = PNBUMap.GetModuleRevenueActiveSeries(starttime,syscfg["SHIPREVENUEBU"]);
            var monthlycostdict = ItemCostData.GetMonthlyCost(this);

            var producgroupdict = new Dictionary<string,List<ModuleRevenue>>();

            var content = new List<List<string>>();
            foreach (var ser in activeseries)
            {
                var lines = new List<string>();
                var revenlist = ModuleRevenue.GetRevenueList(starttime,ser.ProjectGroup, ser.Series, monthlycostdict, datatype,this);
                var qret = ModuleRevenue.ToQuartRevenue(revenlist);
                if (qret.Count > 0)
                {
                    var key = ser.BU.ToUpper().Trim() + ":::" + ser.ProjectGroup.ToUpper().Trim();
                    if (!producgroupdict.ContainsKey(key))
                    {
                        var templist = new List<ModuleRevenue>();
                        foreach (var q in qlist)
                        { templist.Add(new ModuleRevenue()); }
                        producgroupdict.Add(key, templist);
                    }

                    lines.Add(ser.BU);
                    lines.Add(ser.ProjectGroup);
                    lines.Add(ser.Series);
                    var idx = 0;
                    foreach (var q in qlist)
                    {
                        if (qret.ContainsKey(q))
                        {
                            lines.Add(UT.O2I(qret[q].ShipQty).ToString("N0"));
                            lines.Add("$" + UT.O2I(qret[q].Cost).ToString("N0"));
                            lines.Add("$" + UT.O2I(qret[q].SalePrice).ToString("N0"));
                            lines.Add(Math.Round(qret[q].Margin,2).ToString()+"%");

                            producgroupdict[key][idx].Cost += qret[q].Cost;
                            producgroupdict[key][idx].SalePrice += qret[q].SalePrice;
                            producgroupdict[key][idx].ShipQty += qret[q].ShipQty;
                        }
                        else
                        {
                            lines.Add(" ");
                            lines.Add(" ");
                            lines.Add(" ");
                            lines.Add(" ");
                        }
                        idx++;
                    }

                    content.Add(lines);
                }
            }//end foreach

            foreach (var kv in producgroupdict)
            {
                var BUPG = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);

                var line = new List<string>();
                line.Add("<strong>" + BUPG[0] + "</strong>");
                line.Add("<strong>" + BUPG[1] + "</strong>");
                line.Add(" ");

                foreach (var reven in kv.Value)
                {
                    if (reven.SalePrice != 0)
                    {
                        line.Add("<strong>" + UT.O2I(reven.ShipQty).ToString("N0") + "</strong>");
                        line.Add("<strong>" + "$" + UT.O2I(reven.Cost).ToString("N0") + "</strong>");
                        line.Add("<strong>" + "$" + UT.O2I(reven.SalePrice).ToString("N0") + "</strong>");
                        var margin = Math.Round((reven.SalePrice - reven.Cost) / reven.SalePrice * 100.0,2).ToString() + "%";
                        line.Add("<strong>" + margin + "</strong>");
                    }
                    else
                    {
                        line.Add(" ");
                        line.Add(" ");
                        line.Add(" ");
                        line.Add(" ");
                    }
                }

                var idx = 0;
                foreach (var li in content)
                {
                    if (string.Compare(li[1].Trim(), BUPG[1], true) == 0 && string.Compare(li[0].Trim(), BUPG[0], true) == 0)
                    {
                        content.Insert(idx, line);
                        break;
                    }
                    idx++;
                }
            }//end foreach

            var title = new List<string>();
            title.Add("BU");
            title.Add("Product Group");
            title.Add("Project");
            foreach (var q in qlist)
            {
                title.Add(q + " Volume");
                title.Add(q + " Cost");
                title.Add(q + " Revenue");
                title.Add(q + " Margin");
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                tabletitle = title,
                tablecontent = content
            };
            return ret;
        }

        public JsonResult ShipMarginDetail()
        {
            var datatype = Request.Form["datatype"];
            var sqstr = Request.Form["series"];
            var sq = sqstr.Split(new string[] { ":::" },StringSplitOptions.RemoveEmptyEntries);

            var prog = sq[0].Trim().ToUpper();
            var series = sq[1].Trim().ToUpper();
            var qts = sq[2].ToUpper().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var qt = qts[0] + " " + qts[1];

            var shipdetail = new List<ModuleRevenue>();
            if (qts[2].Trim().ToUpper().Contains("COST"))
            {
                var monthlycostdict = ItemCostData.GetMonthlyCost(this);
                shipdetail = ModuleRevenue.GetCostDetail(qt,prog, series, monthlycostdict, datatype,this);
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    fun = "COST",
                    shipdetail = shipdetail
                };
                return ret;
            }
            else
            {
                shipdetail = ModuleRevenue.GetPriceDetail(qt, series);
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    fun = "REVENUE",
                    shipdetail = shipdetail
                };
                return ret;
            }
        }



    }
}