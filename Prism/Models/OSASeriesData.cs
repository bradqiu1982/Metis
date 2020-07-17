using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class OSASeriesData
    {
        public static void LoadData(Controller ctrl)
        {
            var ret = new Dictionary<string, KeyValueCLA>();
            
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var osayield = syscfg["OSASERIESYIELD"];
            var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, osayield, "Sheet1");
            var idx = 0;
            foreach (var line in data)
            {
                if (idx == 0)
                { idx++; continue; }

                if (!string.IsNullOrEmpty(line[0])
                    && !string.IsNullOrEmpty(line[1])
                    && !string.IsNullOrEmpty(line[2]))
                {

                    var mkfm = line[0].ToUpper();
                    var series = line[1].ToUpper();
                    var yield = UT.O2D(line[2]);
                    var ystr = yield.ToString();
                    if (yield > 1)
                    { ystr = (yield / 100.0).ToString(); }

                    if (!ret.ContainsKey(mkfm))
                    {
                        var tempvm = new KeyValueCLA();
                        tempvm.Key = series;
                        tempvm.Value = ystr;
                        ret.Add(mkfm, tempvm);
                    }
                }
            }

            var mklist = ret.Keys.ToList();
            CleanData(mklist);

            foreach (var item in ret)
            {
                StoreData(item.Key, item.Value.Key, item.Value.Value);
            }
        }

        private static void CleanData(List<string> mklist)
        {
            var scond = "('"+string.Join("','", mklist) +"')";
            var sql = "delete from [BSSupport].[dbo].[OSASeriesData] where [MKFM] in <scond>";
            sql = sql.Replace("<scond>", scond);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static void StoreData(string mk, string series, string ystr)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@MKFM", mk);
            dict.Add("@Series", series);
            dict.Add("@Yield", ystr);
            var sql = "insert into [BSSupport].[dbo].[OSASeriesData](MKFM,Series,Yield) values(@MKFM,@Series,@Yield)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, KeyValueCLA> GetOSASeriesYIELD()
        {
            var ret = new Dictionary<string, KeyValueCLA>();
            var sql = "select MKFM,Series,Yield from  [BSSupport].[dbo].[OSASeriesData]";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var mkfm = UT.O2S(line[0]);
                var series = UT.O2S(line[1]);
                var yield = UT.O2S(line[2]);
                if (!ret.ContainsKey(mkfm))
                {
                    var tempvm = new KeyValueCLA();
                    tempvm.Key = series;
                    tempvm.Value = yield;
                    ret.Add(mkfm, tempvm);
                }
            }

            return ret;
        }

        public static Dictionary<string, bool> GetOSASeriesDict()
        {
            var ret = new Dictionary<string, bool>();

            var sql = "select distinct Series from  [BSSupport].[dbo].[OSASeriesData]";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var series = UT.O2S(line[0]);
                ret.Add(series, true);
            }
            return ret;
        }
    }
}