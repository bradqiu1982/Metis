using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class YIELDTYPE
    {
        public static string FIRSTPASSYIELD = "FIRSTPASSYIELD";
        public static string FINALYIELD = "FINALYIELD";
    }
         
    public class YieldPreData
    {
        private static void StoreData(DateTime month, string productfamily, string yieldtype, string whichtest, string failure, int num)
        {
            var sql = @"insert into YieldPreData(YieldMonth,ProductFamily,WhichTest,Failure,FailureNum,YieldType) 
                         values(@YieldMonth,@ProductFamily,@WhichTest,@Failure,@FailureNum,@YieldType)";
            var dict = new Dictionary<string, string>();
            dict.Add("@YieldMonth",month.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ProductFamily",productfamily);
            dict.Add("@WhichTest",whichtest);
            dict.Add("@Failure",failure);
            dict.Add("@FailureNum",num.ToString());
            dict.Add("@YieldType",yieldtype);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void CleanData(DateTime month, string productfamily)
        {
            var sql = "delete from YieldPreData where YieldMonth = @YieldMonth and ProductFamily = @ProductFamily";
            var dict = new Dictionary<string, string>();
            dict.Add("@YieldMonth", month.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ProductFamily", productfamily);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void SolveTestData(List<ModuleTestData> rawdata,DateTime month,string productfamily,string yieldtype)
        {
            var sntestdict = new Dictionary<string, bool>();
            var whichtestfailuredict = new Dictionary<string, Dictionary<string, int>>();
            foreach (var item in rawdata)
            {
                var sntest = item.ModuleSN.ToUpper().Trim() +":::"+ item.WhichTest.ToUpper().Trim();
                if (!sntestdict.ContainsKey(sntest))
                {
                    sntestdict.Add(sntest, true);

                    var whichtest = item.WhichTest.ToUpper().Trim();
                    var failure = item.ErrAbbr.ToUpper().Trim();
                    if (whichtestfailuredict.ContainsKey(whichtest))
                    {
                        var failuredict = whichtestfailuredict[whichtest];
                        if (failuredict.ContainsKey(failure))
                        { failuredict[failure] += 1; }
                        else
                        { failuredict.Add(failure, 1); }
                    }
                    else
                    {
                        var tempdict = new Dictionary<string, int>();
                        tempdict.Add(failure, 1);
                        whichtestfailuredict.Add(whichtest, tempdict);
                    }
                }//end if
            }//end foreach

            foreach (var testkv in whichtestfailuredict)
            {
                foreach (var failurekv in testkv.Value)
                {
                    StoreData(month, productfamily, yieldtype, testkv.Key, failurekv.Key, failurekv.Value);
                }//end foreach
            }//end foreach

        }

        public static void YieldPreScan(Controller ctrl)
        {
            var productfamilys = ModuleTestData.RetrieveAllProductFamily();
            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var nowmonth = DateTime.Now.ToString("yyyy-MM");

            foreach (var pf in productfamilys)
            {
                var zerodate = DateTime.Parse(yieldcfg["YIELDZERODATE"]);
                for (; zerodate < DateTime.Now;)
                {
                    var iscurrentmonth = false;
                    if (string.Compare(zerodate.ToString("yyyy-MM"), nowmonth) == 0)
                    { iscurrentmonth = true; }

                    if (YieldRawData.IsYieldDataActionUpdated(zerodate.ToString("yyyy-MM"), "", pf, YIELDACTIONTYPE.MONTHLYYIELD))
                    {
                        zerodate = zerodate.AddMonths(1);
                        continue;
                    }

                    if (iscurrentmonth)
                    {
                        CleanData(zerodate, pf);
                    }

                    var testdatalist = ModuleTestData.RetrieveTestDate(pf, zerodate, zerodate.AddMonths(1));
                    if (testdatalist.Count > 0)
                    {
                        try
                        {
                            SolveTestData(testdatalist, zerodate, pf, YIELDTYPE.FINALYIELD);
                            testdatalist.Reverse();
                            SolveTestData(testdatalist, zerodate, pf, YIELDTYPE.FIRSTPASSYIELD);
                        }
                        catch (Exception ex) { }
                    }

                    if (!iscurrentmonth)
                    {
                        YieldRawData.UpdateYieldDataAction(zerodate.ToString("yyyy-MM"), "", pf, YIELDACTIONTYPE.MONTHLYYIELD);
                    }

                    zerodate = zerodate.AddMonths(1);
                }
            }
        }

        public static List<string> RetrieveYieldPDFamilyList()
        {
            var ret = new List<string>();
            var sql = "select distinct ProductFamily FROM YieldPreData order by ProductFamily where ProductFamily <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static string Prod2PJKey(string pf)
        {
            var sql = "select productfamily,pjkey from Prod2PJKey where productfamily ='<productfamily>'";
            sql = sql.Replace("<productfamily>", pf);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][1]);
            }
            return string.Empty;
        }

        private static string RetrieveProjectKey(string pd)
        {
            var pndict = new Dictionary<string, bool>();
            var sql = "select distinct PN from ModuleTestData where ProductFamily = '<ProductFamily>' and PN <> ''";
            sql = sql.Replace("<ProductFamily>", pd);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                if (!pndict.ContainsKey(pn))
                { pndict.Add(pn, true); }
            }

            if (pndict.Count == 0)
            { return string.Empty; }

            var pjkeydict = new Dictionary<string, int>();
            var pncond = "('" + string.Join("','", pndict.Keys.ToList()) + "')";
            sql = "select distinct ProjectKey,PN from ProjectTestData where PN in <pncond>";
            sql = sql.Replace("<pncond>", pncond);
            dbret = DBUtility.ExeNPISqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pjkey = Convert.ToString(line[0]);
                var pn = Convert.ToString(line[1]);
                if (pjkeydict.ContainsKey(pjkey))
                {
                    pjkeydict[pjkey] += 1;
                }
                else
                {
                    pjkeydict.Add(pjkey, 1);
                }
            }

            if (pjkeydict.Count == 0)
            { return string.Empty; }

            var matched = 0;
            var ret = "";
            foreach (var kv in pjkeydict)
            {
                if (kv.Value > matched)
                {
                    matched = kv.Value;
                    ret = kv.Key;
                }
            }

            return ret;
        }

        public static void LoadProjectKey()
        {
            var productfamilys = ModuleTestData.RetrieveAllProductFamily();
            foreach (var pf in productfamilys)
            {
                if (string.IsNullOrEmpty(Prod2PJKey(pf)))
                {
                    var pjkey = RetrieveProjectKey(pf);
                    if (!string.IsNullOrEmpty(pjkey))
                    {
                        var sql = "insert into Prod2PJKey(productfamily,pjkey) values(@productfamily,@pjkey)";
                        var dict = new Dictionary<string, string>();
                        dict.Add("@productfamily",pf);
                        dict.Add("@pjkey",pjkey);
                        DBUtility.ExeLocalSqlNoRes(sql, dict);
                    }
                }//end if
            }//end foreach
        }

        public DateTime YieldMonth { set; get; }
        public string ProductFamily { set; get; }
        public string WhichTest { set; get; }
        public string Failure { set; get; }
        public int FailureNum { set; get; }
        public string YieldType { set; get; }
    }
}