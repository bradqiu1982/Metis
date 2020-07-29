using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class PNBUMap
    {
        public static void LoadPNBUData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var busrcfile = syscfg["PNVSBU"];
            var budesfile = ExternalDataCollector.DownloadShareFile(busrcfile, ctrl);
            if (!string.IsNullOrEmpty(budesfile))
            {
                var idx = 0;
                var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, budesfile);
                var pnlist = new List<PNBUMap>();
                foreach (var line in data)
                {
                    if (idx == 0)
                    { idx++;continue; }

                    if (!string.IsNullOrEmpty(line[0]))
                    {
                        pnlist.Add(new PNBUMap(line[0],line[2],line[4],line[5],line[6],line[10]));
                    }
                }

                foreach (var p in pnlist)
                { p.StoreData(); }
            }
        }

        public static Dictionary<string, PNBUMap> GetPNMap()
        {
            var ret = new Dictionary<string, PNBUMap>();
            var sql = "select PN,PlannerCode,ProjectGroup,Series,BU,AppVal1 from BSSupport.dbo.PNBUMap";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pn = UT.O2S(line[0]);
                if (!ret.ContainsKey(pn))
                {
                    var tempvm = new PNBUMap(pn,UT.O2S(line[1])
                        , UT.O2S(line[2]), UT.O2S(line[3]), UT.O2S(line[4]),UT.O2S(line[5]));
                    ret.Add(pn, tempvm);
                }
            }
            return ret;
        }

        private bool DataExist()
        {
            var sql = "select  * from BSSupport.dbo.PNBUMap where PN = @PN";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null, dict);
            if (dbret.Count > 0)
            { return true; }
            return false;
        }

        private void StoreData()
        {
            var sql = "";
            if (DataExist())
            { sql = "update BSSupport.dbo.PNBUMap set PlannerCode=@PlannerCode,ProjectGroup=@ProjectGroup,Series=@Series,BU=@BU,AppVal1=@Series2 where PN = @PN"; }
            else
            { sql = "insert into BSSupport.dbo.PNBUMap(PN,PlannerCode,ProjectGroup,Series,BU,AppVal1) values(@PN,@PlannerCode,@ProjectGroup,@Series,@BU,@Series2)"; }
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            dict.Add("@PlannerCode", PlannerCode);
            dict.Add("@ProjectGroup", ProjectGroup);
            dict.Add("@Series", Series);
            dict.Add("@BU", BU);
            dict.Add("@Series2", Series2);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<PNBUMap> GetActiveSeries(string starttime,string endtime)
        {
            var budict = new Dictionary<string, bool>();
            var dict = new Dictionary<string, string>();
            dict.Add("@starttime", starttime);
            dict.Add("@endtime", endtime);

            var sql = @"select distinct bu.BU,bu.ProjectGroup,bu.Series from [BSSupport].[dbo].[PNBUMap] bu with(nolock) 
                          left join [BSSupport].[dbo].[ShipForcastData] fc with(nolock) on fc.PN = bu.PN
                          where fc.DataTime > @starttime and  fc.DataTime < @endtime and fc.DataUpdateStamp  > @starttime and fc.DataUpdateStamp  < @endtime and bu.Series <> ''
                          and bu.BU in ('TRANSCEIVER','COHERENT','WAVELENGTH MANAGEMENT') order by BU,ProjectGroup,Series";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new PNBUMap();
                tempvm.BU = UT.O2S(line[0]);
                tempvm.ProjectGroup = UT.O2S(line[1]);
                tempvm.Series = UT.O2S(line[2]);
                var key = tempvm.BU + tempvm.ProjectGroup + tempvm.Series;
                if (!budict.ContainsKey(key))
                { budict.Add(key, true); }
            }

            var ret = new List<PNBUMap>();
            sql = @"select distinct bu.BU,bu.ProjectGroup,bu.Series from [BSSupport].[dbo].[PNBUMap] bu with(nolock) 
                          left join [BSSupport].[dbo].[FsrShipData] sp with(nolock)  on bu.PN = sp.PN 
                          where sp.ShipDate > @starttime and sp.ShipDate < @endtime  and bu.Series <> ''
                            and bu.BU in ('TRANSCEIVER','COHERENT','WAVELENGTH MANAGEMENT') order by BU,ProjectGroup,Series";
            
            dbret = DBUtility.ExeLocalSqlWithRes(sql,null,dict);
            foreach (var line in dbret)
            {
                var tempvm = new PNBUMap();
                tempvm.BU = UT.O2S(line[0]);
                tempvm.ProjectGroup = UT.O2S(line[1]);
                tempvm.Series = UT.O2S(line[2]);
                var key = tempvm.BU + tempvm.ProjectGroup + tempvm.Series;
                if (budict.ContainsKey(key))
                { ret.Add(tempvm); }
            }

            return ret;
        }

        public static List<PNBUMap> GetModuleRevenueActiveSeries(string starttime,string BUCond)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@starttime", starttime);

            var ret = new List<PNBUMap>();
            var sql = @"select distinct bu.BU,bu.ProjectGroup,bu.Series from [BSSupport].[dbo].[PNBUMap] bu with(nolock) 
                          inner join [BSSupport].[dbo].[FsrShipData] sp with(nolock)  on bu.PN = sp.PN 
                          where sp.ShipDate >= @starttime  and bu.Series <> ''
                            and bu.BU in ("+BUCond+") order by BU,ProjectGroup,Series";

           var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new PNBUMap();
                tempvm.BU = UT.O2S(line[0]);
                tempvm.ProjectGroup = UT.O2S(line[1]);
                tempvm.Series = UT.O2S(line[2]);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static Dictionary<string, string> GetSeriesPNMap()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select Series,PN from [BSSupport].[dbo].[PNBUMap] where Series <> '' order by Series,PN";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var series = UT.O2S(line[0]).Trim().ToUpper();
                var pn = UT.O2S(line[1]);
                if (!ret.ContainsKey(series))
                {
                    ret.Add(series, pn);
                }
            }

            sql = "select AppVal1,PN from [BSSupport].[dbo].[PNBUMap] where AppVal1 <> '' order by AppVal1,PN";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var series = UT.O2S(line[0]).Trim().ToUpper();
                var pn = UT.O2S(line[1]);
                if (!ret.ContainsKey(series))
                {
                    ret.Add(series, pn);
                }
            }

            return ret;
        }

        public static Dictionary<string, bool> GetWSSSeries()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct Series from [BSSupport].[dbo].[PNBUMap] where ProjectGroup = 'WSS' and Series <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var series = UT.O2S(line[0]).Trim().ToUpper();
                if (!ret.ContainsKey(series))
                {
                    ret.Add(series, true);
                }
            }

            sql = "select distinct AppVal1 from [BSSupport].[dbo].[PNBUMap] where ProjectGroup = 'WSS' and AppVal1 <> ''";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var series = UT.O2S(line[0]).Trim().ToUpper();
                if (!ret.ContainsKey(series))
                {
                    ret.Add(series, true);
                }
            }

            return ret;
        }

        public static Dictionary<string, string> GetSeriesPLMDict()
        {
            var ret = new Dictionary<string, string>();
            var sql = @"select distinct b.Series,p.PLM FROM [BSSupport].[dbo].[PNBUMap] b
                      left join [BSSupport].[dbo].[PLMMatrix] p on b.PN = p.PN
                      where p.PLM <> '' and b.Series <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var s = UT.O2S(line[0]).ToUpper();
                var p = UT.O2S(line[1]);
                if (!ret.ContainsKey(s))
                {
                    ret.Add(s, p);
                }
            }

            sql = @"select distinct b.AppVal1,p.PLM FROM [BSSupport].[dbo].[PNBUMap] b
                  left join [BSSupport].[dbo].[PLMMatrix] p on b.PN = p.PN
                  where p.PLM <> '' and b.AppVal1 <> ''";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var s = UT.O2S(line[0]).ToUpper();
                var p = UT.O2S(line[1]);
                if (!ret.ContainsKey(s))
                {
                    ret.Add(s, p);
                }
            }

            return ret;
        }

        public PNBUMap(string pn,string pc,string pg,string s,string bu,string s2)
        {
            PN = pn;
            PlannerCode = pc;
            ProjectGroup = pg;
            Series = s;
            BU = bu.Split(new string[] { "("},StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            Accuracy = 0.0;
            Series2 = s2;
        }

        public PNBUMap()
        {
            PN = "";
            PlannerCode = "";
            ProjectGroup = "";
            Series = "";
            BU = "";
            Accuracy = 0.0;
            Series2 = "";
            PLM = "";
        }


        public string PN { set; get; }
        public string PlannerCode { set; get; }
        public string ProjectGroup { set; get; }
        public string Series { set; get; }
        public string BU { set; get; }
        public double Accuracy { set; get; }
        public string Series2 { set; get; }
        public string PLM { set; get; }
    }
}