using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class MachinePreData
    {

        private static void StoreData(DateTime month, string productfamily,string machine,string whichtest,int spendtime)
        {
            var sql = @"insert into MachinePreData(MachineMonth,ProductFamily,Machine,WhichTest,SpendTime) 
                         values(@MachineMonth,@ProductFamily,@Machine,@WhichTest,@SpendTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@MachineMonth", month.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ProductFamily", productfamily);
            dict.Add("@Machine", machine);
            dict.Add("@WhichTest", whichtest);
            dict.Add("@SpendTime", spendtime.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void CleanData(DateTime month, string productfamily)
        {
            var sql = "delete from MachinePreData where MachineMonth = @MachineMonth and ProductFamily = @ProductFamily";
            var dict = new Dictionary<string, string>();
            dict.Add("@MachineMonth", month.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ProductFamily", productfamily);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void SolveTestData(List<ModuleTestData> rawdata, DateTime month, string productfamily)
        {
            var machinetestdict = new Dictionary<string, Dictionary<string, int>>();
            foreach (var item in rawdata)
            {
                if (!string.IsNullOrEmpty(item.SpendTime))
                {
                    try
                    {
                        var spt = Convert.ToInt32(item.SpendTime);
                        var machine = item.TestStation.ToUpper().Trim();
                        var whichtest = item.WhichTest.ToUpper().Trim();

                        if (machinetestdict.ContainsKey(machine))
                        {
                            var testdict = machinetestdict[machine];
                            if (testdict.ContainsKey(whichtest))
                            { testdict[whichtest] += spt; }
                            else
                            { testdict.Add(whichtest, spt); }
                        }
                        else
                        {
                            var testdict = new Dictionary<string, int>();
                            testdict.Add(whichtest, spt);
                            machinetestdict.Add(machine, testdict);
                        }
                    }
                    catch (Exception ex) { }
                }//end if
            }//end foreach

            foreach (var mkv in machinetestdict)
            {
                foreach (var tkv in mkv.Value)
                {
                    StoreData(month, productfamily, mkv.Key, tkv.Key, tkv.Value);
                }//end foreach
            }//end foreach

        }

        public static void MachinePreScan(Controller ctrl)
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

                    if (YieldRawData.IsYieldDataActionUpdated(zerodate.ToString("yyyy-MM"), "", pf, YIELDACTIONTYPE.MONTHLYMACHINE))
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
                        SolveTestData(testdatalist, zerodate, pf);
                    }

                    if (!iscurrentmonth)
                    {
                        YieldRawData.UpdateYieldDataAction(zerodate.ToString("yyyy-MM"), "", pf, YIELDACTIONTYPE.MONTHLYMACHINE);
                    }
                    zerodate = zerodate.AddMonths(1);
                }
            }
        }

        public DateTime MachineMonth { set; get; }
        public string ProductFamily { set; get; }
        public string Machine { set; get; }
        public string WhichTest { set; get; }
        public int SpendTime { set; get; }
       
    }
}