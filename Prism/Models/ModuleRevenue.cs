using Prism.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class ModuleRevenue
    {
        public static void LoadData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var shipsrcfile = syscfg["MOUDULESHIPREVENUE"];
            var shipdesfile = ExternalDataCollector.DownloadShareFile(shipsrcfile, ctrl);
            if (!string.IsNullOrEmpty(shipdesfile))
            {
                var costdict = ItemCostData.RetrieveStandardCost();

                var lastday = ModuleRevenue.RetrieveLastDate();
                var now = DateTime.Now;

                var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, shipdesfile);

                var shipdatalist = new List<ModuleRevenue>();

                var soidx = 0; //So Number
                var lnidx = 16; //Line
                var cpoidx = 3; //Cust Po Number
                 var sqtidx = 31; //Shipped Qty
                var pnidx = 25; //Item
                var spdidx = 35; //Actual Ship Date
                var prcidx = 22; //Unit Selling Price Inv

                var idx = 0;
                foreach (var item in data[0])
                {
                    if (string.Compare(item, "So Number", true) == 0) { soidx = idx; }
                    else if (string.Compare(item, "Line", true) == 0) { lnidx = idx; }
                    else if (string.Compare(item, "Cust Po Number", true) == 0) { cpoidx = idx; }
                    else if (string.Compare(item, "Shipped Qty", true) == 0) { sqtidx = idx; }
                    else if (string.Compare(item, "Item", true) == 0) { pnidx = idx; }
                    else if (string.Compare(item, "Actual Ship Date", true) == 0) { spdidx = idx; }
                    else if (string.Compare(item, "Unit Selling Price Inv", true) == 0) { prcidx = idx; }
                    idx++;
                }//end foreach
                idx = 0;
                foreach (var line in data)
                {
                    if (idx == 0)
                    { idx++; continue; }

                    try
                    {
                        var shipid = UT.O2S(line[soidx]) + "-" + UT.O2S(line[lnidx]).Replace("0", "") + "-" + UT.O2S(line[sqtidx]);

                        var cpo = UT.O2S(line[cpoidx]).ToUpper();
                        var tempvm = new ModuleRevenue();
                        tempvm.ShipID = shipid;
                        tempvm.ShipQty = UT.O2I(line[sqtidx]);
                        tempvm.PN = UT.O2S(line[pnidx]);
                        tempvm.ShipDate = UT.T2S(line[spdidx]);
                        var shipdate = UT.O2T(line[spdidx]);

                            if (shipdate > lastday && shipdate < now &&
                            !cpo.Contains("RMA") && tempvm.ShipQty > 0 && !string.IsNullOrEmpty(tempvm.PN))
                            {
                                tempvm.SalePrice = UT.O2D(line[prcidx]);
                                if (costdict.ContainsKey(tempvm.PN))
                                { tempvm.Cost = costdict[tempvm.PN]; }
                                shipdatalist.Add(tempvm);
                            }
                    }
                    catch (Exception ex) { }
                }//end foreach

                foreach (var shipd in shipdatalist)
                { shipd.StoreData(); }

            }//end if
        }

        public static void LoadPostPrice(Controller ctrl)
        {
            var seriasdict = PNBUMap.GetSeriesPNMap();

            var shipdesfile = ExternalDataCollector.DownloadShareFile(@"\\wux-engsys01\PlanningForCast\SerialSalePrice.xlsx", ctrl);
            if (!string.IsNullOrEmpty(shipdesfile))
            {
                var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, shipdesfile);
                if (data.Count > 0)
                {
                    var qlist = new List<string>();
                    foreach (var item in data[0])
                    {
                        if (item.Contains(" Q"))
                        {
                            qlist.Add(item);
                        }
                    }

                    var idx = 0;
                    foreach (var line in data)
                    {
                        if (idx == 0)
                        { idx++; continue; }
                        if (seriasdict.ContainsKey(line[0].ToUpper().Trim()))
                        {
                            var pn = seriasdict[line[0].ToUpper().Trim()];
                            var jdx = 0;
                            foreach (var item in line)
                            {
                                if (jdx == 0)
                                { jdx++; continue; }

                                if (jdx > qlist.Count)
                                { break; }

                                if (!string.IsNullOrEmpty(item))
                                {
                                    var price = UT.O2D(item);
                                    if (price != 0)
                                    {
                                        var q = qlist[jdx - 1];
                                        var dt = QuarterCLA.RetrieveDateFromQuarter(q)[0].AddMonths(1);
                                        var shipid = pn + "_" + dt.ToString("yyyy-MM-dd") + "_100";

                                        var tempvm = new ModuleRevenue();
                                        tempvm.ShipID = shipid;
                                        tempvm.ShipQty = 100;
                                        tempvm.PN = pn;
                                        tempvm.SalePrice = price;
                                        tempvm.Cost = 0;
                                        tempvm.ShipDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                        tempvm.StoreData();
                                    }//end if
                                }//end if
                                jdx++;
                            }
                        }//end pn
                    }//end foreach
                }//end if
            }//end if
        }

        public static void LoadPostCost(Controller ctrl)
        {
            var seriasdict = PNBUMap.GetSeriesPNMap();

            var shipdesfile = ExternalDataCollector.DownloadShareFile(@"\\wux-engsys01\PlanningForCast\SerialCost.xlsx", ctrl);
            if (!string.IsNullOrEmpty(shipdesfile))
            {
                var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, shipdesfile,null,30);
                if (data.Count > 0)
                {
                    var qlist = new List<string>();
                    foreach (var item in data[0])
                    {
                        if (item.Contains(" Q"))
                        {
                            qlist.Add(item);
                        }
                    }

                    var sql = @"update ItemCostData set FrozenCost = @cost where pn in 
                                    (SELECT PN FROM [BSSupport].[dbo].[PNBUMap] where series = @series) and [Quarter] = @Quarter";

                    var idx = 0;
                    foreach (var line in data)
                    {
                        if (idx == 0)
                        { idx++; continue; }
                        if (seriasdict.ContainsKey(line[0].ToUpper().Trim()))
                        {
                            var sr = line[0].ToUpper().Trim();
                            var jdx = 0;
                            foreach (var item in line)
                            {
                                if (jdx == 0)
                                { jdx++; continue; }

                                if (jdx > qlist.Count)
                                { break; }

                                if (!string.IsNullOrEmpty(item))
                                {
                                    var cost = UT.O2D(item);
                                    if (cost != 0)
                                    {
                                        var q = qlist[jdx - 1];

                                        var dict = new Dictionary<string, string>();
                                        dict.Add("@cost", cost.ToString());
                                        dict.Add("@series", sr);
                                        dict.Add("@Quarter", q);
                                        DBUtility.ExeLocalSqlNoRes(sql, dict);
                                    }//end if
                                }//end if
                                jdx++;
                            }
                        }//end pn
                    }//end foreach
                }//end if
            }//end if
        }

        public void StoreData()
        {
            var sql = "insert into ModuleRevenue(ShipID,ShipQty,PN,SalePrice,Cost,ShipDate) values(@ShipID,@ShipQty,@PN,@SalePrice,@Cost,@ShipDate)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ShipID", ShipID);
            dict.Add("@ShipQty", ShipQty.ToString());
            dict.Add("@PN",PN);
            dict.Add("@SalePrice", SalePrice.ToString());
            dict.Add("@Cost", Cost.ToString());
            dict.Add("@ShipDate",ShipDate);
            DBUtility.ExeLocalSqlWithRes(sql, null, dict);
        }

        public static Dictionary<string, double> GetSeriasPrice(string series)
        {
           var sql = @"select ShipDate,SalePrice,ShipQty,pn from [BSSupport].[dbo].[ModuleRevenue] where pn in
                         (SELECT PN FROM [BSSupport].[dbo].[PNBUMap] where series = @series) 
                           and SalePrice > 0 order by ShipDate";

            var dict = new Dictionary<string, string>();
            dict.Add("@series", series);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);

            if (dbret.Count == 0)
            { return new Dictionary<string, double>(); }

            var qpricesum = new Dictionary<string, double>();
            var qqtysum = new Dictionary<string, double>();
            var tp = 0.0;
            var tq = 0.0;

            var pnpricesum = new Dictionary<string, double>();
            var pnqtysum = new Dictionary<string, double>();

            foreach (var line in dbret)
            {
                var dt = UT.O2T(line[0]);
                var q = QuarterCLA.RetrieveQuarterFromDate(dt);
                var price = UT.O2D(line[1]);
                var qty = UT.O2D(line[2]);
                var pn = UT.O2S(line[3]);

                var pk = pn + "_" + q;
                if (pnpricesum.ContainsKey(pk))
                {
                    pnpricesum[pk] += price * qty;
                    pnqtysum[pk] += qty;
                }
                else
                {
                    pnpricesum.Add(pk, price * qty);
                    pnqtysum.Add(pk, qty);
                }

                if (qpricesum.ContainsKey(q))
                {
                    qpricesum[q] += price * qty;
                    qqtysum[q] += qty;
                    tp += price * qty;
                    tq += qty;
                }
                else
                {
                    qpricesum.Add(q, price * qty);
                    qqtysum.Add(q, qty);
                    tp += price * qty;
                    tq += qty;
                }
            }

            var genprice = tp / tq;
            var qpricedict = new Dictionary<string, double>();
            foreach (var kv in qpricesum)
            {
                qpricedict.Add(kv.Key, kv.Value / qqtysum[kv.Key]);
            }

            foreach (var kv in pnpricesum)
            {
                qpricedict.Add(kv.Key, kv.Value / pnqtysum[kv.Key]);
            }

            qpricedict.Add("GENERAL", genprice);


            return qpricedict;
        }

        public static List<ModuleRevenue> GetRevenueList(string startdate, string series,Dictionary<string,double> costdict
            ,Dictionary<string,double> monthlycostdict, double USDRate)
        {
            var qpricedict = GetSeriasPrice(series);
            if (qpricedict.Count == 0)
            { return new List<ModuleRevenue>(); }

            var qcostdict = ItemCostData.RetrieveQuartCost(series);


            var sql = @"select ShipQty,ShipDate,PN from [BSSupport].[dbo].[FsrShipData] where pn in
                        (SELECT PN FROM [BSSupport].[dbo].[PNBUMap] where series = @series) and ShipDate >= @startdate";
            var dict = new Dictionary<string, string>();
            dict.Add("@series", series);
            dict.Add("@startdate", startdate);

            var shipdata = new List<ModuleRevenue>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new ModuleRevenue();
                tempvm.ShipQty = UT.O2D(line[0]);
                tempvm.ShipDate = UT.T2S(line[1]);
                tempvm.PN = UT.O2S(line[2]);
                shipdata.Add(tempvm);
            }

            var ret = new List<ModuleRevenue>();

            var dt_201903 = DateTime.Parse("2019-03-01 00:00:00").AddSeconds(-1);

            foreach (var item in shipdata)
            {
                var shipd = UT.O2T(item.ShipDate);
                var pmk = item.PN + "_" + shipd.ToString("yyyy-MM");

                var price = qpricedict["GENERAL"];
                var q = QuarterCLA.RetrieveQuarterFromDate(shipd);

                var pqk = item.PN + "_" + q;
                if (qpricedict.ContainsKey(pqk))
                {
                    price = qpricedict[pqk];
                }
                else if (qpricedict.ContainsKey(q))
                { price = qpricedict[q]; }

                var cost = 0.0;
                if (shipd > dt_201903 && monthlycostdict.ContainsKey(pmk))
                {
                    cost = monthlycostdict[pmk];
                }
                else if (qcostdict.ContainsKey(item.PN + "_" + q))
                {
                    cost = qcostdict[item.PN + "_" + q];
                }
                else if (shipd <= dt_201903 && qcostdict.ContainsKey(q))
                {
                    cost = qcostdict[q];
                }
                //else if (costdict.ContainsKey(item.PN))
                //{
                //    cost = costdict[item.PN];
                //}
                else
                { continue; }

                var ucs = cost / USDRate;
                item.SalePrice = price;
                item.Cost = ucs;
                ret.Add(item);

            }

            return ret;
        }

        public static Dictionary<string,ModuleRevenue> ToQuartRevenue(List<ModuleRevenue> revenuedatas)
        {
            var dict = new Dictionary<string, ModuleRevenue>();
            foreach (var data in revenuedatas)
            {
                var q = QuarterCLA.RetrieveQuarterFromDate(UT.O2T(data.ShipDate));
                if (dict.ContainsKey(q))
                {
                    dict[q].SalePrice += data.ShipQty * data.SalePrice;
                    dict[q].Cost += data.ShipQty * data.Cost;
                    dict[q].QT = q;
                    dict[q].ShipQty += data.ShipQty;
                }
                else
                {
                    var tempvm = new ModuleRevenue();
                    tempvm.SalePrice = data.ShipQty * data.SalePrice;
                    tempvm.Cost = data.ShipQty * data.Cost;
                    tempvm.QT = q;
                    tempvm.ShipQty = data.ShipQty;
                    dict.Add(q, tempvm);
                }
            }

            foreach (var kv in dict)
            {
                var q = kv.Value;

                if (q.SalePrice == 0)
                { q.SalePrice = 1; }

                q.Margin = (q.SalePrice - q.Cost) / q.SalePrice * 100.0;
            }

            return dict;
        }

        private static DateTime RetrieveLastDate()
        {
            var sql = "select max(ShipDate) from ModuleRevenue";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (DBNull.Value == dbret[0][0])
            { return DateTime.Parse("2019-07-01 00:00:00"); }
            else
            { return UT.O2T(dbret[0][0]); }
        }

        public ModuleRevenue()
        {
            ShipID = "";
            ShipQty = 0;
            PN = "";
            SalePrice = 0;
            Cost = 0;
            ShipDate = "1982-05-06 10:00:00";
            Margin = 0;
            QT = "";
        }

        public string ShipID { set; get; }
        public double ShipQty { set; get; }
        public string PN { set; get; }
        public double SalePrice { set; get; }
        public double Cost { set; get; }
        public string ShipDate { set; get; }
        public double Margin { set; get; }
        public string QT { set; get; }
    }

}