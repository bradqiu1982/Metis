using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Prism.Models
{
    public class ModuleTestData
    {
        public ModuleTestData()
        {
            Init();
        }

        public ModuleTestData(string dataid,string sn,string tm,string wt,string err,string station,
            string pf,string pn,string pndesc,string mt,string spd,string spt,string mes,string yf)
        {
            DataID = dataid;
            ModuleSN = sn;
            TestTimeStamp = tm;
            WhichTest = wt;
            ErrAbbr = err;
            TestStation = station;
            ProductFamily = pf;
            PN = pn;
            PNDesc = pndesc;
            ModuleType = mt;
            SpeedRate = spd;
            SpendTime = spt;
            MESTab = mes;
            YieldFamily = yf;
        }

        private void Init()
        {
            DataID = "";
            ModuleSN = "";
            TestTimeStamp = "";
            WhichTest = "";
            ErrAbbr = "";
            TestStation = "";
            ProductFamily = "";
            PN = "";
            PNDesc = "";
            ModuleType = "";
            SpeedRate = "";
            SpendTime = "";
            MESTab = "";
            YieldFamily = "";
        }

        public static void StoreData(List<ModuleTestData> datalist)
        {
            var newlist = new List<object>();
            foreach (var item in datalist)
            { newlist.Add(item); }
            DBUtility.WriteDBWithTable(newlist, typeof(ModuleTestData), "ModuleTestData");
        }

        public static void CleanTestData(string yieldfamily, string mestab, DateTime startdate)
        {
            var sql = "delete from ModuleTestData where MESTab='<MESTab>' and YieldFamily = '<YieldFamily>' and TestTimeStamp >= '<startdate>' and TestTimeStamp < '<enddate>'";
            sql = sql.Replace("<MESTab>",mestab).Replace("<YieldFamily>",yieldfamily)
                .Replace("<startdate>",startdate.ToString("yyyy-MM-dd HH:mm:dd")).Replace("<enddate>", startdate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:dd"));
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<string> RetrieveAllProductFamily()
        {
            var ret = new List<string>();
            var sql = "select distinct ProductFamily from ModuleTestData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<ModuleTestData> RetrieveTestDate(string productfamily, DateTime startdate,DateTime enddate)
        {
            var ret = new List<ModuleTestData>();

            var sql = @"select DataID,ModuleSN,TestTimeStamp,WhichTest,ErrAbbr,TestStation,ProductFamily,PN,PNDesc,ModuleType,SpeedRate,SpendTime,MESTab,YieldFamily from ModuleTestData 
                            where ProductFamily = '<ProductFamily>' and TestTimeStamp >= '<startdate>' and TestTimeStamp < '<enddate>' order by ModuleSN,TestTimeStamp desc";
            sql = sql.Replace("<ProductFamily>", productfamily).Replace("<startdate>", startdate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("<enddate>", enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(new ModuleTestData(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss")
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8]), Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]), Convert.ToString(line[13])));
            }
            return ret;
        }

        public string DataID { set; get; }
        public string ModuleSN { set; get; }
        public string TestTimeStamp { set; get; }
        public string WhichTest { set; get; }
        public string ErrAbbr { set; get; }
        public string TestStation { set; get; }
        public string ProductFamily { set; get; }

        public string PN { set; get; }
        public string PNDesc { set; get; } 
        public string ModuleType { set; get; }
        public string SpeedRate { set; get; }
        public string SpendTime { set; get; }

        public string MESTab { set; get; }
        public string YieldFamily { set; get; }

    }
}