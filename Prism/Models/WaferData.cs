using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class WaferData
    {
        public static void LoadAllWaferData(Controller ctrl)
        {
            var vcselpndict = CfgUtility.LoadVcselPNConfig(ctrl);
            var waferlist = LoadWaferNum("2015-10-01 00:00:00");
            foreach (var wf in waferlist)
            {
                LoadWaferData(wf, vcselpndict);
            }

        }

        public static void LoadWaferDataIn3Month(Controller ctrl)
        {
            var vcselpndict = CfgUtility.LoadVcselPNConfig(ctrl);
            var threemonth = DateTime.Now.AddMonths(-4).ToString("yyyy-MM-dd HH:mm:ss");
            var waferlist = LoadWaferNum(threemonth);
            foreach (var wf in waferlist)
            {
                LoadWaferData(wf, vcselpndict);
            }
        }

        private static List<string> LoadWaferNum(string startdate)
        {
            var ret = new List<string>();
            var sql = @"SELECT distinct SUBSTRING(isnull(dc.[ParamValueString],''),0,10) as WaferLot from InsiteDB.insite.dc_AOC_ManualInspection dc 
	                     left join InsiteDB.insite.historyMainline hmll with (nolock) on hmll.[HistoryMainlineId]=dc.[HistoryMainlineId]  
	                     left join InsiteDB.insite.container cFrom with (nolock) on cFrom.OriginalcontainerId=hmll.historyid  
                         left join InsiteDB.insite.product p with (nolock) on  cFrom.productId = p.productId  
 	                      WHERE dc.parametername='Trace_ID'  and dc.[ParamValueString] like '%-%' and p.description like '%VCSEL%' and hmll.MfgDate > '<startdate>' ";
            sql = sql.Replace("<startdate>", startdate);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static void LoadWaferData(string Wafer, Dictionary<string, VcselPNInfo> vcselpndict)
        {
            
            var sndict = new Dictionary<string,bool>();
            var shipsndict = new Dictionary<string, WaferData>();
            var undefinesndict = new Dictionary<string, WaferData>();


            var tempvm = new WaferData();

            var sql = @"SELECT distinct c.ContainerName,pb.productname MaterialPN,hml.MfgDate,ws.WorkflowStepName
                        FROM InsiteDB.insite.container c with (nolock) 
                        left join InsiteDB.insite.currentStatus cs (nolock) on c.currentStatusId = cs.currentStatusId 
                        left join InsiteDB.insite.workflowstep ws(nolock) on  cs.WorkflowStepId = ws.WorkflowStepId 
                        left join InsiteDB.insite.componentRemoveHistory crh with (nolock) on crh.historyId = c.containerId 
                        left join InsiteDB.insite.removeHistoryDetail rhd on rhd.componentRemoveHistoryId = crh.componentRemoveHistoryId 
                        left join InsiteDB.insite.starthistorydetail  shd(nolock) on c.containerid=shd.containerId and shd.historyId <> shd.containerId 
                        left join InsiteDB.insite.container co (nolock) on co.containerid=shd.historyId 
                        left join InsiteDB.insite.historyMainline hml with (nolock) on c.containerId = hml.containerId 
                        left join InsiteDB.insite.componentIssueHistory cih with (nolock) on  hml.historyMainlineId=cih.historyMainlineId 
                        left join InsiteDB.insite.issueHistoryDetail ihd with (nolock) on cih.componentIssueHistoryId = ihd.componentIssueHistoryId 
                        left join InsiteDB.insite.issueActualsHistory iah with (nolock) on  ihd.issueHistoryDetailId = iah.issueHistoryDetailId 
                        left join InsiteDB.insite.RemoveHistoryDetail rem with (nolock) on iah.IssueActualsHistoryId = rem.IssueActualsHistoryId 
                        left join InsiteDB.insite.RemovalReason re with (nolock) on rem.RemovalReasonId = re.RemovalReasonId 
                        left join InsiteDB.insite.container cFrom with (nolock) on iah.fromContainerId = cFrom.containerId 
                        left join InsiteDB.insite.product p with (nolock) on  cFrom.productId = p.productId 
                        left join InsiteDB.insite.productBase pb with (nolock) on p.productBaseId  = pb.productBaseId 
                        left join InsiteDB.insite.historyMainline hmll with (nolock)on cFrom.OriginalcontainerId=hmll.historyid 
                        left join InsiteDB.insite.product pp with (nolock) on c.productid=pp.productid 
                        left join InsiteDB.insite.productfamily pf (nolock) on  pp.productFamilyId = pf.productFamilyId 
                        left join InsiteDB.insite.productbase pbb with (nolock) on pp.productbaseid=pbb.productbaseid 
                        left join InsiteDB.insite.dc_AOC_ManualInspection dc (nolock) on hmll.[HistoryMainlineId]=dc.[HistoryMainlineId] 
                        WHERE dc.parametername='Trace_ID' and p.description like '%VCSEL%' and dc.[ParamValueString] like '%<wafer>%' 
		                 and Len(c.ContainerName) = 7  order by hml.MfgDate asc";
            sql = sql.Replace("<wafer>", Wafer);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                if (sndict.Count == 0)
                {
                    tempvm.WaferNum = Wafer;
                    tempvm.WaferPN = Convert.ToString(line[1]);
                    tempvm.BuildDate = Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                }

                
                var sn = Convert.ToString(line[0]);
                var workstepname = Convert.ToString(line[3]).ToUpper();
                if (!sndict.ContainsKey(sn))
                {
                    sndict.Add(sn, true);
                    var builddate = Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                    if (workstepname.Contains("SHIPPING")
                        || workstepname.Contains("MAIN STORE")
                        || workstepname.Contains("MAINSTORE")
                        || workstepname.Contains("MAIN_STORE"))
                    {
                        var tempval = new WaferData();
                        tempval.BuildDate = builddate;
                        tempval.SNWorkFlowName = workstepname;
                        shipsndict.Add(sn, tempval);
                    }
                    else
                    {
                        var tempval = new WaferData();
                        tempval.BuildDate = builddate;
                        tempval.SNWorkFlowName = workstepname;
                        undefinesndict.Add(sn, tempval);
                    }
                }
            }

            if (undefinesndict.Count > 0)
            {
                var combinsndict = new Dictionary<string, List<string>>();
                var sncond = "('" + string.Join("','", undefinesndict.Keys.ToList()) + "')";
                sql = "select ToContainer,FromContainer FROM [PDMS].[dbo].[ComponentIssueSummary] where FromContainer in <sncond> and ToContainer is not null and FromContainer is not null order by IssueDate desc";
                sql = sql.Replace("<sncond>", sncond);
                dbret = DBUtility.ExeMESReportMasterSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var csn = Convert.ToString(line[0]);
                    var sn = Convert.ToString(line[1]);
                    if (!combinsndict.ContainsKey(csn))
                    {
                        var templist = new List<string>();
                        templist.Add(sn);
                        combinsndict.Add(csn, templist);
                    }
                    else
                    {
                        combinsndict[csn].Add(sn);
                    }
                }

                if (combinsndict.Count > 0)
                {
                    var templist = new List<string>();
                    templist.AddRange(undefinesndict.Keys.ToList());
                    templist.AddRange(combinsndict.Keys.ToList());
                    sncond = "('" + string.Join("','", templist) + "')";
                }

                sql = @"select  distinct c.ContainerName FROM InsiteDB.insite.container c 
		                left join InsiteDB.insite.historyMainline hml with (nolock) on c.containerId = hml.containerId 
		                left join InsiteDB.insite.workflowstep ws(nolock) on  hml.WorkflowStepId = ws.WorkflowStepId 
		                 where c.ContainerName in <sncond> and ws.WorkflowStepName = 'Shipping'";
                sql = sql.Replace("<sncond>", sncond);
                dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var sn = Convert.ToString(line[0]);
                    if (!shipsndict.ContainsKey(sn))
                    {
                        if (undefinesndict.ContainsKey(sn))
                        {
                            undefinesndict[sn].SNWorkFlowName = "SHIPPING";
                            shipsndict.Add(sn, undefinesndict[sn]);
                        }
                        else
                        {
                            if (combinsndict.ContainsKey(sn))
                            {
                                var tempsnlist = combinsndict[sn];
                                foreach (var tempsn in tempsnlist)
                                {
                                    if (!undefinesndict.ContainsKey(tempsn))
                                    { continue; }

                                    undefinesndict[tempsn].SNWorkFlowName = "SHIPPING";
                                    if (!shipsndict.ContainsKey(tempsn))
                                    {
                                        shipsndict.Add(tempsn, undefinesndict[tempsn]);
                                    }
                                }
                            }
                        }//end else
                    }//end if
                }//end foreach


                sncond = "('" + string.Join("','", undefinesndict.Keys.ToList()) + "')";
                sql = @"select distinct  c.ContainerName FROM InsiteDB.insite.container c 
                     left join InsiteDB.insite.container co on co.ContainerId = c.IssuedToContainerId 
                     left join InsiteDB.insite.historyMainline nhml with (nolock) on co.containerId = nhml.containerId 
                     left join InsiteDB.insite.workflowstep nws(nolock) on  nhml.WorkflowStepId = nws.WorkflowStepId 
                     where c.ContainerName in  <sncond>  and  nws.WorkflowStepName  = 'Shipping'";

                sql = sql.Replace("<sncond>", sncond);
                dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var sn = Convert.ToString(line[0]);
                    if (!shipsndict.ContainsKey(sn))
                    {
                        undefinesndict[sn].SNWorkFlowName = "SHIPPING";
                        shipsndict.Add(sn, undefinesndict[sn]);
                    }
                }

            }

            if (shipsndict.Count > 0 && vcselpndict.ContainsKey(tempvm.WaferPN))
            {
                tempvm.WaferCount = shipsndict.Count;
                tempvm.WaferArray = vcselpndict[tempvm.WaferPN].varray;
                tempvm.Rate = vcselpndict[tempvm.WaferPN].vrate;
                tempvm.WaferTech = vcselpndict[tempvm.WaferPN].vtech;

                CleanWaferData(Wafer);

                tempvm.StoreData();
                foreach (var kv in shipsndict)
                {
                    StoreWafeSNMap(Wafer, kv.Key, kv.Value.BuildDate,kv.Value.SNWorkFlowName);
                }

                foreach (var kv in undefinesndict)
                {
                    if (!shipsndict.ContainsKey(kv.Key))
                    {
                        StoreWafeSNMap(Wafer, kv.Key, kv.Value.BuildDate, kv.Value.SNWorkFlowName);
                    }
                }
            }
        }

        private static void CleanWaferData(string wafer)
        {
            var sql = "delete from WaferData where WaferNum = '<WaferNum>'";
            sql = sql.Replace("<WaferNum>", wafer);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "delete from WaferSNMap where WaferNum = '<WaferNum>'";
            sql = sql.Replace("<WaferNum>", wafer);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreData()
        {
            var sql = "insert into WaferData(WaferNum,WaferPN,Rate,WaferArray,WaferTech,WaferCount,BuildDate) values(@WaferNum,@WaferPN,@Rate,@WaferArray,@WaferTech,@WaferCount,@BuildDate)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            dict.Add("@WaferPN", WaferPN);
            dict.Add("@Rate", Rate);
            dict.Add("@WaferArray", WaferArray);
            dict.Add("@WaferTech", WaferTech);
            dict.Add("@WaferCount", WaferCount.ToString());
            dict.Add("@BuildDate", BuildDate);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void StoreWafeSNMap(string wafer, string sn, string sndate,string workname)
        {
            var sql = "insert into WaferSNMap(WaferNum,SN,SNDate,SNWorkFlowName) values(@WaferNum,@SN,@SNDate,@SNWorkFlowName)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            dict.Add("@SN", sn);
            dict.Add("@SNDate", sndate);
            dict.Add("@SNWorkFlowName", workname);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, WaferData> GetWaferInfoBySN(string sncond)
        {
            var ret = new Dictionary<string, WaferData>();
            var sql = @" select wsn.SN,wsn.SNDate,wd.WaferPN,wd.WaferNum,wd.Rate,wd.WaferArray,wd.WaferTech FROM [BSSupport].[dbo].[WaferSNMap] wsn (nolock) 
                          left join [BSSupport].[dbo].[WaferData] wd (nolock) on wsn.WaferNum = wd.WaferNum 
                          where wsn.SN in <sncond>";
            sql = sql.Replace("<sncond>", sncond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new WaferData();
                tempvm.SN = Convert.ToString(line[0]);

                if (!ret.ContainsKey(tempvm.SN))
                {
                    tempvm.SNDate = Convert.ToDateTime(line[1]).ToString("yyyy-MM-dd HH:mm:ss");
                    tempvm.WaferPN = Convert.ToString(line[2]);
                    tempvm.WaferNum = Convert.ToString(line[3]);
                    tempvm.Rate = Convert.ToString(line[4]);
                    tempvm.WaferArray = Convert.ToString(line[5]);
                    tempvm.WaferTech = Convert.ToString(line[6]);
                    ret.Add(tempvm.SN, tempvm);
                }
            }

            return ret;
        }

        public static Dictionary<string, int> RetriveWaferCountDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = "select WaferNum,WaferCount from WaferData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var wafernum = Convert.ToString(line[0]);
                var wafercount = Convert.ToInt32(line[1]);
                if (!ret.ContainsKey(wafernum) && wafercount > 100)
                {
                    ret.Add(wafernum, wafercount);
                }
            }
            return ret;
        }

        public static Dictionary<string, int> RetriveWaferTechCountDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = "select WaferTech,WaferCount,Rate from WaferData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tech = Convert.ToString(line[0]);
                var rate = Convert.ToString(line[2]);
                if (string.Compare(tech, "MESA", true) == 0 &&
                    (rate.Contains("10G") || rate.Contains("14G")))
                { continue; }

                var wafercount = Convert.ToInt32(line[1]);
                if (!ret.ContainsKey(tech))
                {
                    ret.Add(tech, wafercount);
                }
                else
                {
                    ret[tech] += wafercount;
                }
            }
            return ret;
        }

        public static Dictionary<string, int> RetriveWaferArrayCountDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = "select WaferArray,WaferCount from WaferData where Rate='25G'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var ary = Convert.ToString(line[0]);
                var wafercount = Convert.ToInt32(line[1]);
                if (!ret.ContainsKey(ary))
                {
                    ret.Add(ary, wafercount);
                }
                else
                {
                    ret[ary] += wafercount;
                }
            }
            return ret;
        }

        public static List<string> RetrieveDistinctWaferListASC(string rate, string startdate,string enddate)
        {
            var ret = new List<string>();
            var wdict = new Dictionary<string, bool>();
            var sql = "select WaferNum,BuildDate,Rate from WaferData where BuildDate > '<startdate>' and  BuildDate < '<enddate>'";
            sql = sql.Replace("<startdate>", startdate).Replace("<enddate>", enddate);

            if (!string.IsNullOrEmpty(rate.Trim()))
            {
                sql = sql + "  and Rate = '<Rate>'  ";
                sql = sql.Replace("<Rate>", rate);
            }
            sql = sql + " order by BuildDate ASC";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var w = Convert.ToString(line[0]);
                if (!wdict.ContainsKey(w))
                {
                    wdict.Add(w, true);
                    ret.Add(w);
                }
            }
            return ret;
        }


        public WaferData()
        {
            WaferNum = "";
            WaferPN = "";
            Rate = "";
            WaferArray = "";
            WaferTech = "";
            WaferCount = 0;
            BuildDate = "1982-05-06 10:00:00";

            SN = "";
            SNDate = "1982-05-06 10:00:00";
            SNWorkFlowName = "";
        }


        public string WaferNum { set; get; }
        public string WaferPN { set; get; }
        public string BuildDate { set; get; }
        public string Rate { set; get; }
        public string WaferArray { set; get; }
        public string WaferTech { set; get; }
        public int WaferCount { set; get; }

        public string SN { set; get; }
        public string SNDate { set; get; }
        public string SNWorkFlowName { set; get; }
    }
}