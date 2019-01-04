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
            var waferlist = LoadWaferNum("2016-02-01 00:00:00");
            foreach (var wf in waferlist)
            {
                LoadWaferData(wf, vcselpndict);
            }

        }

        public static void LoadWaferDataIn3Month(Controller ctrl)
        {
            var vcselpndict = CfgUtility.LoadVcselPNConfig(ctrl);
            var threemonth = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
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
            
            var sndict = new Dictionary<string,string>();
            var tempvm = new WaferData();

            var sql = @"SELECT distinct c.ContainerName,pb.productname MaterialPN,hml.MfgDate
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
		                 and Len(c.ContainerName) = 7 and ( WorkflowStepName like 'Shipping' or WorkflowStepName like 'Main Store' or WorkflowStepName like 'MainStore' or WorkflowStepName like 'Main_Store') order by hml.MfgDate asc";
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
                if (!sndict.ContainsKey(sn))
                {
                    var builddate = Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                    sndict.Add(sn, builddate);
                }
            }

            if (sndict.Count > 0 && vcselpndict.ContainsKey(tempvm.WaferPN))
            {
                tempvm.WaferCount = sndict.Count;
                tempvm.WaferArray = vcselpndict[tempvm.WaferPN].varray;
                tempvm.Rate = vcselpndict[tempvm.WaferPN].vrate;
                tempvm.WaferTech = vcselpndict[tempvm.WaferPN].vtech;

                CleanWaferData(Wafer);

                tempvm.StoreData();
                foreach (var kv in sndict)
                {
                    StoreWafeSNMap(Wafer, kv.Key, kv.Value);
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

        private static void StoreWafeSNMap(string wafer, string sn, string sndate)
        {
            var sql = "insert into WaferSNMap(WaferNum,SN,SNDate) values(@WaferNum,@SN,@SNDate)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            dict.Add("@SN", sn);
            dict.Add("@SNDate", sndate);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
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
    }
}