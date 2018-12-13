using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class QuarterCLA
    {
        public static List<DateTime> RetrieveDateFromQuarter(string quarter)
        {
            var ret = new List<DateTime>();

            var splitstr = quarter.Split(new string[] { " ","-" }, StringSplitOptions.RemoveEmptyEntries);
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
                ret.Add(DateTime.Parse((year - 1).ToString() + "-11-01 00:00:00"));
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
                return year.ToString() + " " + "Q3";
            }
        }

        public static double QuarterSec
        {
            get { return 13.0 * 6.5 * 20.0 * 3600.0; }
        }

        public static double QuarterSecMax
        {
            get { return 13.0 * 7.0 * 24.0 * 3600.0; }
        }
    }

    public class ProductYield
    {
        public ProductYield()
        {
            ProductFamily = "";
            FirstYieldList = new List<YieldVM>();
            FinalYieldList = new List<YieldVM>();
            ProjectKey = "";
            FPYTG = 0.0;
            FYTG = 0.0;
        }

        public string ProductFamily { set; get; }
        public List<YieldVM> FirstYieldList { set; get; }
        public List<YieldVM> FinalYieldList { set; get; }
        public string ProjectKey { set; get; }
        public double FPYTG { set; get; }
        public double FYTG { set; get; }
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

                return Math.Round((double)Pass / (double)totle * 100.0,4);
                }
        }
    }

    public class YieldVM
    {

        public YieldVM()
        {
            ProductFamily = "";
            Quarter = "";
            TestYieldList = new List<TestYieldVM>();
            FailureMap = new Dictionary<string, int>();
            MaxInput = 0;
        }

        private static void _load2yielddict(Dictionary<string, Dictionary<string, Dictionary<string, int>>> firstyielddict
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
            var tunableignorelist = CfgUtility.LoadTunableIgnoreConfig(ctrl).Keys.ToList();

            var quarteryieldlist = new List<YieldVM>();
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

                    if (input > 50)
                    {
                        temptestyieldlist.Add(testyield);
                    }

                }//end foreach

                //load test yield list into linecardyield dict
                if (pdfamily.ToUpper().Contains("LINECARD"))
                {
                    
                    var linecarddict = new Dictionary<string, bool>();
                    var linecardyielddict = new Dictionary<string, TestYieldVM>();

                    foreach (var testyield in temptestyieldlist)
                    {
                        if ((testyield.Pass + testyield.Failed) > (maxinput / 5)
                            && testyield.Yield > yieldlowbound)
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
                        }//end if
                    }//end foreach

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
                        if ((ltestyield.Failed + ltestyield.Pass) > vm.MaxInput)
                        { vm.MaxInput = (ltestyield.Failed + ltestyield.Pass); }
                    }
                }
                else
                {
                    

                    foreach (var testyield in temptestyieldlist)
                    {
                        if ((testyield.Pass+testyield.Failed) > (maxinput / 5)
                            && testyield.Yield > yieldlowbound)
                        {
                            if (pdfamily.ToUpper().Contains("PARALLEL"))
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
                                if ((testyield.Failed + testyield.Pass) > vm.MaxInput)
                                { vm.MaxInput = (testyield.Failed + testyield.Pass); }
                            }
                            else
                            {
                                var ignmatch = false;
                                var testname = System.Text.RegularExpressions.Regex.Replace(testyield.WhichTest, @"\d", "").Replace("_", "")
                                    .Replace("-", "").Replace("[", "").Replace("]", "").Replace("%", "").ToUpper();
                                foreach (var ign in tunableignorelist)
                                {
                                    if (testname.Contains(ign))
                                    {
                                        ignmatch = true;
                                        break;
                                    }
                                }
                                if (ignmatch)
                                { continue; }

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
                                if ((testyield.Failed + testyield.Pass) > vm.MaxInput)
                                { vm.MaxInput = (testyield.Failed + testyield.Pass); }
                            }
                       }
                    }//end foreach
                }

                quarteryieldlist.Add(vm);
            }//end foreach
            return quarteryieldlist;
        }

        public static List<List<YieldVM>> RetrieveYieldsByProductFamily(string pdfamily,string familycond, Controller ctrl)
        {
            var ret = new List<List<YieldVM>>();

            //date,whichtest,failure,int
            var firstyielddict = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            var finalyielddict = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            var sql = "select YieldMonth,ProductFamily,WhichTest,Failure,FailureNum,YieldType from YieldPreData where  (<familycond>) and WhichTest <> '' order by YieldMonth asc";
            sql = sql.Replace("<familycond>", familycond);

            var qlist = new List<string>();
            var qdict = new Dictionary<string, bool>();

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var yieldmonth = Convert.ToDateTime(line[0]);
                var quarter = QuarterCLA.RetrieveQuarterFromDate(yieldmonth);
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
                {  _load2yielddict(firstyielddict, quarter, whichtest, failure, failurenum); }
                else
                { _load2yielddict(finalyielddict, quarter, whichtest, failure, failurenum); }
            }//end foreach

            ret.Add(_pumpyielddata(pdfamily,qlist,firstyielddict,10,ctrl));
            ret.Add(_pumpyielddata(pdfamily, qlist, finalyielddict,10,ctrl));
            return ret;
        }

        public static ProductYield RetrievePDFamilyYield(string yf, Dictionary<string, string> yieldcfg, Controller ctrl)
        {
            var pdfamily = yieldcfg[yf + "_FAMILY"];
            var sb = new System.Text.StringBuilder(1024 * 50);
            var pdfms = pdfamily.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pdf in pdfms)
            {
                sb.Append(" or ProductFamily like '%" + pdf + "%' ");
            }
            var familycond = sb.ToString().Substring(3);

            if (string.Compare(yf, "parallel", true) == 0)
            {
                familycond = " ProductFamily like 'PARALLEL%' and ProductFamily not like 'PARALLEL.SFPWIRE%' ";
            }

            var yieldobj = RetrieveYieldsByProductFamily(yf, familycond, ctrl);
            var pdyield = new ProductYield();
            pdyield.ProductFamily = yf;
            pdyield.FirstYieldList.AddRange(yieldobj[0]);
            pdyield.FinalYieldList.AddRange(yieldobj[1]);
            return pdyield;
        }

        public static List<ProductYield> RetrieveAllYield(Controller ctrl)
        {
            var ret = new List<ProductYield>();
            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);

            var yieldfamilys = yieldcfg["YIELDFAMILY"].Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var yf in yieldfamilys)
            {
                var pdyield = RetrievePDFamilyYield(yf, yieldcfg, ctrl);
                ret.Add(pdyield);
            }//end foreach
            return ret;
        }

        public static List<ProductYield> RetrieveProductYieldByYF(string yf, Controller ctrl)
        {
            var ret = new List<ProductYield>();

            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);

            var sb = new System.Text.StringBuilder(1024 * 50);

            if (yieldcfg.ContainsKey(yf + "_FAMILY"))
            {
                var pdfamily = yieldcfg[yf + "_FAMILY"];
                var pdfms = pdfamily.Split(new string[] { ",",";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pdf in pdfms)
                {
                    sb.Append(" or ProductFamily like '%" + pdf + "%' ");
                }
            }
            else
            {
                var pdfms = yf.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pdf in pdfms)
                {
                    sb.Append(" or ProductFamily like '%" + pdf + "%' ");
                }
            }
            var familycond = sb.ToString().Substring(3);


            var familylist = new List<string>();
            var familydict = new Dictionary<string, bool>();
            var sql = "select distinct ProductFamily,YieldMonth from YieldPreData where (<familycond>) order by YieldMonth desc";
            sql = sql.Replace("<familycond>", familycond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pd = Convert.ToString(line[0]);
                if (!familydict.ContainsKey(pd))
                {
                    familydict.Add(pd, true);
                    familylist.Add(pd);
                }
            }

            foreach (var pd in familylist)
            {
                var yieldobj = RetrieveYieldsByProductFamily(pd, " ProductFamily like '" + pd + "' ", ctrl);
                var pdyield = new ProductYield();
                pdyield.ProductFamily = pd;
                pdyield.FirstYieldList.AddRange(yieldobj[0]);
                pdyield.FinalYieldList.AddRange(yieldobj[1]);
                pdyield.ProjectKey = YieldPreData.Prod2PJKey(pd);

                pdyield.FPYTG = 135;
                if (yieldcfg.ContainsKey(pd + "-FPY"))
                { pdyield.FPYTG = Convert.ToDouble(yieldcfg[pd + "-FPY"]); }
                pdyield.FYTG = 135;
                if (yieldcfg.ContainsKey(pd + "-FY"))
                { pdyield.FYTG = Convert.ToDouble(yieldcfg[pd + "-FY"]); }

                ret.Add(pdyield);
            }

            return ret;
        }

        public static List<string> RetrieveAllProductList()
        {
            var familylist = new List<string>();
            var sql = "select distinct ProductFamily from YieldPreData  order by ProductFamily";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pd = Convert.ToString(line[0]);
                familylist.Add(pd);
            }
            return familylist;
        }

        public static List<ProductYield> RetrieveProductYield(List<string> familylist, Controller ctrl)
        {
            var ret = new List<ProductYield>();
            foreach (var pd in familylist)
            {
                var yieldobj = RetrieveYieldsByProductFamily(pd, " ProductFamily like '" + pd + "' ", ctrl);
                var pdyield = new ProductYield();
                pdyield.ProductFamily = pd;
                pdyield.FirstYieldList.AddRange(yieldobj[0]);
                pdyield.FinalYieldList.AddRange(yieldobj[1]);
                ret.Add(pdyield);
            }
            return ret;
        }

        public static Dictionary<string, Dictionary<string, int>> RetrieveBurnInData(string testtype)
        {
            var ret = new Dictionary<string, Dictionary<string, int>>();

            var sql = "select StartDate,Failure,Num FROM [NPITrace].[dbo].[VcselMonthData] where VTYPE like <testtype> order by StartDate asc";
            sql = sql.Replace("<testtype>", testtype);
            var dbret = DBUtility.ExeNPISqlWithRes(sql);
            foreach (var line in dbret)
            {
                var date = Convert.ToDateTime(line[0]);
                var q = QuarterCLA.RetrieveQuarterFromDate(date);
                var failure = Convert.ToString(line[1]);
                var num = Convert.ToInt32(line[2]);

                if (ret.ContainsKey(q))
                {
                    var fdict = ret[q];
                    if (string.Compare(failure, "pass", true) == 0)
                    { fdict["PASS"] += num; }
                    else
                    { fdict["FAILED"] += num; }
                }
                else
                {
                    var fdict = new Dictionary<string, int>();
                    fdict.Add("PASS", 1);
                    fdict.Add("FAILED", 1);
                    if (string.Compare(failure, "pass", true) == 0)
                    { fdict["PASS"] += num; }
                    else
                    { fdict["FAILED"] += num; }
                    ret.Add(q, fdict);
                }
            }

            return ret;
        }

        private static List<string> RetrieveQuarterFromYield(List<ProductYield> pdys)
        {
            var quarterdict = new Dictionary<string, bool>();
            foreach (var pdy in pdys)
            {
                foreach (var y in pdy.FinalYieldList)
                {
                    if (!quarterdict.ContainsKey(y.Quarter))
                    {
                        quarterdict.Add(y.Quarter, true);
                    }
                }
            }
            var qlist = quarterdict.Keys.ToList();
            qlist.Sort(delegate (string q1, string q2)
            {
                var qd1 = QuarterCLA.RetrieveDateFromQuarter(q1);
                var qd2 = QuarterCLA.RetrieveDateFromQuarter(q2);
                return qd1[0].CompareTo(qd2[0]);
            });
            return qlist;
        }

        public static object GetYieldTable(List<ProductYield> pdyieldlist, string tabtitle, bool fordepartment)
        {
            var titlelist = new List<object>();
            titlelist.Add(tabtitle);
            titlelist.Add("");

            var quarterlist = RetrieveQuarterFromYield(pdyieldlist);
            titlelist.AddRange(quarterlist);


            var pdy = pdyieldlist[0];

                var linelist = new List<object>();

                if (fordepartment)
                {
                    linelist.Add("<a href='/Yield/DepartmentYield' target='_blank'>" + pdy.ProductFamily + "</a>");
                }
                else
                {
                    linelist.Add("<a href='/Yield/ProductYield?productfaimly=" + HttpUtility.UrlEncode(pdy.ProductFamily) + "' target='_blank'>" + pdy.ProductFamily + "</a>");
                }

                linelist.Add("<span class='YINPUT'>INPUT</span><br><span class='YFPY'>FPY</span><br><span class='YFY'>FY</span>");

                foreach (var qt in quarterlist)
                {
                    var matchidx = 0;
                    var matchflag = false;
                    foreach (var fy in pdy.FinalYieldList)
                    {
                        if (string.Compare(qt, fy.Quarter, true) == 0)
                        {
                            matchflag = true;
                            break;
                        }
                        matchidx += 1;
                    }

                    if (matchflag && pdy.FirstYieldList[matchidx].MaxInput > 0)
                    {
                        linelist.Add("<span class='YINPUT'>" + String.Format("{0:n0}", pdy.FirstYieldList[matchidx].MaxInput) + "</span><br><span class='YFPY'>" + pdy.FirstYieldList[matchidx].YieldVal + "%</span><br><span class='YFY'>" + pdy.FinalYieldList[matchidx].YieldVal + "%</span>");
                    }
                    else
                    {
                        linelist.Add(" ");
                    }
                }//end foreach

                return new
                {
                    tabletitle = titlelist,
                    tablecontent = linelist
                };
        }


        public string ProductFamily { set; get; }
        public string Quarter { set; get; }
        public List<TestYieldVM> TestYieldList { set; get; }
        public Dictionary<string, int> FailureMap { set; get; }
        public int MaxInput { set; get; }

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