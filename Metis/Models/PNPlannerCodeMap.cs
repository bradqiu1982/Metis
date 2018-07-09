using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metis.Models
{
    public class PNPlannerCodeMap
    {
        public PNPlannerCodeMap()
        {
            PN = "";
            PlannerCode = "";
            PJName = "";
        }

        public static Dictionary<string, PNPlannerCodeMap> RetrieveAllMaps()
        {
            var ret = new Dictionary<string, PNPlannerCodeMap>();
            var sql = "select PN,PlannerCode,PJName from PNPlannerCodeMap";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                var plcode = Convert.ToString(line[1]);
                var pjname = Convert.ToString(line[2]);
                if (!ret.ContainsKey(pn))
                {
                    var tempvm = new PNPlannerCodeMap();
                    tempvm.PN = pn;
                    tempvm.PlannerCode = plcode;
                    tempvm.PJName = pjname;
                    ret.Add(pn, tempvm);
                }
            }
            return ret;
        }

        public void StoreData()
        {
            var sql = "insert into PNPlannerCodeMap(PN,PlannerCode,PJName) values(@PN,@PlannerCode,@PJName)";
            var param = new Dictionary<string, string>();
            param.Add("@PN",PN);
            param.Add("@PlannerCode", PlannerCode);
            param.Add("@PJName", PJName);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public string PN { set; get; }
        public string PlannerCode { set; get; }
        public string PJName { set; get; }
    }

}