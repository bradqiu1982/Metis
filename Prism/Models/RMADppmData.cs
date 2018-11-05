using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class RMADppmData
    {
        public RMADppmData()
        {
            ID = string.Empty;
            RMANum = string.Empty;
            ProductType = string.Empty;
            PN = string.Empty;
            QTY = 0;
            IssueOpenDate = DateTime.Parse("1982-05-06 10:00:00");
            PNDesc = string.Empty;
            SN = string.Empty;
            RootCause = string.Empty;
        }

        public RMADppmData(string id, string rmanum, string producttype, string pn, string pndesc, string sn, double qty, DateTime issuedate, string rootcause)
        {
            ID = id;
            RMANum = rmanum;
            ProductType = producttype;
            PN = pn;
            PNDesc = pndesc;
            SN = sn;
            QTY = qty;
            IssueOpenDate = issuedate;
            RootCause = rootcause;
        }

        private static string Convert2Str(object obj)
        {
            if (obj == null)
            { return string.Empty; }
            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }

        private static DateTime Convert2DT(object obj)
        {
            if (obj == null)
            { return DateTime.Parse("1982-05-06 10:00:00"); }
            try
            {
                return Convert.ToDateTime(obj);
            }
            catch (Exception ex) { return DateTime.Parse("1982-05-06 10:00:00"); }
        }

        private static double Convert2DB(object obj)
        {
            if (obj == null)
            { return 0.0; }
            try
            {
                return Convert.ToDouble(obj);
            }
            catch (Exception ex) { return 0.0; }
        }

        private static Dictionary<string, bool> RetrieveAllID()
        {
            var ret = new Dictionary<string, bool>();

            var sql = "select ID from RMADppmData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var id = Convert2Str(line[0]);
                if (!ret.ContainsKey(id))
                { ret.Add(id, true); }
            }

            return ret;
        }

        public static void LoadRMADppmData()
        {
            var iddict = RetrieveAllID();
            var crtiddict = new Dictionary<string, bool>();

            var updaterootcausedict = new Dictionary<string, string>();

            var dppmlist = new List<RMADppmData>();
            var sql = "select AppV_B,AppV_F,AppV_G,AppV_H,AppV_I,AppV_J,AppV_P,AppV_X from [NPITrace].[dbo].[RMABackupData]";
            var dbret = DBUtility.ExeNPISqlWithRes(sql);
            foreach (var line in dbret)
            {
                var rmanum = Convert2Str(line[0]);
                var pn = Convert2Str(line[2]);
                var qty = Math.Round(Convert2DB(line[5]), 0);
                var rootcause = Convert2Str(line[7]);

                var id = rmanum + "_" + pn + "_" + qty;
                if (!iddict.ContainsKey(id) && !crtiddict.ContainsKey(id))
                {
                    crtiddict.Add(id, true);

                    var producttype = Convert2Str(line[1]);
                    var pndesc = Convert2Str(line[3]);
                    pndesc = pndesc.Length > 30 ? pndesc.Substring(0, 30) : pndesc;
                    var sn = Convert2Str(line[4]);
                    sn = sn.Length > 30 ? sn.Substring(0, 30) : sn;
                    var issuedate = Convert2DT(line[6]);

                    dppmlist.Add(new RMADppmData(id, rmanum, producttype, pn, pndesc, sn, qty, issuedate, rootcause));
                }
                else if (!string.IsNullOrEmpty(rootcause))
                {
                    if (!updaterootcausedict.ContainsKey(id))
                    {
                        updaterootcausedict.Add(id, rootcause);
                    }
                }
            }

            foreach (var item in dppmlist)
            {
                item.StoreData();
            }

            foreach (var kv in updaterootcausedict)
            {
                UpdateRootCause(kv.Key, kv.Value);
            }
        }

        public void StoreData()
        {
            var sql = "insert into RMADppmData(ID,RMANum,ProductType,PN,PNDesc,SN,QTY,IssueOpenDate,RootCause) values(@ID,@RMANum,@ProductType,@PN,@PNDesc,@SN,@QTY,@IssueOpenDate,@RootCause)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ID", ID);
            dict.Add("@RMANum", RMANum);
            dict.Add("@ProductType", ProductType);
            dict.Add("@PN", PN);
            dict.Add("@PNDesc", PNDesc);
            dict.Add("@SN", SN);
            dict.Add("@QTY", QTY.ToString());
            dict.Add("@IssueOpenDate", IssueOpenDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@RootCause", RootCause);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateRootCause(string id, string rootcause)
        {
            var sql = "update RMADppmData set RootCause = @RootCause where ID = @ID";
            var dict = new Dictionary<string, string>();
            dict.Add("@ID", id);
            dict.Add("@RootCause", rootcause);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, int> RetrieveParallelRMACntByMonth(string sdate, string edate)
        {
            var productcond = "ProductType like 'Parallel'";
            return RetrieveRMACntByMonth(sdate, edate, productcond);
        }

        public static Dictionary<string, int> RetrieveTunableRMACntByMonth(string sdate, string edate)
        {
            var productcond = "ProductType like 'Coherent Transceiver' or ProductType like 'COHERENT'  or ProductType like '10G Tunable' or ProductType like '10G T-XFP' or ProductType like 'TXFP' ";
            return RetrieveRMACntByMonth(sdate, edate, productcond);
        }

        private static Dictionary<string, int> RetrieveRMACntByMonth(string sdate, string edate, string productcond)
        {
            var ret = new Dictionary<string, int>();
            var sql = "select IssueOpenDate,QTY from RMADppmData where IssueOpenDate >= @sdate and IssueOpenDate <=@edate and RootCause <> 'NTF' and  RootCause <> '' and <productcond> ";
            sql = sql.Replace("<productcond>", productcond);

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var m = Convert.ToDateTime(line[0]).ToString("yyyy-MM");
                var qty = Convert.ToInt32(Convert2DB(line[1]));
                if (ret.ContainsKey(m))
                {
                    ret[m] = ret[m] + qty;
                }
                else
                {
                    ret.Add(m, qty);
                }
            }
            return ret;
        }

        private static List<RMADppmData> RetrieveRMADataByMonth(string sdate, string edate,string productcond)
        {
            var ret = new List<RMADppmData>();

            var sql = "select ID,RMANum,ProductType,PN,PNDesc,SN,QTY,IssueOpenDate,RootCause from RMADppmData where IssueOpenDate >= @sdate and IssueOpenDate <=@edate and RootCause <> 'NTF' and  RootCause <> '' and <productcond> ";
            sql = sql.Replace("<productcond>", productcond);
            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var rootcause = Convert2Str(line[8]);
                rootcause = rootcause.Length > 48 ? rootcause.Substring(0, 48) : rootcause;
                ret.Add(new RMADppmData(Convert2Str(line[0]), Convert2Str(line[1]), Convert2Str(line[2]), Convert2Str(line[3])
                    , Convert2Str(line[4]), Convert2Str(line[5]), Convert2DB(line[6]), Convert2DT(line[7]), rootcause)); 
            }
            return ret;
        }

        public static List<RMADppmData> RetrieveRMARawDataByMonth(string sdate, string edate,string producttype)
        {
            if (string.Compare(producttype, SHIPPRODTYPE.PARALLEL, true) == 0)
            {
                var productcond = "ProductType like 'Parallel'";
                return RetrieveRMADataByMonth(sdate, edate, productcond);
            }
            else if (string.Compare(producttype, SHIPPRODTYPE.OPTIUM, true) == 0)
            {
                var productcond = "ProductType like 'Coherent Transceiver' or ProductType like 'COHERENT'  or ProductType like '10G Tunable' or ProductType like '10G T-XFP' or ProductType like 'TXFP' ";
                return RetrieveRMADataByMonth(sdate, edate, productcond);
            }
            return new List<RMADppmData>();
        }

        public string ID { set; get; }
        public string RMANum { set; get; }
        public string ProductType { set; get; }
        public string PN { set; get; }
        public string PNDesc { set; get; }
        public string SN { set; get; }
        public double QTY { set; get; }
        public DateTime IssueOpenDate { set; get; }
        public string IssueDateStr { get { return IssueOpenDate.ToString("yyyy-MM-dd"); } }
        public string RootCause { set; get; }
    }
}