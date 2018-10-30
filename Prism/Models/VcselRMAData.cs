using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class VcselRMAData
    {
        public static Dictionary<string, int> RetrieveRMACntByMonth(string sdate, string edate, string rate)
        {
            var ret = new Dictionary<string, int>();
            var sql = "select ShipDate from VcselRMAData where ShipDate >= @sdate and ShipDate <=@edate ";

            if (string.Compare(rate, VCSELRATE.r14G, true) == 0)
            { sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')"; }
            else
            { sql = sql + " and VcselType = '" + rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var dbret = DBUtility.ExeNPISqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var m = Convert.ToDateTime(line[0]).ToString("yyyy-MM");
                if (ret.ContainsKey(m))
                {
                    ret[m] = ret[m] + 1;
                }
                else
                {
                    ret.Add(m, 1);
                }
            }
            return ret;
        }

        public static List<VcselRMAData> RetrieveWaferRawDataByMonth(string sdate, string edate, string rate)
        {
            var ret = new List<VcselRMAData>();
            var sql = "select SN,BuildDate,Wafer,PN,PNDesc,VcselPN,VcselType,ProductType,ShipDate,RMAOpenDate,RMANum,Customer from VcselRMAData where ShipDate >= @sdate and ShipDate <= @edate ";
            if (rate.Contains(VCSELRATE.r14G))
            { sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')"; }
            else
            { sql = sql + " and VcselType = '" + rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);

            var dbret = DBUtility.ExeNPISqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new VcselRMAData();
                tempvm.SN = Convert.ToString(line[0]);
                tempvm.BuildDate = Convert.ToDateTime(line[1]);
                tempvm.Wafer = Convert.ToString(line[2]);

                tempvm.PN = Convert.ToString(line[3]);
                tempvm.PNDesc = Convert.ToString(line[4]);
                tempvm.VcselPN = Convert.ToString(line[5]);
                tempvm.VcselType = Convert.ToString(line[6]);

                tempvm.ProductType = Convert.ToString(line[7]);
                tempvm.ShipDate = Convert.ToString(line[8]);
                tempvm.RMAOpenDate = Convert.ToString(line[9]);
                tempvm.RMANum = Convert.ToString(line[10]);
                tempvm.Customer = Convert.ToString(line[11]);

                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            {
                var sncond = "('";
                foreach (var item in ret)
                {
                    if (!string.IsNullOrEmpty(item.SN))
                    {
                        sncond = sncond + item.SN + "','";
                    }
                }
                sncond = sncond.Substring(0, sncond.Length - 2) + ")";
                var snkeydict = RetrieveIssueBySNs(sncond);
                foreach (var item in ret)
                {
                    if (snkeydict.ContainsKey(item.SN))
                    {
                        item.IssueKey = snkeydict[item.SN];
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, string> RetrieveIssueBySNs(string sncond)
        {
            var ret = new Dictionary<string, string>();
            var sql = "select ModuleSN,IssueKey from Issue where ModuleSN in <sncond> and IssueType='<IssueType>'";
            sql = sql.Replace("<sncond>", sncond).Replace("<IssueType>", "RMA");
            var dbret = DBUtility.ExeNPISqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                var key = Convert.ToString(line[1]);
                if (!ret.ContainsKey(sn))
                {
                    ret.Add(sn, key);
                }
            }
            return ret;
        }

        public VcselRMAData()
        {
            SN = "";
            BuildDate = DateTime.Parse("1982-05-06 10:00:00");
            Wafer = "";

            PN = "";
            PNDesc = "";
            VcselPN = "";
            VcselType = "";

            ProductType = "";
            ShipDate = "";
            RMAOpenDate = "";
            RMANum = "";
            Customer = "";
            IssueKey = "";
        }

        public string SN { set; get; }
        public DateTime BuildDate { set; get; }
        public string Wafer { set; get; }


        public string PN { set; get; }
        public string PNDesc { set; get; }
        public string VcselPN { set; get; }
        public string VcselType { set; get; }

        public string ProductType { set; get; }
        public string ShipDate { set; get; }
        public string RMAOpenDate { set; get; }
        public string RMANum { set; get; }
        public string Customer { set; get; }
        public string IssueKey { set; get; }

    }
}