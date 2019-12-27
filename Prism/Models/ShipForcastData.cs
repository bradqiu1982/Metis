using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class ShipForcastData
    {
        public static void LoadData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var forcastfolder = syscfg["FORCASTDATA"];
            var allfiles = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, forcastfolder);
            var flist = new List<ShipForcastData>();
            foreach (var f in allfiles)
            {
                var p = System.IO.Path.GetFileNameWithoutExtension(f).ToUpper();
                if (p.Contains("MANUFACTURING CAPACITY") && p.Contains("GLOBAL"))
                {
                    var tfp = new ShipForcastData();
                    tfp.DataPath = f;
                    var y = p.Replace(".","").Split(new string [] { "GLOBAL" }, StringSplitOptions.RemoveEmptyEntries);
                    var dstr = y[1].Trim().Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    tfp.DataUpdateStamp = "20" + dstr.Substring(dstr.Length - 2, 2)+"-"+dstr.Substring(0,2)+"-01 10:00:00";
                    flist.Add(tfp);
                }
            }

            foreach (var f in flist)
            {
                if (DataExist(f.DataUpdateStamp))
                {
                    var ago3m = DateTime.Now.AddMonths(-3);
                    if (UT.O2T(f.DataUpdateStamp) > ago3m)
                    {
                        CleanData(f.DataUpdateStamp);
                        LoadData_(f, ctrl);
                    }
                }
                else
                {
                    LoadData_(f, ctrl);
                }
            }

        }

        private static void LoadData_(ShipForcastData f,Controller ctrl)
        {
            var desfile = ExternalDataCollector.DownloadShareFile(f.DataPath, ctrl);
            if (!string.IsNullOrEmpty(desfile))
            {
                var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, desfile, "Raw data");

                var pnidx = 1;
                var srcorgidx = 17;

                var idx = 0;
                foreach (var cname in data[0])
                {
                    if (cname.ToUpper().Contains("ITEM"))
                    { pnidx = idx; break; }
                    idx++;
                }

                idx = 0;
                foreach (var cname in data[0])
                {
                    if (string.Compare(cname.ToUpper(),"SOURCE ORG") == 0)
                    { srcorgidx = idx; break; }
                    idx++;
                }

                var colidxdict = new Dictionary<int, string>();
                var mdict = new Dictionary<string, string>();
                mdict.Add("JAN", "01");
                mdict.Add("FEB", "02");
                mdict.Add("MAR", "03");
                mdict.Add("APR", "04");
                mdict.Add("MAY", "05");
                mdict.Add("JUN", "06");
                mdict.Add("JUL", "07");
                mdict.Add("AUG", "08");
                mdict.Add("SEP", "09");
                mdict.Add("OCT", "10");
                mdict.Add("NOV", "11");
                mdict.Add("DEC", "12");

                for (idx = 18; idx < 36; idx++)
                {
                    foreach (var mkv in mdict)
                    {
                        if (data[0][idx].ToUpper().Contains(mkv.Key))
                        {
                            colidxdict.Add(idx, "20" + (UT.O2I(data[0][idx].ToUpper().Replace(mkv.Key, "")) - 1) + "-" + mkv.Value + "-01 10:00:00");
                            break;
                        }
                    }
                }//end for

                var forcastlist = new List<ShipForcastData>();
                idx = 0;
                foreach (var line in data)
                {
                    if (idx == 0)
                    { idx++;continue; }

                    var pn = line[pnidx];
                    var loc = line[srcorgidx];

                    if (loc.ToUpper().Contains("WXI"))
                    {
                        foreach (var ckv in colidxdict)
                        {
                            var tempvm = new ShipForcastData();
                            tempvm.PN = pn;
                            tempvm.DataUpdateStamp = f.DataUpdateStamp;
                            tempvm.DataTime = ckv.Value;
                            if (string.IsNullOrEmpty(line[ckv.Key]))
                            { tempvm.FCount = 0; }
                            else
                            { tempvm.FCount = UT.O2I(line[ckv.Key]); }
                            forcastlist.Add(tempvm);
                        }
                    }
                }

                var pndatadict = new Dictionary<string, Dictionary<string, ShipForcastData>>();
                foreach (var d in forcastlist)
                {
                    if (pndatadict.ContainsKey(d.PN))
                    {
                        var tmpdict = pndatadict[d.PN];
                        if (tmpdict.ContainsKey(d.DataTime))
                        {
                            tmpdict[d.DataTime].FCount += d.FCount; 
                        }
                        else
                        { tmpdict.Add(d.DataTime, d); }
                    }
                    else
                    {
                        var tmpdict = new Dictionary<string, ShipForcastData>();
                        tmpdict.Add(d.DataTime, d);
                        pndatadict.Add(d.PN, tmpdict);
                    }
                }

                foreach (var pkv in pndatadict)
                {
                    foreach (var dkv in pkv.Value)
                    {
                        dkv.Value.StoreData();
                    }
                }
            }//end if
        }

        private static void CleanData(string DataUpdateStamp)
        {
            var sql = "delete from ShipForcastData where DataUpdateStamp = @DataUpdateStamp";
            var dict = new Dictionary<string, string>();
            dict.Add("@DataUpdateStamp", DataUpdateStamp);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }


        private static bool DataExist(string DataUpdateStamp)
        {
            var sql = "select top 1 * from ShipForcastData where DataUpdateStamp = @DataUpdateStamp";
            var dict = new Dictionary<string, string>();
            dict.Add("@DataUpdateStamp", DataUpdateStamp);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null, dict);
            if (dbret.Count > 0)
            { return true; }
            return false; 
        }

        public void StoreData()
        {
            var sql = "insert into ShipForcastData(PN,DataTime,FCount,DataUpdateStamp) values(@PN,@DataTime,@FCount,@DataUpdateStamp)";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            dict.Add("@DataTime", DataTime);
            dict.Add("@FCount", FCount.ToString());
            dict.Add("@DataUpdateStamp", DataUpdateStamp);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, double> GetShipData(string series,string startdate,string enddate)
        {
            var ret = new Dictionary<string, double>();
            var sql = @"select ShipDate,ShipQty from FsrShipData where pn in
                        (SELECT PN FROM [BSSupport].[dbo].[PNBUMap] where series = @series) 
                    and ShipDate >= @startdate and ShipDate < @enddate ";
            var dict = new Dictionary<string, string>();
            dict.Add("@series", series);
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var m = UT.O2T(line[0]).ToString("yyyy-MM");
                var qty = UT.O2D(line[1]);
                if (ret.ContainsKey(m))
                { ret[m] += qty; }
                else
                { ret.Add(m, qty); }
            }
            return ret;
        }

        public static Dictionary<string, double> GetForecastData(string series, string startdate, string enddate)
        {
            var flist = new List<ShipForcastData>();
            var kdict = new Dictionary<string, bool>();
            var ret = new Dictionary<string, double>();

            var sqllist = new List<string>();
            sqllist.Add(@"select PN,DataTime,FCount from ShipForcastData where PN in 
                    (SELECT PN FROM [BSSupport].[dbo].[PNBUMap] where series = @series) 
                    and DataTime >=  @startdate and DataTime < @enddate and DataTime = dataupdatestamp order by PN,dataupdatestamp desc");
            sqllist.Add(@"select PN,DataTime,FCount from ShipForcastData where PN in 
                    (SELECT PN FROM [BSSupport].[dbo].[PNBUMap] where series = @series) 
                    and DataTime >=  @startdate and DataTime < @enddate order by PN,dataupdatestamp desc");

            var dict = new Dictionary<string, string>();
            dict.Add("@series", series);
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);

            foreach (var sql in sqllist)
            {
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
                foreach (var line in dbret)
                {
                    var pn = UT.O2S(line[0]);
                    var datatime = UT.O2T(line[1]).ToString("yyyy-MM");
                    var key = pn + datatime;
                    if (!kdict.ContainsKey(key))
                    {
                        kdict.Add(key, true);
                        var tempvm = new ShipForcastData();
                        tempvm.PN = pn;
                        tempvm.DataTime = datatime;
                        tempvm.FCount = UT.O2I(line[2]);
                        flist.Add(tempvm);
                    }
                }
            }

            foreach (var f in flist)
            {
                if (ret.ContainsKey(f.DataTime))
                {
                    ret[f.DataTime] += UT.O2D(f.FCount);
                }
                else
                {
                    ret.Add(f.DataTime, UT.O2D(f.FCount));
                }
            }

            return ret;
        }

        public static void GetSeriesAccuracy()
        {
            var shipdata = GetShipData("Tunable XFP Gen1", "2018-11-01 00:00:00", "2019-11-01 00:00:00");
            var forecastdata = GetForecastData("Tunable XFP Gen1", "2018-11-01 00:00:00", "2019-11-01 00:00:00");

            var flist = new List<ShipForcastData>();
            foreach (var kv in forecastdata)
            {
                var shipqty = 1.0;
                if (shipdata.ContainsKey(kv.Key))
                { shipqty = shipdata[kv.Key]; }
                var forecast = kv.Value;
                var acc = Math.Abs((shipqty - forecast) / shipqty);
                if (acc > 1.0)
                { acc = 1.0; }

                var tempvm = new ShipForcastData();
                tempvm.Accuracy = acc;
                tempvm.DataTime = kv.Key;
                tempvm.FCount = (int)forecast;
                tempvm.ShipCount = (int)shipqty;
                flist.Add(tempvm);
            }

            flist.Sort(delegate (ShipForcastData obj1, ShipForcastData obj2)
            {
                var d1 = UT.O2T(obj1.DataTime + "-01 00:00:00");
                var d2 = UT.O2T(obj2.DataTime + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

        }


        public ShipForcastData()
        {
            DataPath = "";
            DataUpdateStamp = "";
            PN = "";
            DataTime = "";
            FCount = 0;
            Accuracy = 1.0;
            ShipCount = 0;
        }

        public string PN { set; get; }
        public string DataTime { set; get; }
        public int FCount { set; get; }
        public string DataPath { set; get; }
        public string DataUpdateStamp { set; get; }
        public double Accuracy { set; get; }
        public int ShipCount { set; get; }
    }
}