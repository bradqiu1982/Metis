using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;

namespace Prism.Controllers
{
    public class VCSELController : Controller
    {
        public ActionResult VCSELShipmentDppm()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ShipmentData"))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        private object GetShipmentChartData(Dictionary<string, Dictionary<string, double>> shipdata, Dictionary<string, int> vcselrmacntdict
            , Dictionary<string, int> allrmacntdict, string rate, string producttype)
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
            var title = shipdatelist[0] + " ~ " + shipdatelist[lastdidx] + " " + producttype + " Shipment vs VCSEL DPPM (" + rate + ")";


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
                    yAxis = 1,
                    color = "#f7a35c",
                    lineWidth = 4
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

        public JsonResult VCSELDppmDistribution()
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

                if (startdate < DateTime.Parse("2016-01-01 00:00:00"))
                { startdate = DateTime.Parse("2016-01-01 00:00:00"); }

                if (enddate < DateTime.Parse("2016-01-01 00:00:00"))
                { enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1); }
            }
            else
            {
                startdate = DateTime.Parse("2016-01-01 00:00:00");
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            //<date,<customer,int>>
            var shipdata25g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var shipdata14g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var shipdataarray = new List<object>();
            if (shipdata14g.Count > 0)
            {
                var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentChartData(shipdata14g, vcselrmacntdict, allrmacntdict, "10G_14G", SHIPPRODTYPE.PARALLEL));
            }
            if (shipdata25g.Count > 0)
            {
                var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentChartData(shipdata25g, vcselrmacntdict, allrmacntdict, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
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

        public ActionResult VCSELWaferDppm()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ShipmentData"))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        public JsonResult RetrieveVcselRMARawData()
        {
            var wafer = Request.Form["wafer"];
            var waferdatalist = VcselRMAData.RetrieveWaferRawData(wafer);
            var ret = new JsonResult();
            ret.Data = new { waferdatalist = waferdatalist };
            return ret;
        }

        public JsonResult VCSELWaferDppmData()
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

                if (startdate < DateTime.Parse("2016-01-01 00:00:00"))
                { startdate = DateTime.Parse("2016-01-01 00:00:00"); }

                if (enddate < DateTime.Parse("2016-01-01 00:00:00"))
                { enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1); }
            }
            else
            {
                startdate = DateTime.Parse("2016-01-01 00:00:00");
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var ratechartlist = new List<object>();

            var ratelist = VcselRMAData.RetrieveRateList(startdate.ToString("yyyy-MM-dd HH:mm:ss"),enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            foreach (var rate in ratelist)
            {
                var dppmlist = VcselRMASum.RetrieveVcselDPPM(rate, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));

                var xdatalist = new List<string>();
                var dppmdatalist = new List<double>();
                var shippeddatalist = new List<double>();
                foreach (var dppm in dppmlist)
                {
                    xdatalist.Add(dppm.Wafer);
                    dppmdatalist.Add(dppm.DPPM);
                    shippeddatalist.Add(dppm.ShippedQty);
                }

                var plotbands = new List<object>();

                var latestwaferlist = VcselRMAData.RetrieveLatestWafer(rate);
                if (latestwaferlist.Count > 0)
                {
                    var latestwafer = latestwaferlist[0].Wafer;
                    var from = 0.0;
                    var to = 0.0;
                    var idx = 0.0;
                    foreach (var item in xdatalist)
                    {
                        if (string.Compare(item, latestwafer, true) == 0)
                        {
                            from = idx - 0.5;
                            to = idx + 0.5;
                            break;
                        }
                        idx = idx + 1.0;
                    }
                    if (to != 0.0)
                    {
                        plotbands.Add(
                            new
                            {
                                color = "#00b050",
                                from = from,
                                to = to,
                            }
                            );
                    }
                }

                var xAxis = new
                {
                    data = xdatalist
                };
                var yAxis = new
                {
                    title = "Dppm"
                };
                var dppmdata = new
                {
                    name = "Dppm",
                    color = "#5CB85C",
                    data = dppmdatalist
                };
                var shipdata = new
                {
                    name = "Shipped",
                    color = "#12CC92",
                    data = shippeddatalist
                };
                var alldata = new
                {
                    data = dppmdata,
                    cdata = shipdata
                };
                var dppmline = new
                {
                    id = "vcsel_dppm"+"_"+rate,
                    title = rate+ " Vcsel RMA Wafer Dppm",
                    xAxis = xAxis,
                    yAxis = yAxis,
                    data = alldata,
                    plotbands = plotbands
                };

                ratechartlist.Add(dppmline);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                ratedataarray = ratechartlist
            };
            return ret;

        }


        public ActionResult VCSELStatistic()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 9, "ShipmentData"))
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        public JsonResult VCSELStatisticData()
        {
            var vcseltech = VcselTechDppm();
            var vcselarray = VcselArrayDppm();
            var vcselvstime = VcselVSTimeData();
            var milestone = VcselRMAMileStoneData();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true,
                vcseltechdata = vcseltech,
                vcselarraydata = vcselarray,
                shipdatedata = vcselvstime[0],
                accumulatedata =vcselvstime[1],
                vcsel_milestone = milestone
            };
            return ret;
        }

        public List<object> VcselVSTimeData()
        {
            //<month,<type,count>>
            var rmavsshipdate = new Dictionary<string, Dictionary<string, int>>();
            //<type,sumlist>
            var typesumlist = new Dictionary<string, List<double>>();
            //<type,countlist>
            var typecntlist = new Dictionary<string, List<int>>();

            var alldata = VcselRMAData.RetrievAllDataASC();
            var typedict = new Dictionary<string, bool>();
            var monthxlist = (new string[] { "3", "6", "12", "18", "24", "30", "36", "48" }).ToList();

            foreach (var item in alldata)
            {
                if (!typedict.ContainsKey(item.VcselType))
                {
                    typedict.Add(item.VcselType, true);
                }

                try
                {
                    var rdate = DateTime.Parse(item.RMAOpenDate);
                    var sdate = DateTime.Parse(item.ShipDate);
                    var vsmonth = DaysToMonth((rdate - sdate).TotalDays).ToString();
                    if (rmavsshipdate.ContainsKey(vsmonth))
                    {
                        var temptypedict = rmavsshipdate[vsmonth];
                        temptypedict["ALL_TYPE"] = temptypedict["ALL_TYPE"] + 1;
                        if (temptypedict.ContainsKey(item.VcselType))
                        {
                            temptypedict[item.VcselType] = temptypedict[item.VcselType] + 1;
                        }
                        else
                        {
                            temptypedict.Add(item.VcselType, 1);
                        }
                    }
                    else
                    {
                        var temptypedict = new Dictionary<string, int>();
                        temptypedict.Add("ALL_TYPE", 1);
                        temptypedict.Add(item.VcselType, 1);
                        rmavsshipdate.Add(vsmonth, temptypedict);
                    }
                }
                catch (Exception ex) { }
            }//end foreach

            typedict.Add("ALL_TYPE", true);

            //fill the whole struct
            foreach (var m in monthxlist)
            {
                if (rmavsshipdate.ContainsKey(m))
                {
                    var rmaship = rmavsshipdate[m];
                    foreach (var t in typedict)
                    {
                        if (!rmaship.ContainsKey(t.Key))
                        {
                            rmaship.Add(t.Key, 0);
                        }
                    }
                }
                else
                {
                    var temptypedict = new Dictionary<string, int>();
                    foreach (var t in typedict)
                    {
                        temptypedict.Add(t.Key, 0);
                    }
                    rmavsshipdate.Add(m, temptypedict);
                }
            }


            foreach (var m in monthxlist)
            {
                //has data of this month
                var temptypedict = rmavsshipdate[m];
                foreach (var t in typedict)
                {
                    if (typesumlist.ContainsKey(t.Key))
                    {
                        var tempsumlist = typesumlist[t.Key];
                        if (tempsumlist.Count == 0)
                        {
                            tempsumlist.Add(temptypedict[t.Key]);
                        }
                        else
                        {
                            var lastval = tempsumlist[tempsumlist.Count - 1] + temptypedict[t.Key];
                            tempsumlist.Add(lastval);
                        }
                    }
                    else
                    {
                        var tempsumlist = new List<double>();
                        tempsumlist.Add(temptypedict[t.Key]);
                        typesumlist.Add(t.Key, tempsumlist);
                    }

                    if (!typecntlist.ContainsKey(t.Key))
                    {
                        var tempcntlist = new List<int>();
                        tempcntlist.Add(temptypedict[t.Key]);
                        typecntlist.Add(t.Key, tempcntlist);
                    }
                    else
                    {
                        typecntlist[t.Key].Add(temptypedict[t.Key]);
                    }
                }//end foreach
            }//end foreach

            var rmashipchartdata = new List<object>();
            foreach (var kv in typecntlist)
            {
                rmashipchartdata.Add(new
                {
                    name = kv.Key + " Failure",
                    data = kv.Value,
                    lineWidth = 3
                });
            }

            var rmaaccudata = new List<object>();
            foreach (var kv in typesumlist)
            {
                var last = kv.Value.Last();
                if (last != 0.0)
                {
                    var templist = new List<double>();
                    foreach (var c in kv.Value)
                    {
                        templist.Add(Math.Round(c / last * 100.0, 2));
                    }
                    rmaaccudata.Add(new
                    {
                        name = kv.Key + " accu",
                        data = templist,
                        lineWidth = 3
                    });
                }
            }

            var sxaxisdata = new List<string>();
            for (var midx = 0; midx < monthxlist.Count; midx++)
            {
                if (midx == 0)
                    sxaxisdata.Add("in " + monthxlist[midx] + " M");
                else
                    sxaxisdata.Add("From " + monthxlist[midx - 1] + " To " + monthxlist[midx] + " M");
            }

            var cxaxisdata = new List<string>();
            for (var midx = 0; midx < monthxlist.Count; midx++)
            {
                cxaxisdata.Add("in " + monthxlist[midx] + " M");
            }

            var ret = new List<object>();

            var shipdatedata = new
            {
                id = "rmavshipdate",
                title = "VCSEL RMA Failure vs Ship Date",
                xaxis = sxaxisdata,
                data = rmashipchartdata
            };
            var accumulatedata = new
            {
                id = "rmaaccumulatedata",
                title = "VCSEL RMA Accumulate Failure Rate vs Ship Date",
                xaxis = cxaxisdata,
                data = rmaaccudata
            };
            ret.Add(shipdatedata);
            ret.Add(accumulatedata);
            return ret;
        }

        public static int DaysToMonth(double days)
        {
            var ret = 3;
            if (days <= 90)
                ret = 3;
            else if (days > 90 && days <= 180)
                ret = 6;
            else if (days > 180 && days <= 360)
                ret = 12;
            else if (days > 360 && days <= 540)
                ret = 18;
            else if (days > 540 && days <= 720)
                ret = 24;
            else if (days > 720 && days <= 900)
                ret = 30;
            else if (days > 900 && days <= 1080)
                ret = 36;
            else if (days > 1080)
                ret = 48;

            return ret;
        }

        public object VcselRMAMileStoneData()
        {
            var combinexdict = new Dictionary<string, bool>();

            var id = "vcsel_milestone";
            var title = "Vcsel RMA Statistic vs Action Milestone";

            var retdata = VcselRMASum.VcselRMAMileStoneDataByBuildDate();

            //dict<month,dict<rate,count>>
            var monthlyrma = (Dictionary<string, Dictionary<string, int>>)retdata[0];
            var milestonedata = (List<EngineeringMileStone>)retdata[1];
            var vratecolordict = (Dictionary<string, string>)retdata[2];


            var vratelist = vratecolordict.Keys.ToList();
            vratelist.Sort();

            foreach (var item in monthlyrma)
            {
                if (!combinexdict.ContainsKey(item.Key))
                { combinexdict.Add(item.Key, true); }
            }

            var milestonedict = new Dictionary<string, string>();
            foreach (var item in milestonedata)
            {
                var month = item.ActionDate.ToString("yyyy-MM");
                if (!combinexdict.ContainsKey(month))
                { combinexdict.Add(month, true); }

                if (milestonedict.ContainsKey(month))
                {
                    milestonedict[month] = milestonedict[month] + "<br/>" + item.Location + "_" + item.ActionDetail;
                }
                else
                {
                    milestonedict.Add(month, item.Location + "_" + item.ActionDetail);
                }
            }


            var combinxlist = combinexdict.Keys.ToList();
            combinxlist.Sort();

            //dict<rate,countlist>
            var ratecountbydate = new Dictionary<string, List<int>>();
            var maxrmacount = 0;
            //get rate count list of each date
            foreach (var date in combinxlist)
            {
                if (monthlyrma.ContainsKey(date))
                {
                    var ratedict = monthlyrma[date];
                    var datecount = 0;
                    foreach (var ratekv in ratedict)
                    {
                        if (ratecountbydate.ContainsKey(ratekv.Key))
                        {
                            ratecountbydate[ratekv.Key].Add(ratekv.Value);
                            datecount += ratekv.Value;
                        }
                        else
                        {
                            var templist = new List<int>();
                            templist.Add(ratekv.Value);
                            ratecountbydate.Add(ratekv.Key, templist);
                            datecount += ratekv.Value;
                        }
                    }

                    if (datecount > maxrmacount) { maxrmacount = datecount; }
                }
                else
                {
                    foreach (var rate in vratelist)
                    {
                        if (ratecountbydate.ContainsKey(rate))
                        {
                            ratecountbydate[rate].Add(0);
                        }
                        else
                        {
                            var templist = new List<int>();
                            templist.Add(0);
                            ratecountbydate.Add(rate, templist);
                        }
                    }
                }
            }//end foreach

            maxrmacount = maxrmacount + 1;


            //construct milestone data array
            var idx = 0;
            var milestoneinfoarray = new List<object>();
            foreach (var date in combinxlist)
            {
                if (milestonedict.ContainsKey(date))
                {
                    milestoneinfoarray.Add(new
                    {
                        x = idx,
                        y = maxrmacount,
                        name = milestonedict[date]
                    });
                }
                idx = idx + 1;
            }
            //construct milestone struct
            var mstmarker = new
            {
                radius = 6,
                symbol = "circle",
                fillColor = "#fff",
                lineColor = "#ffc000",
                lineWidth = 2
            };
            var msttooltip = new
            {
                useHTML = true,
                headerFormat = "<span style='font-size: 10px'>{point.x}</span><br/>",
                pointFormat = "{point.name}"
            };

            var mstseria = new
            {
                type = "scatter",
                name = "Milestone",
                marker = mstmarker,
                tooltip = msttooltip,
                data = milestoneinfoarray
            };


            //construct vcsel rate column data
            var serialarray = new List<object>();
            foreach (var rate in vratelist)
            {
                serialarray.Add(new
                {
                    name = rate,
                    color = vratecolordict[rate],
                    data = ratecountbydate[rate]
                });
            }
            serialarray.Add(mstseria);

            var allx = new { data = combinxlist };
            var ally = new { title = "Amount" };
            var time = new
            {
                name = "Milestone",
                color = "#F0AD4E",
                data = maxrmacount
            };

            var vcsel_milestone = new
            {
                id = id,
                title = title,
                coltype = "normal",
                xAxis = allx,
                yAxis = ally,
                time = time,
                data = serialarray
            };

            //var ret = new JsonResult();
            //ret.Data = new
            //{
            //    success = true,
            //    vcsel_milestone = vcsel_milestone
            //};
            //return ret;

            return vcsel_milestone;
        }


        public object VcselTechDppm()
        {
            var techshipdata = WaferData.RetriveWaferTechCountDict();
            var techrmadata = VcselRMAData.RetrieveVcselTechDict();
            var techlist = techshipdata.Keys.ToList();
            techlist.Sort();
            if (techlist.Contains("PLANAR"))
            {
                techlist.Remove("PLANAR");
                techlist.Insert(0, "PLANAR");
            }

            var tempxlist = new List<string>();
            foreach (var x in techlist)
            {
                if (x.Contains("PLANAR"))
                {
                    tempxlist.Add("10G/14G PLANAR");
                }
                else
                {
                    tempxlist.Add("25G " + x);
                }
            }

            var shiplist = new List<double>();
            var dppmlist = new List<double>();

            foreach (var tech in techlist)
            {
                shiplist.Add(techshipdata[tech]);
                if (techrmadata.ContainsKey(tech))
                {
                    dppmlist.Add(Math.Round((double)techrmadata[tech] / (double)techshipdata[tech] * 1000000, 0));
                }
                else
                {
                    dppmlist.Add(0);
                }
            }

            var serial = new List<object>();
            serial.Add(new { name="ship count",data=shiplist,type="column", yAxis=1 });
            serial.Add(new { name = "vcsel dppm", data = dppmlist,type="line" });
            return new
            {
                id = "vcsel_tech_dppm",
                title = "VCSEL TECH DPPM FROM 2016",
                xlist = tempxlist,
                series = serial
            };
        }


        public object VcselArrayDppm()
        {
            var arrayshipdata = WaferData.RetriveWaferArrayCountDict();
            var arrayrmadata = VcselRMAData.RetrieveVcselArrayDict();
            var arraylist = arrayshipdata.Keys.ToList();
            arraylist.Sort(delegate(string obj1,string obj2) {
                var v1 = Convert.ToInt32(obj1.Split(new string[] { "X", "x" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                var v2 = Convert.ToInt32(obj2.Split(new string[] { "X", "x" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                return v1.CompareTo(v2);
            });

            var shiplist = new List<double>();
            var dppmlist = new List<double>();

            foreach (var ary in arraylist)
            {
                shiplist.Add(arrayshipdata[ary]);
                if (arrayrmadata.ContainsKey(ary))
                {
                    dppmlist.Add(Math.Round((double)arrayrmadata[ary] / (double)arrayshipdata[ary] * 1000000, 0));
                }
                else
                {
                    dppmlist.Add(0);
                }
            }

            var serial = new List<object>();
            serial.Add(new { name = "ship count", data = shiplist, type = "column", yAxis = 1 });
            serial.Add(new { name = "vcsel dppm", data = dppmlist, type = "line" });
            return new
            {
                id = "vcsel_array_dppm",
                title = "VCSEL 25G ARRAY DPPM FROM 2016",
                xlist = arraylist,
                series = serial
            };
        }

    }
}