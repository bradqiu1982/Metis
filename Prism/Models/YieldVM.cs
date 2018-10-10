﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class ProductYield
    {
        public ProductYield()
        {
            ProductFamily = "";
            FirstYieldList = new List<YieldVM>();
            FinalYieldList = new List<YieldVM>();
        }

        public string ProductFamily { set; get; }
        public List<YieldVM> FirstYieldList { set; get; }
        public List<YieldVM> FinalYieldList { set; get; }
    }

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

        private static List<YieldVM> _pumpyielddata(string pdfamily,List<string> quarterlist
            , Dictionary<string, Dictionary<string, Dictionary<string, int>>> firstyielddict,double yieldlowbound, Controller ctrl)
        {
            var linecardignorelist = CfgUtility.LoadLineCardIgnoreConfig(ctrl).Keys.ToList();

            var ret = new List<YieldVM>();
            foreach (var q in quarterlist)
            {
                var vm = new YieldVM();
                vm.ProductFamily = pdfamily;
                vm.Quarter = q;
                var wdict = firstyielddict[q];
                var maxinput = 0;

                //retrieve test yield list of each quarter
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
                        }
                    }//end foreach
                    testyield.Pass = pass;
                    testyield.Failed = failed;

                    var input = pass + failed;
                    if (input > maxinput)
                    { maxinput = input; }

                    temptestyieldlist.Add(testyield);
                }//end foreach

                var linecarddict = new Dictionary<string, bool>();
                var linecardyielddict = new Dictionary<string, TestYieldVM>();

                foreach (var testyield in temptestyieldlist)
                {
                    if ((testyield.Pass+testyield.Failed) > (maxinput/10)
                        && testyield.Yield > yieldlowbound)
                    {
                        //load test yield list into linecardyield dict
                        if (pdfamily.Contains("LINECARD"))
                        {
                            if (testyield.Yield == 100 && linecardyielddict.Count > 0)
                            { continue; }

                            var ignmatch = false;
                            var testname = System.Text.RegularExpressions.Regex.Replace(testyield.WhichTest, @"\d", "").Replace("_", "").ToUpper();
                            foreach (var ign in linecardignorelist)
                            {
                                if (testname.Contains(ign))
                                {
                                    ignmatch = true;
                                    break;
                                }
                            }
                            if (ignmatch)
                            { continue; }

                            var id = System.Text.RegularExpressions.Regex.Replace(testyield.WhichTest, @"\d", "") + "_" + (testyield.Pass + testyield.Failed).ToString();
                            if (linecarddict.ContainsKey(id))
                            {
                                if (testyield.Yield < linecardyielddict[id].Yield)
                                {
                                    linecardyielddict.Remove(id);
                                    linecardyielddict.Add(id, testyield);
                                }
                            }
                            else
                            {
                                linecarddict.Add(id, true);
                                linecardyielddict.Add(id, testyield);
                            }
                        }
                        else
                        {
                            //load test yield list into yieldvm
                            foreach (var fkv in testyield.FailureMap)
                            {
                                if (string.Compare(fkv.Key, "pass", true) != 0)
                                {
                                    if (vm.FailureMap.ContainsKey(fkv.Key))
                                    { vm.FailureMap[fkv.Key] += fkv.Value; }
                                    else
                                    { vm.FailureMap.Add(fkv.Key, fkv.Value); }
                                }
                            }
                            vm.TestYieldList.Add(testyield);
                        }
                    }
                }//end foreach

                //load linecard yield dict into yieldvm
                if (pdfamily.Contains("LINECARD"))
                {
                    var lincardyieldlist = linecardyielddict.Values.ToList();
                    foreach (var ltestyield in lincardyieldlist)
                    {
                        foreach (var fkv in ltestyield.FailureMap)
                        {
                            if (string.Compare(fkv.Key, "pass", true) != 0)
                            {
                                if (vm.FailureMap.ContainsKey(fkv.Key))
                                { vm.FailureMap[fkv.Key] += fkv.Value; }
                                else
                                { vm.FailureMap.Add(fkv.Key, fkv.Value); }
                            }
                        }
                        vm.TestYieldList.Add(ltestyield);
                    }
                }

                ret.Add(vm);
            }//end foreach
            return ret;
        }

        public static List<List<YieldVM>> RetrieveYieldsByProductFamily(string pdfamily,string familycond, Controller ctrl)
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

            ret.Add(_pumpyielddata(pdfamily,qlist,firstyielddict,10,ctrl));
            ret.Add(_pumpyielddata(pdfamily, qlist, finalyielddict,10,ctrl));
            return ret;
        }

        public static List<ProductYield> RetrieveAllYield(Controller ctrl)
        {
            var ret = new List<ProductYield>();

            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var yieldfamilys = yieldcfg["YIELDFAMILY"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var yf in yieldfamilys)
            {
                var pdfamily = yieldcfg[yf + "_FAMILY"];
                var sb = new System.Text.StringBuilder(1024 * 50);
                var pdfms = pdfamily.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pdf in pdfms)
                {
                    sb.Append(" or ProductFamily like '" + pdf + "%' ");
                }
                var familycond = sb.ToString().Substring(3);
                var yieldobj = RetrieveYieldsByProductFamily(yf, familycond,ctrl);
                var pdyield = new ProductYield();
                pdyield.ProductFamily = yf;
                pdyield.FirstYieldList.AddRange(yieldobj[0]);
                pdyield.FinalYieldList.AddRange(yieldobj[1]);
                ret.Add(pdyield);
            }//end foreach
            return ret;
        }

        public static void RetrieveParallelYield()
        {
            //var yieldlist = RetrieveYieldsByProductFamily("Parallel", "ProductFamily like 'Parallel%'");
            //var firstpassyieldlist = yieldlist[0];
            //var finalyieldlist = yieldlist[1];
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
                return Math.Round(ret*100.0,2);
            } }

    }
}