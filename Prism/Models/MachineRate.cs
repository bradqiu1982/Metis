using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class TestMachineRate
    {
        public TestMachineRate()
        {
            WhichTest = "";
            MachineList = new List<string>();
            SpendTime = 0;
        }

        public string WhichTest { set; get; }
        public List<string> MachineList { set; get; }
        public int SpendTime { set; get; }

        public int Hours
        {
            get
            {
                return (int)(SpendTime / 3600);
            }
        }

        public double Machines
        { get { return MachineList.Count; } }

        public double Rate {
            get {
                if (MachineList.Count == 0)
                { return 0.0; }
                return Math.Round((double)SpendTime / (Machines * QuarterCLA.QuarterSec) * 100.0,2);
            }
        }
    }

    public class TestSpendTime
    {
        public TestSpendTime()
        {
            whichTest = "";
            SpendTime = 0;
        }

        public string whichTest { set; get; }
        public int SpendTime { set; get; }
    }

    public class MachineSpendTime
    {
        public MachineSpendTime()
        {
            Machine = "";
            TestList = new List<TestSpendTime>();
        }

        public string Machine { set; get; }
        public List<TestSpendTime> TestList { set; get; }
        public int SpendTime { get {
                var val = 0;
                foreach (var t in TestList)
                { val += t.SpendTime; }
                return val;
            } }

        public int Hours { get {
                return (int)(SpendTime/3600);
            } }

        public double Rate
        {
            get
            {
                if (TestList.Count == 0)
                { return 0.0; }
                return Math.Round(SpendTime / QuarterCLA.QuarterSec * 100.0, 2);
            }
        }

        public string MainTest {
            get {
                if (TestList.Count == 0)
                { return string.Empty; }
                TestList.Sort(delegate (TestSpendTime obj1, TestSpendTime obj2) { return obj2.SpendTime.CompareTo(obj1.SpendTime); });
                return TestList[0].whichTest;
            }
        }

    }

    public class MachineRate
    {
        private static void _load2machinedict(Dictionary<string, Dictionary<string, Dictionary<string, int>>> qmdict,string quarter,
            string machine,string whichtest,int spendtime)
        {
                if (qmdict.ContainsKey(quarter))
                {
                    var mdict = qmdict[quarter];
                    if (mdict.ContainsKey(machine))
                    {
                        var wdict = mdict[machine];
                        if (wdict.ContainsKey(whichtest))
                        { wdict[whichtest] += spendtime; }
                        else
                        { wdict.Add(whichtest, spendtime); }
                    }
                    else
                    {
                        var wdict = new Dictionary<string, int>();
                        wdict.Add(whichtest, spendtime);
                        mdict.Add(machine, wdict);
                    }
                }
                else
                {
                    var wdict = new Dictionary<string, int>();
                    wdict.Add(whichtest, spendtime);
                    var mdict = new Dictionary<string, Dictionary<string, int>>();
                    mdict.Add(machine, wdict);
                    qmdict.Add(quarter, mdict);
                }
        }

        private static List<MachineRate> _pumpmachinedata(string pdfamily, List<string> quarterlist,
            Dictionary<string, Dictionary<string, Dictionary<string, int>>> qmdict,double lowbound, Controller ctrl)
        {
            var ignoretestlist = CfgUtility.LoadYieldConfig(ctrl)["MACHINEIGNORETEST"].Split(new string[]{ ";" },StringSplitOptions.RemoveEmptyEntries).ToList();

            var ret = new List<MachineRate>();
            foreach (var q in quarterlist)
            {
                var vm = new MachineRate();
                vm.ProductFamily = pdfamily;
                vm.Quarter = q;

                var machinedict = qmdict[q];
                foreach (var mkv in machinedict)
                {
                    var mvm = new MachineSpendTime();
                    mvm.Machine = mkv.Key;
                    foreach (var wkv in mkv.Value)
                    {
                        var tvm = new TestSpendTime();
                        tvm.whichTest = wkv.Key;
                        tvm.SpendTime = wkv.Value;
                        mvm.TestList.Add(tvm);
                    }//end foreach

                    if (mvm.SpendTime > lowbound && mvm.SpendTime < QuarterCLA.QuarterSecMax)
                    {
                        var matched = false;
                        foreach (var t in ignoretestlist)
                        {
                            if (string.Compare(mvm.MainTest, t,true) == 0)
                            {
                                matched = true;
                                break;
                            }
                        }

                        if (matched)
                        { continue; }
                                 
                        vm.MachineTimeList.Add(mvm);
                    }//end if

                }//end machine
                if (vm.MachineTimeList.Count > 0)
                {
                    vm.ProjectKey = YieldPreData.Prod2PJKey(pdfamily);
                    ret.Add(vm);
                }
            }//end foreach

            return ret;
        }

        
        private static List<MachineRate> RetrievePdMachine(string pdfamily, string familycond,Controller ctrl)
        {
            var ret = new List<MachineRate>();

            var sql = "select MachineMonth, ProductFamily,Machine, WhichTest,SpendTime  from MachinePreData where  (<familycond>) and WhichTest <> '' order by MachineMonth asc";
            sql = sql.Replace("<familycond>", familycond);

            var qlist = new List<string>();
            var qdict = new Dictionary<string, bool>();
            //quarter,machine,test
            var qmdict = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var machinemonth = Convert.ToDateTime(line[0]);
                var quarter = QuarterCLA.RetrieveQuarterFromDate(machinemonth);
                if (!qdict.ContainsKey(quarter))
                {
                    qdict.Add(quarter, true);
                    qlist.Add(quarter);
                }
                var productfamily = Convert.ToString(line[1]);
                var machine = Convert.ToString(line[2]);
                var whichtest = Convert.ToString(line[3]);
                var spendtime = Convert.ToInt32(line[4]);

                _load2machinedict(qmdict, quarter, machine, whichtest, spendtime);
            }//end foreach

            ret.AddRange(_pumpmachinedata(pdfamily,qlist,qmdict,0.05* QuarterCLA.QuarterSec,ctrl));

            return ret;
        }

        public static List<List<MachineRate>> RetrieveAllMachine(Controller ctrl)
        {
            var ret = new List<List<MachineRate>>();

            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var yieldfamilys = yieldcfg["YIELDFAMILY"].Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var yf in yieldfamilys)
            {
                var pdfamily = yieldcfg[yf + "_FAMILY"];
                var sb = new System.Text.StringBuilder(1024 * 50);
                var pdfms = pdfamily.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pdf in pdfms)
                {
                    sb.Append(" or ProductFamily like '%" + pdf + "%' ");
                }
                var familycond = sb.ToString().Substring(3);
                var machineratelist = RetrievePdMachine(yf, familycond, ctrl);
                if (machineratelist.Count > 0)
                {
                    ret.Add(machineratelist);
                }
            }
            return ret;
        }

        public static List<List<MachineRate>> RetrieveAllMachineByYF(string yf,Controller ctrl)
        {
            var ret = new List<List<MachineRate>>();

            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);

                var pdfamily = yieldcfg[yf + "_FAMILY"];
                var sb = new System.Text.StringBuilder(1024 * 50);
                var pdfms = pdfamily.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pdf in pdfms)
                {
                    sb.Append(" or ProductFamily like '%" + pdf + "%' ");
                }
                var familycond = sb.ToString().Substring(3);
                var machineratelist = RetrievePdMachine(yf, familycond, ctrl);
                if (machineratelist.Count > 0)
                {
                    ret.Add(machineratelist);
                }

            return ret;
        }

        public static List<List<MachineRate>> RetrieveProductMachineByYF(string yf, Controller ctrl)
        {
            var ret = new List<List<MachineRate>>();

            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var sb = new System.Text.StringBuilder(1024 * 50);

            if (yieldcfg.ContainsKey(yf + "_FAMILY"))
            {
                var pdfamily = yieldcfg[yf + "_FAMILY"];
                var pdfms = pdfamily.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
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
                    sb.Append(" or ProductFamily like '" + pdf + "' ");
                }
            }
            var familycond = sb.ToString().Substring(3);


            var familylist = new List<string>();
            var familydict = new Dictionary<string, bool>();
            var sql = "select distinct ProductFamily,MachineMonth from MachinePreData where (<familycond>) order by MachineMonth desc";
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
                var machineratelist = RetrievePdMachine(pd, " ProductFamily like '" + pd + "' ", ctrl);
                if (machineratelist.Count > 0)
                {
                    ret.Add(machineratelist);
                }
            }

            return ret;
        }

        public static List<TestMachineRate> GetTestMachineRate(List<MachineSpendTime> MachineTimeList)
        {
            var ret = new List<TestMachineRate>();

            var wdict = new Dictionary<string, List<MachineSpendTime>>();
            foreach (var m in MachineTimeList)
            {
                if (wdict.ContainsKey(m.MainTest))
                {
                    wdict[m.MainTest].Add(m);
                }
                else
                {
                    var mlist = new List<MachineSpendTime>();
                    mlist.Add(m);
                    wdict.Add(m.MainTest, mlist);
                }
            }

            foreach (var w in wdict)
            {
                var vm = new TestMachineRate();
                vm.WhichTest = w.Key;
                var spendtime = 0;
                foreach (var m in w.Value)
                {
                    spendtime += m.SpendTime;
                    vm.MachineList.Add(m.Machine);
                }
                vm.SpendTime = spendtime;
                ret.Add(vm);
            }

            ret.Sort(delegate (TestMachineRate obj1, TestMachineRate obj2) { return obj1.Rate.CompareTo(obj2.Rate); });
            return ret;
        }

        private static List<string> RetrieveQuarterFromYield(List<List<MachineRate>> pdms)
        {
            var quarterdict = new Dictionary<string, bool>();
            foreach (var pdm in pdms)
            {
                foreach (var y in pdm)
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

        public static  object GetMachineTable(List<List<MachineRate>> pdmachinelist, string title, bool withlink = false)
        {
            var titlelist = new List<object>();
            titlelist.Add(title);
            titlelist.Add("");

            var quarterlist = RetrieveQuarterFromYield(pdmachinelist);
            titlelist.AddRange(quarterlist);


            var idx = 0;
            var linelist = new List<object>();
            foreach (var pdm in pdmachinelist)
            {
                if (pdm.Count == 0)
                { continue; }

                linelist = new List<object>();
                if (withlink)
                {
                    linelist.Add("<a href='/Machine/DepartmentMachine' target='_blank'>" + pdm[0].ProductFamily + "</a>");
                }
                else
                {
                    linelist.Add("<a href='/Machine/ProductMachine?productfaimly=" + HttpUtility.UrlEncode(pdm[0].ProductFamily) + "' target='_blank'>" + pdm[0].ProductFamily + "</a>");
                }

                linelist.Add("<span class='YINPUT'>Machines</span><br><span class='YFPY'>Hours</span><br><span class='YFY'>Rate%</span>");


                foreach (var qt in quarterlist)
                {
                    var matchidx = 0;
                    var matchflag = false;
                    foreach (var mr in pdm)
                    {
                        if (string.Compare(qt, mr.Quarter, true) == 0)
                        {
                            matchflag = true;
                            break;
                        }
                        matchidx += 1;
                    }

                    if (matchflag && pdm[matchidx].MachineTimeList.Count > 0)
                    {

                        linelist.Add("<span class='YINPUT'>" + pdm[matchidx].MachineTimeList.Count + "</span><br><span class='YFPY'>" + (int)(pdm[matchidx].SpendTime / 3600) + "</span><br><span class='YFY'>" + pdm[matchidx].Rate + "</span>");
                        pdm[matchidx].MachineTimeList.Sort(delegate (MachineSpendTime obj1, MachineSpendTime obj2) { return obj1.SpendTime.CompareTo(obj2.SpendTime); });
                    }
                    else
                    {
                        linelist.Add(" ");
                    }
                }//end foreach

                 idx += 1;
            }

            return  new
            {
                tabletitle = titlelist,
                tablecontent = linelist
            };
        }

        public MachineRate()
        {
            ProductFamily = "";
            Quarter = "";
            MachineTimeList = new List<MachineSpendTime>();
            ProjectKey = "";
        }

        public string ProductFamily { set; get; }
        public string Quarter { set; get; }
        public List<MachineSpendTime> MachineTimeList { set; get; }
        public string ProjectKey { set; get; }

        public double Rate { get {
                if (MachineTimeList.Count == 0)
                { return 0.0; }
 
                var totletime = QuarterCLA.QuarterSec * MachineTimeList.Count;
                return Math.Round(SpendTime / totletime * 100.0, 2);
            } }

        public double SpendTime
        {
            get {
                if (MachineTimeList.Count == 0)
                { return 0.0; }
                var spendtime = 0.0;
                foreach (var m in MachineTimeList)
                { spendtime += m.SpendTime; }
                return spendtime;
            }
        }

    }
}