using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class YIELDACTIONTYPE {
        public static string LOAD = "LOAD";
        public static string COMPUTER = "COMPUTER";
    }
    
    public class YieldRawData
    {
        private static List<string> LoadMESTabs(Controller ctrl, string familycond, bool withappendtab = false)
        {
            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var appendtablist = new List<string>();
            if (withappendtab)
            { appendtablist.AddRange(yieldcfg["MESAPPENDTAB"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList()); }

            var mesignoretablist = yieldcfg["MESIGNORETAB"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            var sql = @"select distinct ddr.DataCollectionDefName from insitedb.insite.DataCollectionDefBase ddr  (nolock)
                        inner join insitedb.insite.TxnMap tm with(noloCK) ON tm.DataCollectionDefinitionBaseId = ddr.DataCollectionDefBaseId
                        inner join insitedb.insite.spec sp with(nolock) on sp.specid =  tm.specid
                        inner join InsiteDB.insite.WorkflowStep ws (nolock)on  ws.specbaseid = sp.specbaseid
                        inner join InsiteDB.insite.Workflow w (nolock)on w.WorkflowID = ws.WorkflowID
                        inner join InsiteDB.insite.Product p(nolock) on w.WorkflowBaseId = p.WorkflowBaseId
                        inner join [InsiteDB].[insite].[ProductBase] pb on pb.ProductBaseId = p.ProductBaseId
                        where pb.ProductName in(
                          select distinct pb.ProductName from [InsiteDB].[insite].[ProductFamily] pf
                          left join [InsiteDB].[insite].[Product] pd on pd.ProductFamilyId = pf.ProductFamilyId
                          left join [InsiteDB].[insite].[ProductBase] pb on pb.ProductBaseId = pd.ProductBaseId
                          where  (<FAMILYCOND>)  and pd.[Description] is not null 
                          and ( pd.[Description] not like 'LEN%' and pd.[Description] not like 'Shell%') 
                        ) and  ddr.DataCollectionDefName like 'DCD_%' order by ddr.DataCollectionDefName";
            sql = sql.Replace("<FAMILYCOND>", familycond);


            var ret = new List<string>();
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                try {
                    var dctab = Convert.ToString(line[0]).ToUpper().Substring(3);
                    bool ignore = false;
                    foreach (var ig in mesignoretablist)
                    {
                        if (string.Compare(dctab, ig, true) == 0 || dctab.Contains("_0811"))
                        {
                            ignore = true;
                        }
                    }
                    if (!ignore)
                    {
                        ret.Add(dctab);
                    }
                } catch (Exception ex) { }
            }//end foreach
            ret.AddRange(appendtablist);
            return ret;
        }

        private static string Convert2Str(object obj1)
        {
            if (obj1 != null)
            {
                try
                {
                    return Convert.ToString(obj1);
                } catch(Exception ex) { }
            }
            return string.Empty;
        }

        private static void _LoadParallelData(Controller ctrl, string mestab,string familycond,string yieldfamily)
        {
            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var zerodate = DateTime.Parse(yieldcfg["YIELDZERODATE"]);
            var nowmonth = DateTime.Now.ToString("yyyy-MM");
            for (; zerodate < DateTime.Now;)
            {
                var iscurrentmonth = false;
                if (string.Compare(zerodate.ToString("yyyy-MM"), nowmonth) == 0)
                { iscurrentmonth = true; }

                if (IsYieldDataActionUpdated(zerodate.ToString("yyyy-MM"), mestab, yieldfamily,YIELDACTIONTYPE.LOAD))
                {
                    zerodate = zerodate.AddMonths(1);
                    continue;
                }

                ModuleTestData.CleanTestData(yieldfamily, "DC" + mestab, zerodate);

                var sql = @"SELECT distinct [dc<DCTABLE>HistoryId] ,[ModuleSerialNum],[TestTimeStamp],[WhichTest],[ErrAbbr] ,[TestStation]
                            ,pf.ProductFamilyName ,[AssemblyPartNum],[ModulePartNum],[ModuleType],[SpecFreq_GHz] ,[TestDuration_s] 
                          FROM [InsiteDB].[insite].[dc<DCTABLE>] dc 
                          left join [InsiteDB].[insite].[ProductBase] pb on (pb.ProductName = dc.AssemblyPartNum or pb.ProductName = dc.ModulePartNum) 
                          left join [InsiteDB].[insite].[Product] pd on pd.ProductBaseId = pb.ProductBaseId  
                          left join [InsiteDB].[insite].[ProductFamily] pf on pf.ProductFamilyId = pd.ProductFamilyId 
                          where (<FAMILYCOND>) and dc.TestTimeStamp >= '<STARTDATE>' and dc.TestTimeStamp < '<ENDDATE>'  and ErrAbbr is not null  
                          and [ModuleSerialNum] is not null and Len([ModuleSerialNum]) = 7 order by ModuleSerialNum,TestTimeStamp ASC";
                sql = sql.Replace("<DCTABLE>", mestab).Replace("<FAMILYCOND>", familycond)
                    .Replace("<STARTDATE>",zerodate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("<ENDDATE>", zerodate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss"));

                var rawdata = new List<ModuleTestData>();
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    try {
                        rawdata.Add(new ModuleTestData(Convert2Str(line[0]), Convert2Str(line[1]), Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss")
                            , Convert2Str(line[3]), Convert2Str(line[4]), Convert2Str(line[5])
                            , Convert2Str(line[6]), Convert2Str(line[7]), Convert2Str(line[8])
                            , Convert2Str(line[9]), Convert2Str(line[10]), Convert2Str(line[11]),"DC"+mestab,yieldfamily));
                    } catch (Exception ex) { }
                }

                ModuleTestData.StoreData(rawdata);

                if (!iscurrentmonth)
                {
                    UpdateYieldDataAction(zerodate.ToString("yyyy-MM"), mestab, yieldfamily, YIELDACTIONTYPE.LOAD);
                }
                break;
            }
            
        }

        private static void LoadParallelData(Controller ctrl,string pdfamily,string yieldfamily)
        {
            var pdfamilylist = pdfamily.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var sb = new System.Text.StringBuilder((pdfamilylist.Count + 1) * 80);
            foreach (var family in pdfamilylist)
            {
                sb.Append(" or pf.ProductFamilyName like '" + family + "%'");
            }
            var familycond = sb.ToString().Substring(3);

            var mestables = LoadMESTabs(ctrl, familycond, true);
            foreach (var mestab in mestables)
            {
                _LoadParallelData(ctrl, mestab, familycond, yieldfamily);
            }
        }

        private static void _LoadLineCardData(Controller ctrl, string mestab,string familycond,string yieldfamily)
        {
            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var zerodate = DateTime.Parse(yieldcfg["YIELDZERODATE"]);
            var nowmonth = DateTime.Now.ToString("yyyy-MM");
            for (; zerodate < DateTime.Now;)
            {
                var iscurrentmonth = false;
                if (string.Compare(zerodate.ToString("yyyy-MM"), nowmonth) == 0)
                { iscurrentmonth = true; }

                if (IsYieldDataActionUpdated(zerodate.ToString("yyyy-MM"), mestab, yieldfamily, YIELDACTIONTYPE.LOAD))
                {
                    zerodate = zerodate.AddMonths(1);
                    continue;
                }

                ModuleTestData.CleanTestData(yieldfamily, "DC" + mestab, zerodate);

                var sql = @"SELECT distinct [dc<DCTABLE>HistoryId] ,[ModuleSerialNum],[TestTimeStamp],[WhichTest],[ErrAbbr] ,[TestStation]
                            ,pf.ProductFamilyName ,[AssemblyPartNum],[ModulePartNum],[ModuleType],[SpecFreq_GHz] ,[TestDuration_s] 
                          FROM [InsiteDB].[insite].[dc<DCTABLE>] dc 
                          left join [InsiteDB].[insite].[ProductBase] pb on (pb.ProductName = dc.AssemblyPartNum or pb.ProductName = dc.ModulePartNum) 
                          left join [InsiteDB].[insite].[Product] pd on pd.ProductBaseId = pb.ProductBaseId  
                          left join [InsiteDB].[insite].[ProductFamily] pf on pf.ProductFamilyId = pd.ProductFamilyId 
                          where (<FAMILYCOND>) and dc.TestTimeStamp >= '<STARTDATE>' and dc.TestTimeStamp < '<ENDDATE>'  and ErrAbbr is not null  
                          and [ModuleSerialNum] is not null order by ModuleSerialNum,TestTimeStamp ASC";
                sql = sql.Replace("<DCTABLE>", mestab).Replace("<FAMILYCOND>", familycond)
                    .Replace("<STARTDATE>", zerodate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("<ENDDATE>", zerodate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss"));

                var iddict = new Dictionary<string, bool>();

                var rawdata = new List<ModuleTestData>();
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    try
                    {
                        var tempvm = new ModuleTestData(Convert2Str(line[0]), Convert2Str(line[1]), Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss")
                            , Convert2Str(line[3]), Convert2Str(line[4]), Convert2Str(line[5])
                            , Convert2Str(line[6]), Convert2Str(line[7]), Convert2Str(line[8])
                            , Convert2Str(line[9]), Convert2Str(line[10]), Convert2Str(line[11]), "DC" + mestab,yieldfamily);
                        rawdata.Add(tempvm);
                        if (string.Compare(tempvm.ErrAbbr, "pass", true) != 0)
                        {
                            if (!iddict.ContainsKey(tempvm.DataID))
                            { iddict.Add(tempvm.DataID,true); }
                        }
                    }
                    catch (Exception ex) { }
                }

                var iderrordict = new Dictionary<string, string>();
                if (iddict.Count > 0)
                {
                    var idlist = iddict.Keys.ToList();
                    var idcond = "('" + string.Join("','", idlist)+"')";

                    var csql = @"select ParentHistoryID,DataColumn from[InsiteDB].[insite].[dce<DCTABLE>_main] 
                                where ParentHistoryID in <IDCOND> and DataValue2 = 'FAIL'";
                    csql = csql.Replace("<DCTABLE>", mestab).Replace("<IDCOND>", idcond);
                    var dbret2 = DBUtility.ExeMESSqlWithRes(csql);
                    foreach (var line in dbret2)
                    {
                        var id = Convert2Str(line[0]).Trim();
                        var err = Convert2Str(line[1]).Trim();
                        if (!iderrordict.ContainsKey(id))
                        { iderrordict.Add(id,err); }
                    }
                }//end if

                foreach (var item in rawdata)
                {
                    if (string.IsNullOrEmpty(item.PN))
                    { item.PN = item.PNDesc; }
                    if (iderrordict.ContainsKey(item.DataID))
                    {
                        item.ErrAbbr = iderrordict[item.DataID];
                    }
                }

                ModuleTestData.StoreData(rawdata);
                if (!iscurrentmonth)
                {
                    UpdateYieldDataAction(zerodate.ToString("yyyy-MM"), mestab, yieldfamily, YIELDACTIONTYPE.LOAD);
                }
                break;
            }
        }

        private static void LoadLineCardData(Controller ctrl,string pdfamily,string yieldfamily)
        {
            var pdfamilylist = pdfamily.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var sb = new System.Text.StringBuilder((pdfamilylist.Count + 1) * 80);
            foreach (var family in pdfamilylist)
            {
                sb.Append(" or pf.ProductFamilyName like '" + family + "%'");
            }
            var familycond = sb.ToString().Substring(3);

            var mestables = LoadMESTabs(ctrl, familycond);
            foreach (var mestab in mestables)
            {
                _LoadLineCardData(ctrl, mestab, familycond, yieldfamily);
            }
        }

        private static void _LoadTunableData(Controller ctrl, string familycond, string yieldfamily)
        {
            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var zerodate = DateTime.Parse(yieldcfg["YIELDZERODATE"]);
            var nowmonth = DateTime.Now.ToString("yyyy-MM");
            for (; zerodate < DateTime.Now;)
            {
                var iscurrentmonth = false;
                if (string.Compare(zerodate.ToString("yyyy-MM"), nowmonth) == 0)
                { iscurrentmonth = true; }

                if (IsYieldDataActionUpdated(zerodate.ToString("yyyy-MM"), "ROUTE_DATA", yieldfamily, YIELDACTIONTYPE.LOAD))
                {
                    zerodate = zerodate.AddMonths(1);
                    continue;
                }

                ModuleTestData.CleanTestData(yieldfamily, "ROUTE_DATA", zerodate);

                var rawdata = ATETestData.LoadATETestData(familycond, zerodate, zerodate.AddMonths(1), ctrl,yieldfamily);
                ModuleTestData.StoreData(rawdata);

                if (!iscurrentmonth)
                {
                    UpdateYieldDataAction(zerodate.ToString("yyyy-MM"), "ROUTE_DATA", yieldfamily, YIELDACTIONTYPE.LOAD);
                }
                break;
            }
        }
        private static void LoadTunableData(Controller ctrl,string pdfamily,string yieldfamily)
        {
            var pdfamilylist = pdfamily.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var familycond = "('" + string.Join("','", pdfamilylist) + "')";
            _LoadTunableData(ctrl, familycond, yieldfamily);
        }

        public static void LoadData(Controller ctrl)
        {
            var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
            var yieldfamilys = yieldcfg["YIELDFAMILY"].Split(new string[] { ";"},StringSplitOptions.RemoveEmptyEntries);
            foreach (var yf in yieldfamilys)
            {
                var pdfamily = yieldcfg[yf + "_FAMILY"];
                if (string.Compare(yf, "PARALLEL", true) == 0)
                {
                    LoadParallelData(ctrl,pdfamily,yf);
                }
                else if (string.Compare(yf, "LINECARD", true) == 0)
                {
                    LoadLineCardData(ctrl,pdfamily,yf);
                }
                else if (string.Compare(yf, "TUNABLE", true) == 0)
                {
                    LoadTunableData(ctrl,pdfamily,yf);
                }
            }
        }


        public static bool IsYieldDataActionUpdated(string month, string mestab,string yieldfamily,string actiontype)
        {
            var sql = "select month,mestab,yieldfamily from MesDataUpdate where month='<month>' and mestab='<mestab>' and yieldfamily='<yieldfamily>' and actiontype = '<actiontype>'";
            sql = sql.Replace("<month>", month).Replace("<mestab>", mestab).Replace("<yieldfamily>", yieldfamily).Replace("<actiontype>",actiontype);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            { return true; }
            return false;
        }

        public static void UpdateYieldDataAction(string month, string mestab,string yieldfamily,string actiontype)
        {
            var sql = "insert into MesDataUpdate(month,mestab,yieldfamily,actiontype) values('<month>','<mestab>','<yieldfamily>','<actiontype>')";
            sql = sql.Replace("<month>", month).Replace("<mestab>", mestab).Replace("<yieldfamily>", yieldfamily).Replace("<actiontype>", actiontype);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

    }
}