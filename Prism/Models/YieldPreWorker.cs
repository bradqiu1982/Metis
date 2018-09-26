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
         
    public class YieldPreWorker
    {

        private static void SolveTestData(List<ModuleTestData> rawdata,DateTime month,string productfamily,string yieldtype)
        {

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

        public string ProductFamily { set; get; }
        public DateTime YieldMonth { set; get; }
        public string WhichTest { set; get; }
        public string Failure { set; get; }
        public int FailureNum { set; get; }
        public string YieldType { set; get; }
    }
}