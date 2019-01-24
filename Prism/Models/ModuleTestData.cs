using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prism.Models
{
    public class ModuleTestData
    {
        public ModuleTestData()
        {
            Init();
        }

        public ModuleTestData(string dataid,string sn,string tm,string wt,string err,string station,
            string pf,string pn,string pndesc,string mt,string spd,string spt,string mes,string yf)
        {
            DataID = dataid;
            ModuleSN = sn;
            TestTimeStamp = tm;
            WhichTest = wt;
            ErrAbbr = err;
            TestStation = station;
            ProductFamily = pf;
            PN = pn;
            PNDesc = pndesc;
            ModuleType = mt;
            SpeedRate = spd;
            SpendTime = spt;
            MESTab = mes;
            YieldFamily = yf;

            SpendSec = 0;
        }

        private void Init()
        {
            DataID = "";
            ModuleSN = "";
            TestTimeStamp = "";
            WhichTest = "";
            ErrAbbr = "";
            TestStation = "";
            ProductFamily = "";
            PN = "";
            PNDesc = "";
            ModuleType = "";
            SpeedRate = "";
            SpendTime = "";
            MESTab = "";
            YieldFamily = "";
        }

        public static void StoreData(List<ModuleTestData> datalist)
        {
            var newlist = new List<object>();
            foreach (var item in datalist)
            { newlist.Add(item); }
            DBUtility.WriteDBWithTable(newlist, typeof(ModuleTestData), "ModuleTestData");
        }

        public static void CleanTestData(string yieldfamily, string mestab, DateTime startdate)
        {
            var sql = "delete from ModuleTestData where MESTab='<MESTab>' and YieldFamily = '<YieldFamily>' and TestTimeStamp >= '<startdate>' and TestTimeStamp < '<enddate>'";
            sql = sql.Replace("<MESTab>",mestab).Replace("<YieldFamily>",yieldfamily)
                .Replace("<startdate>",startdate.ToString("yyyy-MM-dd HH:mm:dd")).Replace("<enddate>", startdate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:dd"));
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<string> RetrieveAllProductFamily()
        {
            var ret = new List<string>();
            var sql = "select distinct ProductFamily from ModuleTestData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<ModuleTestData> RetrieveTestDate(string productfamily, DateTime startdate,DateTime enddate)
        {
            var ret = new List<ModuleTestData>();

            var sql = @"select DataID,ModuleSN,TestTimeStamp,WhichTest,ErrAbbr,TestStation,ProductFamily,PN,PNDesc,ModuleType,SpeedRate,SpendTime,MESTab,YieldFamily from ModuleTestData 
                            where ProductFamily = '<ProductFamily>' and TestTimeStamp >= '<startdate>' and TestTimeStamp < '<enddate>' order by ModuleSN,TestTimeStamp desc";
            sql = sql.Replace("<ProductFamily>", productfamily).Replace("<startdate>", startdate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("<enddate>", enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(new ModuleTestData(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss")
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8]), Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]), Convert.ToString(line[13])));
            }
            return ret;
        }


        public static void SendHydraWarningEmail(Controller ctrl)
        {
            var content = new List<List<string>>();
            var title = new List<string>();
            title.Add("Tester");
            title.Add("Status");
            title.Add("Start Time");
            title.Add("End Time");
            title.Add("Pending Hour");
            title.Add("Total Pending Hour");
            content.Add(title);

            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var hydratesters = syscfg["HYDRATESTER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var hydratowho = syscfg["HYDRATOWHO"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var starttime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + " 00:00:00";
            var endtime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59";
            foreach (var tester in hydratesters)
            {
                var mdata = RetrieveHydraData(tester, starttime, endtime);
                if (mdata.Count > 0)
                {
                    var pendinglist = (List<ModuleTestData>) CollectWorkingStatus(DateTime.Parse(starttime), mdata)[1];
                    if (pendinglist.Count > 0)
                    {
                        foreach (var item in pendinglist)
                        {
                            var templine = new List<string>();
                            templine.Add(tester);
                            templine.Add("Pending");
                            templine.Add(item.StartDateStr);
                            templine.Add(item.EndDateStr);
                            templine.Add(item.SpendSec.ToString());
                            templine.Add(item.TotalSpend.ToString());
                            content.Add(templine);
                        }
                    }
                }//end if
            }//end foreach

            if (content.Count == 1)
            { return; }

            var routevalue = new RouteValueDictionary();
            string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string url = ctrl.Url.Action("HydraMachineUsage", "Machine", routevalue, scheme);
            var netcomputername = EmailUtility.RetrieveCurrentMachineName();
            url = url.Replace("//localhost", "//" + netcomputername);
            var econtent = EmailUtility.CreateTableHtml("Hi Guys", "Below is the HYDRA running status:", url, content);
            EmailUtility.SendEmail(ctrl, "HYDRA RUNNING STATUS", hydratowho, econtent);
            new System.Threading.ManualResetEvent(false).WaitOne(1000);
        }

        public static List<ModuleTestData> RetrieveHydraData(string tester, string startdate, string enddate)
        {
            var ret = new List<ModuleTestData>();

            var sql = "select ProductFamily,TestStation,TestTimeStamp StartTime,SpendTime FROM [BSSupport].[dbo].[ModuleTestData] WHERE TestStation = '<tester>' and TestTimeStamp > '<startdate>' and TestTimeStamp < '<enddate>' order by TestTimeStamp asc";
            sql = sql.Replace("<tester>", tester).Replace("<startdate>", startdate).Replace("<enddate>", enddate);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new ModuleTestData();
                tempvm.ProductFamily = Convert.ToString(line[0]);
                tempvm.TestStation = Convert.ToString(line[1]);
                tempvm.StartDate = Convert.ToDateTime(line[2]);
                tempvm.SpendSec = Convert.ToInt32(line[3]);
                tempvm.EndDate = tempvm.StartDate.AddSeconds(tempvm.SpendSec);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<object> CollectWorkingStatus(DateTime startdate, List<ModuleTestData> srcdatalist)
        {
            var ret = new List<object>();

            var workinglist = new List<ModuleTestData>();
            var pendindlist = new List<ModuleTestData>();
            var halfhour = 30.0 * 60;
            var pendingtotal = 0.0;
            var workingtotal = 0.0;

            var datalist = new List<ModuleTestData>();
            var timedict = new Dictionary<string, bool>();
            foreach (var item in srcdatalist)
            {
                var tkey = item.StartDate.ToString("yyyy-MM-dd HH:mm:ss");
                if (!timedict.ContainsKey(tkey))
                {
                    timedict.Add(tkey, true);
                    datalist.Add(item);
                }
            }//end foreach

            var previousisworking = true;
            var dcnt = datalist.Count;
            for (var idx = 0; idx < dcnt; idx++)
            {
                if (idx == 0)
                {
                    if ((datalist[0].StartDate - startdate).TotalSeconds > halfhour)
                    {

                        var tempvm = new ModuleTestData();
                        tempvm.StartDate = startdate;
                        tempvm.EndDate = datalist[0].EndDate;
                        tempvm.SpendSec = Math.Round((tempvm.EndDate - tempvm.StartDate).TotalSeconds / 3600.0, 2);
                        pendingtotal += tempvm.SpendSec;
                        pendindlist.Add(tempvm);
                        previousisworking = false;
                    }
                    else
                    {
                        var tempvm = new ModuleTestData();
                        tempvm.StartDate = startdate;
                        tempvm.EndDate = datalist[0].EndDate;
                        tempvm.SpendSec = Math.Round((tempvm.EndDate - tempvm.StartDate).TotalSeconds / 3600.0, 2);
                        workingtotal += tempvm.SpendSec;
                        workinglist.Add(tempvm);
                        previousisworking = true;
                    }
                }
                else
                {
                    if ((datalist[idx].StartDate - datalist[idx - 1].EndDate).TotalSeconds > halfhour)
                    {
                        var tempvm = new ModuleTestData();
                        tempvm.StartDate = datalist[idx - 1].EndDate;
                        tempvm.EndDate = datalist[idx].StartDate;
                        tempvm.SpendSec = Math.Round((tempvm.EndDate - tempvm.StartDate).TotalSeconds / 3600.0, 2);
                        pendingtotal += tempvm.SpendSec;
                        pendindlist.Add(tempvm);
                        previousisworking = false;
                    }
                    else
                    {
                        if (previousisworking)
                        { workinglist[workinglist.Count - 1].EndDate = datalist[idx].EndDate; }
                        else
                        {
                            var tempvm = new ModuleTestData();
                            tempvm.StartDate = datalist[idx - 1].StartDate;
                            tempvm.EndDate = datalist[idx].EndDate;
                            tempvm.SpendSec = Math.Round((tempvm.EndDate - tempvm.StartDate).TotalSeconds / 3600.0, 2);
                            workingtotal += tempvm.SpendSec;
                            workinglist.Add(tempvm);
                        }
                        previousisworking = true;
                    }//end else
                }
            }

            var endoflastday = DateTime.Parse(srcdatalist[srcdatalist.Count - 1].StartDate.ToString("yyyy-MM-dd") + " 23:59:59");
            if (endoflastday < DateTime.Now
               && string.Compare(endoflastday.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) != 0
               && (endoflastday > srcdatalist[srcdatalist.Count - 1].EndDate)
                && (endoflastday - srcdatalist[srcdatalist.Count - 1].EndDate).TotalSeconds > halfhour)
            {

                var tempvm = new ModuleTestData();
                tempvm.StartDate = srcdatalist[srcdatalist.Count - 1].EndDate;
                tempvm.EndDate = endoflastday;
                tempvm.SpendSec = Math.Round((tempvm.EndDate - tempvm.StartDate).TotalSeconds / 3600.0, 2);
                pendingtotal += tempvm.SpendSec;
                pendindlist.Add(tempvm);
            }

            foreach (var item in workinglist)
            { item.TotalSpend = Math.Round(workingtotal, 2); }
            foreach (var item in pendindlist)
            { item.TotalSpend = Math.Round(pendingtotal, 2); }

            ret.Add(workinglist);
            ret.Add(pendindlist);
            return ret;
        }

        public string DataID { set; get; }
        public string ModuleSN { set; get; }
        public string TestTimeStamp { set; get; }
        public string WhichTest { set; get; }
        public string ErrAbbr { set; get; }
        public string TestStation { set; get; }
        public string ProductFamily { set; get; }

        public string PN { set; get; }
        public string PNDesc { set; get; } 
        public string ModuleType { set; get; }
        public string SpeedRate { set; get; }
        public string SpendTime { set; get; }

        public string MESTab { set; get; }
        public string YieldFamily { set; get; }


        #region HYDRA
        public DateTime StartDate { set; get; }
        public double SpendSec { set; get; }
        public DateTime EndDate { set; get; }
        public string StartDateStr { get { return StartDate.ToString("yyyy-MM-dd HH:mm:ss"); } }
        public string EndDateStr { get { return EndDate.ToString("yyyy-MM-dd HH:mm:ss"); } }
        public double TotalSpend { set; get; }
        #endregion
    }
}