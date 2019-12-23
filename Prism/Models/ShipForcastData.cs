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
                    if (cname.ToUpper().Contains("SOURCE ORG"))
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

        public ShipForcastData()
        {
            DataPath = "";
            DataUpdateStamp = "";
            PN = "";
            DataTime = "";
            FCount = 0;
        }

        public string PN { set; get; }
        public string DataTime { set; get; }
        public int FCount { set; get; }
        public string DataPath { set; get; }
        public string DataUpdateStamp { set; get; }
    }
}