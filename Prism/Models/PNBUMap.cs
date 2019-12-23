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
                        pnlist.Add(new PNBUMap(line[0],line[2],line[4],line[5],line[6]));
                    }
                }

                foreach (var p in pnlist)
                { p.StoreData(); }
            }
        }

        public static Dictionary<string, PNBUMap> GetPNMap()
        {
            var ret = new Dictionary<string, PNBUMap>();
            var sql = "select PN,PlannerCode,ProjectGroup,Series,BU from BSSupport.dbo.PNBUMap";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pn = UT.O2S(line[0]);
                if (!ret.ContainsKey(pn))
                {
                    var tempvm = new PNBUMap(pn,UT.O2S(line[1])
                        , UT.O2S(line[2]), UT.O2S(line[3]), UT.O2S(line[4]));
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
            { sql = "update BSSupport.dbo.PNBUMap set PlannerCode=@PlannerCode,ProjectGroup=@ProjectGroup,Series=@Series,BU=@BU where PN = @PN"; }
            else
            { sql = "insert into BSSupport.dbo.PNBUMap(PN,PlannerCode,ProjectGroup,Series,BU) values(@PN,@PlannerCode,@ProjectGroup,@Series,@BU)";}
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            dict.Add("@PlannerCode", PlannerCode);
            dict.Add("@ProjectGroup", ProjectGroup);
            dict.Add("@Series", Series);
            dict.Add("@BU", BU);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public PNBUMap(string pn,string pc,string pg,string s,string bu)
        {
            PN = pn;
            PlannerCode = pc;
            ProjectGroup = pg;
            Series = s;
            BU = bu;
        }

        public PNBUMap()
        {
            PN = "";
            PlannerCode = "";
            ProjectGroup = "";
            Series = "";
            BU = "";
        }

        public string PN { set; get; }
        public string PlannerCode { set; get; }
        public string ProjectGroup { set; get; }
        public string Series { set; get; }
        public string BU { set; get; }
    }
}