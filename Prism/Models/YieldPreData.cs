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

                    if (YieldRawData.IsYieldDataActionUpdated(zerodate.ToString("yyyy-MM"), "", pf, YIELDACTIONTYPE.COMPUTER))
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
                        SolveTestData(testdatalist, zerodate, pf, YIELDTYPE.FINALYIELD);
                        testdatalist.Reverse();
                        SolveTestData(testdatalist, zerodate, pf, YIELDTYPE.FIRSTPASSYIELD);
                    }

                    if (!iscurrentmonth)
                    {
                        YieldRawData.UpdateYieldDataAction(zerodate.ToString("yyyy-MM"), "", pf, YIELDACTIONTYPE.COMPUTER);
                    }
                    break;
                }
            }
        }

        public DateTime YieldMonth { set; get; }
        public string ProductFamily { set; get; }
        public string WhichTest { set; get; }
        public string Failure { set; get; }
        public int FailureNum { set; get; }
        public string YieldType { set; get; }
    }
}