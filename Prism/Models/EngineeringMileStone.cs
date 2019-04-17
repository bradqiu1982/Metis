using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class EngineeringMileStone
    {
        public EngineeringMileStone()
        {
            PJKey = "";
            ActionDate = DateTime.Parse("1982-05-06 10:00:00");
            Location = "";
            ActionDetail = "";
            AppendInfo = "";
        }

        public static void LoadMileStone(Controller ctrl, string vcselrmafile)
        {
            var idx = 0;
            var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, vcselrmafile, "Changes milestone");
            var milestonelist = new List<EngineeringMileStone>();

            foreach (var line in data)
            {
                if (idx == 0)
                {
                    idx = idx + 1;
                    continue;
                }
                try
                {
                    var tempvm = new EngineeringMileStone();
                    tempvm.ActionDate = DateTime.Parse(line[2]);
                    tempvm.Location = line[3];
                    tempvm.ActionDetail = line[0] + " # " + line[4];
                    tempvm.AppendInfo = line[0];
                    if (string.IsNullOrEmpty(tempvm.AppendInfo))
                    {
                        tempvm.AppendInfo = "OTHERS";
                    }
                    milestonelist.Add(tempvm);
                }
                catch (Exception ex) { }
            }

            if (milestonelist.Count > 0)
            {
                EngineeringMileStone.UpdateVcselMileStone(milestonelist);
            }
        }

        private static void CleanMileStone(string pjkey)
        {
            var sql = "delete from EngineeringMileStone where  PJKey = @PJKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@PJKey", pjkey);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void UpdateVcselMileStone(List<EngineeringMileStone> elist)
        {
            var pjkey = "VCSEL";
            CleanMileStone(pjkey);

            foreach (var item in elist)
            {
                var sql = "insert into EngineeringMileStone(PJKey,ActionDate,Location,ActionDetail,AppendInfo) values(@PJKey,@ActionDate,@Location,@ActionDetail,@AppendInfo)";
                var dict = new Dictionary<string, string>();
                dict.Add("@PJKey", pjkey);
                dict.Add("@ActionDate", item.ActionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                dict.Add("@Location", item.Location);
                dict.Add("@ActionDetail", item.ActionDetail);
                dict.Add("@AppendInfo", item.AppendInfo);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        public static List<EngineeringMileStone> RetrieveVcselMileStone()
        {
            var ret = new List<EngineeringMileStone>();

            var pjkey = "VCSEL";
            var sql = "select PJKey,ActionDate,Location,ActionDetail,AppendInfo from EngineeringMileStone where PJKey = '<PJKey>'";
            sql = sql.Replace("<PJKey>", pjkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new EngineeringMileStone();
                tempvm.PJKey = pjkey;
                tempvm.ActionDate = Convert.ToDateTime(line[1]);
                tempvm.Location = Convert.ToString(line[2]);
                tempvm.ActionDetail = Convert.ToString(line[3]);
                tempvm.AppendInfo = Convert.ToString(line[4]);
                ret.Add(tempvm);
            }

            return ret;
        }

        public string PJKey { set; get; }
        public DateTime ActionDate { set; get; }
        public string Location { set; get; }
        public string ActionDetail { set; get; }
        public string AppendInfo { set; get; }
    }
}