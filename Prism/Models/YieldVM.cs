using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class TestYieldVM
    {
        public TestYieldVM()
        {
            WhichTest = "";
            Pass = 0;
            Failed = 0;
            FailureMap = new Dictionary<string, int>();
        }


        public string WhichTest { set; get; }
        public int Pass { set; get; }
        public int Failed { set; get; }
        public Dictionary<string, int> FailureMap { set; get; }
        public double Yield {
            get {
                var totle = Pass + Failed;
                if (totle == 0)
                { return 0.0; }

                return (double)Pass / (double)totle * 100.0;
                }
        }
    }

    public class YieldVM
    {
        public static List<DateTime> RetrieveDateFromQuarter(string quarter)
        {
            var ret = new List<DateTime>();

            var splitstr = quarter.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var year = Convert.ToInt32(splitstr[0]);
            var q = splitstr[1];

            if (string.Compare(q, "Q1", true) == 0)
            {
                year = year - 1;
                ret.Add(DateTime.Parse(year.ToString() + "-05-01 00:00:00"));
                ret.Add(DateTime.Parse(year.ToString() + "-07-31 23:59:59"));
            }
            else if (string.Compare(q, "Q2", true) == 0)
            {
                year = year - 1;
                ret.Add(DateTime.Parse(year.ToString() + "-08-01 00:00:00"));
                ret.Add(DateTime.Parse(year.ToString() + "-10-31 23:59:59"));
            }
            else if (string.Compare(q, "Q4", true) == 0)
            {
                ret.Add(DateTime.Parse(year.ToString() + "-02-01 00:00:00"));
                ret.Add(DateTime.Parse(year.ToString() + "-04-30 23:59:59"));
            }
            else
            {
                ret.Add(DateTime.Parse((year-1).ToString() + "-11-01 00:00:00"));
                ret.Add(DateTime.Parse(year.ToString() + "-01-31 23:59:59"));
            }
            return ret;
        }

        public static string RetrieveQuarterFromDate(DateTime date)
        {
            var year = Convert.ToInt32(date.ToString("yyyy"));
            var month = Convert.ToInt32(date.ToString("MM"));
            if (month >= 5 && month <= 7)
            {
                return (year + 1).ToString() + " " + "Q1";
            }
            else if (month >= 8 && month <= 10)
            {
                return (year + 1).ToString() + " " + "Q2";
            }
            else if (month >= 2 && month <= 4)
            {
                return year.ToString() + " " + "Q4";
            }
            else if (month >= 11 && month <= 12)
            {
                return (year + 1).ToString() + " " + "Q3";
            }
            else
            {
                return year.ToString() + " " + "Q2";
            }
        }

        public YieldVM()
        {
            ProductFamily = "";
            Quarter = "";
            TestYieldList = new List<TestYieldVM>();
            FailureMap = new Dictionary<string, int>();
        }

        private static void _loadyielddict(Dictionary<string, Dictionary<string, Dictionary<string, int>>> firstyielddict
            ,string quarter,string whichtest,string failure,int failurenum)
        {
            if (firstyielddict.ContainsKey(quarter))
            {
                var wdict = firstyielddict[quarter];
                if (wdict.ContainsKey(whichtest))
                {
                    var fdict = wdict[whichtest];
                    if (fdict.ContainsKey(failure))
                    { fdict[failure] += failurenum; }
                    else
                    { fdict.Add(failure, failurenum); }
                }
                else
                {
                    var fdict = new Dictionary<string, int>();
                    fdict.Add(failure, failurenum);
                    wdict.Add(whichtest, fdict);
                }
            }
            else
            {
                var fdict = new Dictionary<string, int>();
                fdict.Add(failure, failurenum);
                var wdict = new Dictionary<string, Dictionary<string, int>>();
                wdict.Add(whichtest, fdict);
                firstyielddict.Add(quarter, wdict);
            }
        }

        private static List<YieldVM> _pumpyielddata(string pdfamily,List<string> quarterlist, Dictionary<string, Dictionary<string, Dictionary<string, int>>> firstyielddict)
        {
            var ret = new List<YieldVM>();
            foreach (var q in quarterlist)
            {
                var vm = new YieldVM();
                vm.ProductFamily = pdfamily;
                vm.Quarter = q;
                var wdict = firstyielddict[q];
                var maxinput = 0;
                var temptestyieldlist = new List<TestYieldVM>();
                foreach (var wkv in wdict)
                {
                    var testyield = new TestYieldVM();
                    testyield.WhichTest = wkv.Key;
                    var pass = 0;
                    var failed = 0;
                    foreach (var fkv in wkv.Value)
                    {
                        if (string.Compare(fkv.Key, "pass", true) == 0)
                        { pass += fkv.Value; }
                        else
                        {
                            failed += fkv.Value;
                            testyield.FailureMap.Add(fkv.Key, fkv.Value);

                            if (vm.FailureMap.ContainsKey(fkv.Key))
                            { vm.FailureMap[fkv.Key] += fkv.Value; }
                            else
                            { vm.FailureMap.Add(fkv.Key, fkv.Value); }
                        }
                    }//end foreach
                    testyield.Pass = pass;
                    testyield.Failed = failed;

                    var input = pass + failed;
                    if (input > maxinput)
                    { maxinput = input; }

                    temptestyieldlist.Add(testyield);
                }//end foreach

                foreach (var testyield in temptestyieldlist)
                {
                    if ((testyield.Pass+testyield.Failed) > (maxinput/10))
                    {
                        vm.TestYieldList.Add(testyield);
                    }
                }//end foreach

                ret.Add(vm);
            }//end foreach
            return ret;
        }

        public static List<List<YieldVM>> RetrieveYieldsByDepart(string pdfamily,string familycond)
        {
            var ret = new List<List<YieldVM>>();

            //date,whichtest,failure,int
            var firstyielddict = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            var finalyielddict = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            var sql = "select YieldMonth,ProductFamily,WhichTest,Failure,FailureNum,YieldType from YieldPreData where  <familycond> and WhichTest <> '' order by YieldMonth asc";
            sql = sql.Replace("<familycond>", familycond);

            var qlist = new List<string>();
            var qdict = new Dictionary<string, bool>();

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var yieldmonth = Convert.ToDateTime(line[0]);
                var quarter = RetrieveQuarterFromDate(yieldmonth);
                if (!qdict.ContainsKey(quarter))
                {
                    qdict.Add(quarter, true);
                    qlist.Add(quarter);
                }

                var productfamily = Convert.ToString(line[1]);
                var whichtest = Convert.ToString(line[2]);
                var failure = Convert.ToString(line[3]);
                var failurenum = Convert.ToInt32(line[4]);
                var yieldtype = Convert.ToString(line[5]);

                if (string.Compare(yieldtype, YIELDTYPE.FIRSTPASSYIELD) == 0)
                {  _loadyielddict(firstyielddict, quarter, whichtest, failure, failurenum); }
                else
                { _loadyielddict(finalyielddict, quarter, whichtest, failure, failurenum); }
            }//end foreach

            ret.Add(_pumpyielddata(pdfamily,qlist,firstyielddict));
            ret.Add(_pumpyielddata(pdfamily, qlist, finalyielddict));
            return ret;
        }

        public static void RetrieveParallelYield()
        {
            var yieldlist = RetrieveYieldsByDepart("Parallel", "ProductFamily like 'Parallel%'");
            var firstpassyieldlist = yieldlist[0];
            var finalyieldlist = yieldlist[1];
        }


        public string ProductFamily { set; get; }
        public string Quarter { set; get; }
        public List<TestYieldVM> TestYieldList { set; get; }
        public Dictionary<string, int> FailureMap { set; get; }

        public double YieldVal { get {
                var ret = 1.0;
                if (TestYieldList.Count == 0)
                { return 0.0; }
                foreach (var item in TestYieldList)
                {
                    ret = ret * (item.Yield/100.0);
                }
                return ret*100.0;
            } }

    }
}