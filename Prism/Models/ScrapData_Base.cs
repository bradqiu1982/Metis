using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class ScrapData_Base
    {
        public static Dictionary<string, bool> GetKeyDict()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct DataKey from ScrapData_Base";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]), true);
            }
            return ret;
        }

        public static List<string> GetYearList()
        {
            var ret = new List<string>();
            var sql = "select distinct CrtYear from ScrapData_Base";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            ret.Sort();
            return ret;
        }

        public static List<string> GetProjectCodeList()
        {
            var ret = new List<string>();
            var sql = "select distinct ORIGINAL_PROJECT_CODE from ScrapData_Base";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            ret.Sort();
            return ret;
        }

        public void StoreData()
        {
            var sql = @"insert into ScrapData_Base(DataKey,ORGANIZATION_ID,PERIOD_NAME,TRANSACTION_DATE,ACCOUNT_COMBINATION,ACCOUNT,ITEM,ITEM_DESCRIPTION,TRANSACTION_TYPE,SUBINVENTORY_CODE,TRANSACTION_ID,JOB,PRIMARY_QUANTITY_1,ASSEMBLY,REASON_NAME,REFERENCE,JOB_PREFIX,ORIGINAL_PROJECT_CODE,PRODUCT_GROUP,PRODUCT,JOB_POSTFIX,PLM,Scrap_Or_Output,Current_Total_Cost_USD,Actual_Q1Output,Actual_Q1Scrap,Transaction_Value,Transaction_Value_Usd,Transaction_Value_Usd_1,value,Week,CrtYear,CrtQuarter) 
                         values(@DataKey,@ORGANIZATION_ID,@PERIOD_NAME,@TRANSACTION_DATE,@ACCOUNT_COMBINATION,@ACCOUNT,@ITEM,@ITEM_DESCRIPTION,@TRANSACTION_TYPE,@SUBINVENTORY_CODE,@TRANSACTION_ID,@JOB,@PRIMARY_QUANTITY_1,@ASSEMBLY,@REASON_NAME,@REFERENCE,@JOB_PREFIX,@ORIGINAL_PROJECT_CODE,@PRODUCT_GROUP,@PRODUCT,@JOB_POSTFIX,@PLM,@Scrap_Or_Output,@Current_Total_Cost_USD,@Actual_Q1Output,@Actual_Q1Scrap,@Transaction_Value,@Transaction_Value_Usd,@Transaction_Value_Usd_1,@value,@Week,@CrtYear,@CrtQuarter)";
            var param = new Dictionary<string, string>();
            param.Add(@"DataKey", DataKey);
            param.Add(@"ORGANIZATION_ID", ORGANIZATION_ID);
            param.Add(@"PERIOD_NAME", PERIOD_NAME);
            param.Add(@"TRANSACTION_DATE", TRANSACTION_DATE.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add(@"ACCOUNT_COMBINATION", ACCOUNT_COMBINATION);
            param.Add(@"ACCOUNT", ACCOUNT);
            param.Add(@"ITEM", ITEM);
            param.Add(@"ITEM_DESCRIPTION", ITEM_DESCRIPTION);
            param.Add(@"TRANSACTION_TYPE", TRANSACTION_TYPE);
            param.Add(@"SUBINVENTORY_CODE", SUBINVENTORY_CODE);
            param.Add(@"TRANSACTION_ID", TRANSACTION_ID);
            param.Add(@"JOB", JOB);
            param.Add(@"PRIMARY_QUANTITY_1", PRIMARY_QUANTITY_1);
            param.Add(@"ASSEMBLY", ASSEMBLY);
            param.Add(@"REASON_NAME", REASON_NAME);
            param.Add(@"REFERENCE", REFERENCE);
            param.Add(@"JOB_PREFIX", JOB_PREFIX);
            param.Add(@"ORIGINAL_PROJECT_CODE", ORIGINAL_PROJECT_CODE);
            param.Add(@"PRODUCT_GROUP", PRODUCT_GROUP);
            param.Add(@"PRODUCT", PRODUCT);
            param.Add(@"JOB_POSTFIX", JOB_POSTFIX);
            param.Add(@"PLM", PLM);
            param.Add(@"Scrap_Or_Output", Scrap_Or_Output);
            param.Add(@"Current_Total_Cost_USD", Current_Total_Cost_USD);
            param.Add(@"Actual_Q1Output", Actual_Q1Output);
            param.Add(@"Actual_Q1Scrap", Actual_Q1Scrap);
            param.Add(@"Transaction_Value", Transaction_Value);
            param.Add(@"Transaction_Value_Usd", Transaction_Value_Usd);
            param.Add(@"Transaction_Value_Usd_1", Transaction_Value_Usd_1);
            param.Add(@"value", value);
            param.Add(@"Week", Week);
            param.Add(@"CrtYear", CrtYear);
            param.Add(@"CrtQuarter", CrtQuarter);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static List<ScrapData_Base> RetrieveCostCenterQuarterData(string co, string fyear, string fquarter)
        {
            var ret = new List<ScrapData_Base>();

            var sql = @"select Scrap_Or_Output,REASON_NAME,Transaction_Value_Usd_1,Week,ASSEMBLY from ScrapData_Base 
                         where ORIGINAL_PROJECT_CODE=@ORIGINAL_PROJECT_CODE and CrtYear=@CrtYear and CrtQuarter=@CrtQuarter and Week <> ''";
            var param = new Dictionary<string, string>();
            param.Add("@ORIGINAL_PROJECT_CODE",co);
            param.Add("@CrtYear",fyear);
            param.Add("@CrtQuarter", fquarter);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                var tempvm = new ScrapData_Base();
                tempvm.Scrap_Or_Output = Convert.ToString(line[0]);
                tempvm.REASON_NAME = Convert.ToString(line[1]);
                tempvm.Transaction_Value_Usd_1 = Convert.ToString(line[2]);
                tempvm.Week = Convert.ToString(line[3]);
                tempvm.ASSEMBLY = Convert.ToString(line[4]);

                ret.Add(tempvm);
            }
            return ret;
        }

        public static Dictionary<string,List<ScrapData_Base>> RetrieveProductQuarterDataFromCoByPNMAP(string co, string fyear, string fquarter, Dictionary<string,PNPlannerCodeMap> pnmap)
        {
            var alldata = RetrieveCostCenterQuarterData(co, fyear, fquarter);
            var ret = new Dictionary<string, List<ScrapData_Base>>();
            foreach (var onedata in alldata)
            {
                var product = onedata.ASSEMBLY;
                if (pnmap.ContainsKey(product))
                {
                    var pnitem = pnmap[product];
                    if (!string.IsNullOrEmpty(pnitem.PJName))
                    { product = pnitem.PJName; }
                    else
                    { product = pnitem.PlannerCode; }
                }

                if (ret.ContainsKey(product))
                {
                    ret[product].Add(onedata);
                }
                else
                {
                    var templist = new List<ScrapData_Base>();
                    templist.Add(onedata);
                    ret.Add(product, templist);
                }
            }//end foreach

            return ret;
        }

        private static List<ScrapData_Base> RetrieveScrapDataByPD(string pd)
        {
            var ret = new List<ScrapData_Base>();

            var sql = @"select Scrap_Or_Output,REASON_NAME,Transaction_Value_Usd_1,Week,ASSEMBLY,CrtYear,CrtQuarter from ScrapData_Base 
                         where ASSEMBLY in ( SELECT DISTINCT [PN]  FROM [BSSupport].[dbo].[PNPlannerCodeMap] where PJName = '<product>' or PlannerCode = '<product>') and Week <> ''";
            sql = sql.Replace("<product>", pd);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new ScrapData_Base();
                tempvm.Scrap_Or_Output = Convert.ToString(line[0]);
                tempvm.REASON_NAME = Convert.ToString(line[1]);
                tempvm.Transaction_Value_Usd_1 = Convert.ToString(line[2]);
                tempvm.Week = Convert.ToString(line[3]);
                tempvm.ASSEMBLY = Convert.ToString(line[4]);
                tempvm.CrtYear = Convert.ToString(line[5]);
                tempvm.CrtQuarter = Convert.ToString(line[6]);

                ret.Add(tempvm);
            }
            return ret;
        }

        public static Dictionary<string, List<ScrapData_Base>> RetrieveProductDataByPD(string pd)
        {
            var alldata = RetrieveScrapDataByPD(pd);
            //filter data by week num only have 4 weeks data in a querter will be use
            var qdict = new Dictionary<string, Dictionary<string, bool>>();
            foreach (var item in alldata)
            {
                var q = item.CrtYear + "-" + item.CrtQuarter;
                if (qdict.ContainsKey(q))
                {
                    var wdict = qdict[q];
                    if (!wdict.ContainsKey(item.Week))
                    {
                        wdict.Add(item.Week, true);
                    }
                }
                else
                {
                    var wdict = new Dictionary<string, bool>();
                    wdict.Add(item.Week, true);
                    qdict.Add(q, wdict);
                }
            }


            var newalldata = new List<ScrapData_Base>();
            foreach (var item in alldata)
            {
                var q = item.CrtYear + "-" + item.CrtQuarter;
                if (qdict[q].Keys.Count > 3)
                {
                    newalldata.Add(item);
                }
            }

            var ret = new Dictionary<string, List<ScrapData_Base>>();
            ret.Add(pd, newalldata);
            return ret;
        }


        public static List<ScrapData_Base> RetrieveScrapDataByStandardPD(string pd)
        {
            var ret = new List<ScrapData_Base>();

            var sql = @"select Scrap_Or_Output,REASON_NAME,Transaction_Value_Usd_1,Week,ASSEMBLY,CrtYear,CrtQuarter from ScrapData_Base 
                         where PRODUCT = @PRODUCT";
            var dict = new Dictionary<string, string>();
            dict.Add("@PRODUCT",pd);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null,dict);
            foreach (var line in dbret)
            {
                var tempvm = new ScrapData_Base();
                tempvm.Scrap_Or_Output = Convert.ToString(line[0]);
                tempvm.REASON_NAME = Convert.ToString(line[1]);
                tempvm.Transaction_Value_Usd_1 = Convert.ToString(line[2]);
                tempvm.Week = Convert.ToString(line[3]);
                tempvm.ASSEMBLY = Convert.ToString(line[4]);
                tempvm.CrtYear = Convert.ToString(line[5]);
                tempvm.CrtQuarter = Convert.ToString(line[6]);

                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<ScrapData_Base> RetrieveScrapDataByPG(string pg)
        {
            var ret = new List<ScrapData_Base>();

            var sql = @"select Scrap_Or_Output,REASON_NAME,Transaction_Value_Usd_1,Week,ASSEMBLY,CrtYear,CrtQuarter from ScrapData_Base 
                         where PRODUCT_GROUP = @PRODUCT_GROUP";
            var dict = new Dictionary<string, string>();
            dict.Add("@PRODUCT_GROUP", pg);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new ScrapData_Base();
                tempvm.Scrap_Or_Output = Convert.ToString(line[0]);
                tempvm.REASON_NAME = Convert.ToString(line[1]);
                tempvm.Transaction_Value_Usd_1 = Convert.ToString(line[2]);
                tempvm.Week = Convert.ToString(line[3]);
                tempvm.ASSEMBLY = Convert.ToString(line[4]);
                tempvm.CrtYear = Convert.ToString(line[5]);
                tempvm.CrtQuarter = Convert.ToString(line[6]);

                ret.Add(tempvm);
            }
            return ret;
        }


        public static List<string> RetrievePJCodeByDepartment(string dp,string fyear, string fquarter)
        {
            var ret = new List<string>();
            var sql = @"select distinct ORIGINAL_PROJECT_CODE  from ScrapData_Base 
                         where PRODUCT_GROUP=@PRODUCT_GROUP and CrtYear=@CrtYear and CrtQuarter=@CrtQuarter and ORIGINAL_PROJECT_CODE <> '' order by ORIGINAL_PROJECT_CODE asc";
            var param = new Dictionary<string, string>();
            param.Add("@PRODUCT_GROUP", dp);
            param.Add("@CrtYear", fyear);
            param.Add("@CrtQuarter", fquarter);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<ScrapData_Base> RetrieveDPQuarterData(string department, string fyear, string fquarter)
        {
            var ret = new List<ScrapData_Base>();

            var sql = @"select Scrap_Or_Output,REASON_NAME,Transaction_Value_Usd_1,PRODUCT_GROUP from ScrapData_Base 
                         where PRODUCT_GROUP=@PRODUCT_GROUP and CrtYear=@CrtYear and CrtQuarter=@CrtQuarter and Week <> ''";
            var param = new Dictionary<string, string>();
            param.Add("@PRODUCT_GROUP", department);
            param.Add("@CrtYear", fyear);
            param.Add("@CrtQuarter", fquarter);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                var tempvm = new ScrapData_Base();
                tempvm.Scrap_Or_Output = Convert.ToString(line[0]);
                tempvm.REASON_NAME = Convert.ToString(line[1]);
                tempvm.Transaction_Value_Usd_1 = Convert.ToString(line[2]);
                tempvm.PRODUCT_GROUP = Convert.ToString(line[3]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static Dictionary<string, Dictionary<string, double>> RetrieveAllOutputData()
        {
            var ret = new Dictionary<string, Dictionary<string, double>>();

            var coherentdict = PNProuctFamilyCache.GetPNDictByPF("COHERENT");

            var sql = "select Transaction_Value_Usd_1,PRODUCT_GROUP,CrtYear+' '+CrtQuarter,ASSEMBLY from ScrapData_Base where Scrap_Or_Output= '<output>' order by PRODUCT_GROUP";
            sql = sql.Replace("<output>", SCRAPOUTPUTSCRAP.OUTPUT);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var output = Convert.ToDouble(line[0]);
                    var department = Convert.ToString(line[1]);
                    var quarter = Convert.ToString(line[2]);
                    var assembly = Convert.ToString(line[3]);
                    if (coherentdict.ContainsKey(assembly))
                    {
                        department = "COHERENT";
                    }
                    if (ret.ContainsKey(department))
                    {
                        var qdict = ret[department];
                        if (qdict.ContainsKey(quarter))
                        {
                            qdict[quarter] += output;
                        }
                        else
                        {
                            qdict.Add(quarter, output);
                        }
                    }
                    else
                    {
                        var qdict = new Dictionary<string, double>();
                        qdict.Add(quarter, output);
                        ret.Add(department, qdict);
                    }
                }
                catch (Exception ex) { }
            }
            return ret;
        }

        public static List<ScrapData_Base> RetrieveOutputData(string dp,string qt)
        {
            var ret = new Dictionary<string,ScrapData_Base>();

            var sql = "select PRODUCT,PRIMARY_QUANTITY_1,Transaction_Value_Usd_1 from ScrapData_Base  where  Scrap_Or_Output= '<output>' and PRODUCT_GROUP = '<PRODUCT_GROUP>' ";
            if (string.Compare(dp, "COHERENT", true) == 0)
            {
                var pnlist = PNProuctFamilyCache.GetPNListByPF("COHERENT");
                var pncond = "('" + string.Join("','", pnlist) + "')";
                sql = "select PRODUCT,PRIMARY_QUANTITY_1,Transaction_Value_Usd_1 from ScrapData_Base  where  Scrap_Or_Output= '<output>' and ASSEMBLY in <pncond> ";
                sql = sql.Replace("<pncond>", pncond);
            }

            if (!string.IsNullOrEmpty(qt))
            {
                sql += "and CrtYear+' '+CrtQuarter = '<quarter>'";
                sql = sql.Replace("<quarter>", qt);
            }
            sql += " order by PRODUCT,TRANSACTION_DATE";
            sql = sql.Replace("<PRODUCT_GROUP>", dp).Replace("<output>", SCRAPOUTPUTSCRAP.OUTPUT);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var vm = new ScrapData_Base();
                    vm.PRODUCT = Convert.ToString(line[0]);
                    vm.PRIMARY_QUANTITY_1 = Convert.ToString(line[1]);
                    vm.Transaction_Value_Usd_1 = Convert.ToString(line[2]);
                    if (ret.ContainsKey(vm.PRODUCT))
                    {
                        ret[vm.PRODUCT].PRIMARY_QUANTITY_1 = Math.Round(Convert.ToDouble(ret[vm.PRODUCT].PRIMARY_QUANTITY_1) + Convert.ToDouble(vm.PRIMARY_QUANTITY_1),3).ToString();
                        ret[vm.PRODUCT].Transaction_Value_Usd_1 = Math.Round(Convert.ToDouble(ret[vm.PRODUCT].Transaction_Value_Usd_1) + Convert.ToDouble(vm.Transaction_Value_Usd_1), 3).ToString();
                    }
                    else
                    {
                        ret.Add(vm.PRODUCT, vm);
                    }
                }
                catch (Exception ex) { }
            }
            var retlist = ret.Values.ToList();
            retlist.Sort(delegate (ScrapData_Base obj1, ScrapData_Base obj2)
            {
                var d1 = Convert.ToDouble(obj1.Transaction_Value_Usd_1);
                var d2 = Convert.ToDouble(obj2.Transaction_Value_Usd_1);
                return d2.CompareTo(d1);
            });
            return retlist;
        }

        public static List<string> RetrieveWeekListByPJ(string pjcode, string fyear, string fquarter)
        {
            var ret = new List<string>();

            var sql = @"select distinct Week from ScrapData_Base 
                         where ORIGINAL_PROJECT_CODE=@ORIGINAL_PROJECT_CODE and CrtYear=@CrtYear and CrtQuarter=@CrtQuarter and Week <> ''";
            var param = new Dictionary<string, string>();
            param.Add("@ORIGINAL_PROJECT_CODE", pjcode);
            param.Add("@CrtYear", fyear);
            param.Add("@CrtQuarter", fquarter);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }

            ret.Sort(delegate (string obj1, string obj2)
            {
                var i1 = Convert.ToInt32(obj1.Substring(obj1.Length - 2));
                var i2 = Convert.ToInt32(obj2.Substring(obj2.Length - 2));
                return i1.CompareTo(i2);
            });

            return ret;
        }

        public static List<string> RetrieveDPByTime(string fyear, string fquarter)
        {
            var ret = new List<string>();

            var sql = @"select distinct PRODUCT_GROUP from ScrapData_Base 
                         where CrtYear=@CrtYear and CrtQuarter=@CrtQuarter and PRODUCT_GROUP <> '' order by PRODUCT_GROUP asc";
            var param = new Dictionary<string, string>();

            param.Add("@CrtYear", fyear);
            param.Add("@CrtQuarter", fquarter);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }

            return ret;
        }

        public static Dictionary<string, string> CostCentProductMap()
        {
            var ret = new Dictionary<string, string>();
            var sql = "SELECT distinct ORIGINAL_PROJECT_CODE from ScrapData_Base where ORIGINAL_PROJECT_CODE <> '' and PRODUCT <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var colist = new List<string>();
            foreach (var line in dbret)
            {
                colist.Add(Convert.ToString(line[0]));
            }

            foreach (var co in colist)
            {
                sql = "select top 1 PRODUCT,COUNT(PRODUCT) num from [BSSupport].[dbo].[ScrapData_Base] where ORIGINAL_PROJECT_CODE = '<pjcode>' group by PRODUCT order by num  desc";
                sql = sql.Replace("<pjcode>", co);
                dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
                ret.Add(co, Convert.ToString(dbret[0][0]));
            }
            return ret;
        }

        public static List<string> RetrievePNByPG(string pg)
        {
            var ret = new List<string>();
            var sql = "select distinct ASSEMBLY from ScrapData_Base where PRODUCT_GROUP = '<productgroup>'";
            sql = sql.Replace("<productgroup>", pg);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        private static List<string> GetAllPNList()
        {
            var ret = new List<string>();
            var sql = "select distinct ASSEMBLY from ScrapData_Base";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<string> GetAllProductList()
        {
            var ret = new List<string>();
            var sql = "select distinct PRODUCT from ScrapData_Base";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static void UpdateProduct()
        {
            var pndict = PNProuctFamilyCache.PNPFDict();
            var pnlist = GetAllPNList();
            foreach (var pn in pnlist)
            {
                if (pndict.ContainsKey(pn))
                {
                    var sql = "update ScrapData_Base set PRODUCT=@PRODUCT where ASSEMBLY=@ASSEMBLY";
                    var dict = new Dictionary<string, string>();
                    dict.Add("@ASSEMBLY", pn);
                    dict.Add("@Product", pndict[pn]);
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
            }

        }

        public static object GetScrapTable(List<ScrapData_Base> onepddata, string pd, bool fordepartment)
        {
            var sumdata = SCRAPSUMData.GetSumDataFromRawDataByQuarter(onepddata);

            var qlist = sumdata.Keys.ToList();
            qlist.Sort(delegate (string obj1, string obj2)
            {
                var i1 = QuarterCLA.RetrieveDateFromQuarter(obj1)[0];
                var i2 = QuarterCLA.RetrieveDateFromQuarter(obj2)[0];
                return i1.CompareTo(i2);
            });

            var titlelist = new List<object>();
            titlelist.Add("Scrap");
            titlelist.Add("");
            foreach (var q in qlist)
            {
                titlelist.Add(q.Replace("-", " "));
            }


            var linelist = new List<object>();
            if (fordepartment)
            {
                linelist.Add("<a href='/Scrap/DepartmentScrap' target='_blank'>" + pd + "</a>");
            }
            else
            {
                linelist.Add("<a href='/Scrap/ScrapTrend?product=" + HttpUtility.UrlEncode(pd) + "' target='_blank'>" + pd + "</a>");
            }

            linelist.Add("<span class='YFPY'>General Scrap</span><br><span class='YFY'>Nonchina Scrap</span><br><span class='YINPUT'>Total Scrap</span><br><span class='YINPUT'>OUTPUT"
                +"</span><br><span class='YFPY'>General Scrap Rate</span><br><span class='YFY'>Nonchina Scrap Rate</span><br><span class='YINPUT'>Total Scrap Rate</span>");

            //var outputiszero = false;
            var sumscraplist = new List<SCRAPSUMData>();
            foreach (var q in qlist)
            {
                sumscraplist.Add(sumdata[q]);
            }


            if (sumscraplist.Count > 0)
            {

                var generalscrap = new List<double>();
                var nonchinascrap = new List<double>();
                var totlescrap = new List<double>();

                var generalscraprate = new List<double>();
                var nonchinascraprate = new List<double>();
                var totlescraprate = new List<double>();

                foreach (var item in sumscraplist)
                {
                    var gs = Math.Round(item.generalscrap, 2);
                    var ncs = Math.Round(item.nonchinascrap, 2);
                    var ts = gs + ncs;
                    var output = Math.Round(item.output, 2);
                    var gsr = Math.Round(item.generalscrap / item.output * 100.0, 2);
                    var ncsr = Math.Round(item.nonchinascrap / item.output * 100.0, 2);
                    var tsr = Math.Round(ts / item.output * 100.0, 2);

                    linelist.Add("<span class='YFPY'>"+ String.Format("{0:n0}", gs) +"</span><br><span class='YFY'>"+ String.Format("{0:n0}", ncs) +"</span><br><span class='YINPUT'>"+ String.Format("{0:n0}", ts) +"</span><br><span class='YINPUT'>"+ String.Format("{0:n0}", output)
                        +"</span><br><span class='YFPY'>"+gsr+"%</span><br><span class='YFY'>"+ncsr+"%</span><br><span class='YINPUT'>"+tsr+"%</span>");
                }
            }

            return new
            {
                tabletitle = titlelist,
                tablecontent = linelist
            };

        }



        public ScrapData_Base()
        {
            DataKey = "";
            ORGANIZATION_ID = "";
            PERIOD_NAME = "";
            TRANSACTION_DATE = DateTime.Parse("1982-05-06 10:00:00");
            ACCOUNT_COMBINATION = "";
            ACCOUNT = "";
            ITEM = "";
            ITEM_DESCRIPTION = "";
            TRANSACTION_TYPE = "";
            SUBINVENTORY_CODE = "";
            TRANSACTION_ID = "";
            JOB = "";
            PRIMARY_QUANTITY_1 = "";
            ASSEMBLY = "";
            REASON_NAME = "";
            REFERENCE = "";
            JOB_PREFIX = "";
            ORIGINAL_PROJECT_CODE = "";
            PRODUCT_GROUP = "";
            PRODUCT = "";
            JOB_POSTFIX = "";
            PLM = "";
            Scrap_Or_Output = "";
            Current_Total_Cost_USD = "";
            Actual_Q1Output = "";
            Actual_Q1Scrap = "";
            Transaction_Value = "";
            Transaction_Value_Usd = "";
            Transaction_Value_Usd_1 = "";
            value = "";
            Week = "";
            CrtYear = "";
            CrtQuarter = "";
        }

        public string DataKey { set; get; }
        public string ORGANIZATION_ID { set; get; }
        public string PERIOD_NAME { set; get; }
        public DateTime TRANSACTION_DATE { set; get; }
        public string ACCOUNT_COMBINATION { set; get; }
        public string ACCOUNT { set; get; }
        public string ITEM { set; get; }
        public string ITEM_DESCRIPTION { set; get; }
        public string TRANSACTION_TYPE { set; get; }
        public string SUBINVENTORY_CODE { set; get; }
        public string TRANSACTION_ID { set; get; }
        public string JOB { set; get; }
        public string PRIMARY_QUANTITY_1 { set; get; }
        public string ASSEMBLY { set; get; }
        public string REASON_NAME { set; get; }
        public string REFERENCE { set; get; }
        public string JOB_PREFIX { set; get; }
        public string ORIGINAL_PROJECT_CODE { set; get; }
        public string PRODUCT_GROUP { set; get; }
        public string PRODUCT { set; get; }
        public string JOB_POSTFIX { set; get; }
        public string PLM { set; get; }
        public string Scrap_Or_Output { set; get; }
        public string Current_Total_Cost_USD { set; get; }
        public string Actual_Q1Output { set; get; }
        public string Actual_Q1Scrap { set; get; }
        public string Transaction_Value { set; get; }
        public string Transaction_Value_Usd { set; get; }
        public string Transaction_Value_Usd_1 { set; get; }
        public string value { set; get; }
        public string Week { set; get; }
        public string CrtYear { set; get; }
        public string CrtQuarter { set; get; }

    }

    public class SCRAPTYPE {
        public static string GENERALSCRAP = "09090-000";
        public static string NONCHINASCRAP = "SHG-S-0064";
        public static string SPCORTSCRAP = "SHG-S-0013";
    }

    public class SCRAPOUTPUTSCRAP {
        public static string OUTPUT = "OUTPUT";
        public static string SCRAP = "SCRAP";
    }

    public class SCRAPSUMData
    {
        public SCRAPSUMData()
        {
            key = "";
            output = 0.0;
            generalscrap = 0.0;
            nonchinascrap = 0.0;
            spcortscrap = 0.0;
        }

        private static double ConvertToDouble(string val)
        {
            try
            {
                return Math.Round(Convert.ToDouble(val),2);
            }
            catch (Exception ex) { return 0.0; }
        }

        public static Dictionary<string, SCRAPSUMData> GetSumDataFromRawDataByWeek(List<ScrapData_Base> onepjdata)
        {
            var sumdata = new Dictionary<string, SCRAPSUMData>();

            foreach (var item in onepjdata)
            {
                if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0
                    || string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                {
                    if (sumdata.ContainsKey(item.Week))
                    {
                        var tempvm = sumdata[item.Week];

                        if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0)
                        {
                            tempvm.output += ConvertToDouble(item.Transaction_Value_Usd_1);
                        }
                        else if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                        {
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.GENERALSCRAP, true) == 0)
                            { tempvm.generalscrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.NONCHINASCRAP, true) == 0)
                            { tempvm.nonchinascrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.SPCORTSCRAP, true) == 0)
                            { tempvm.spcortscrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                        }
                    }
                    else
                    {
                        var tempvm = new SCRAPSUMData();
                        tempvm.key = item.Week;
                        if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0)
                        {
                            tempvm.output = ConvertToDouble(item.Transaction_Value_Usd_1);
                        }
                        else if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                        {
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.GENERALSCRAP, true) == 0)
                            { tempvm.generalscrap = ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.NONCHINASCRAP, true) == 0)
                            { tempvm.nonchinascrap = ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.SPCORTSCRAP, true) == 0)
                            { tempvm.spcortscrap = ConvertToDouble(item.Transaction_Value_Usd_1); }
                        }
                        sumdata.Add(item.Week, tempvm);
                    }
                }//end if
            }//end foreach

            return sumdata;
        }

        public static Dictionary<string, SCRAPSUMData> GetSumDataFromRawDataByQuarter(List<ScrapData_Base> onepjdata)
        {
            var sumdata = new Dictionary<string, SCRAPSUMData>();

            foreach (var item in onepjdata)
            {
                var q = item.CrtYear + "-" + item.CrtQuarter;

                if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0
                    || string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                {
                    if (sumdata.ContainsKey(q))
                    {
                        var tempvm = sumdata[q];

                        if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0)
                        {
                            tempvm.output += ConvertToDouble(item.Transaction_Value_Usd_1);
                        }
                        else if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                        {
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.GENERALSCRAP, true) == 0)
                            { tempvm.generalscrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.NONCHINASCRAP, true) == 0)
                            { tempvm.nonchinascrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.SPCORTSCRAP, true) == 0)
                            { tempvm.spcortscrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                        }
                    }
                    else
                    {
                        var tempvm = new SCRAPSUMData();
                        tempvm.key = q;
                        if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0)
                        {
                            tempvm.output = ConvertToDouble(item.Transaction_Value_Usd_1);
                        }
                        else if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                        {
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.GENERALSCRAP, true) == 0)
                            { tempvm.generalscrap = ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.NONCHINASCRAP, true) == 0)
                            { tempvm.nonchinascrap = ConvertToDouble(item.Transaction_Value_Usd_1); }
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.SPCORTSCRAP, true) == 0)
                            { tempvm.spcortscrap = ConvertToDouble(item.Transaction_Value_Usd_1); }
                        }
                        sumdata.Add(q, tempvm);
                    }
                }//end if
            }//end foreach

            return sumdata;
        }

        public static SCRAPSUMData GetSumDataFromRawDataByDP(List<ScrapData_Base> onepjdata)
        {
            var tempvm = new SCRAPSUMData();
            tempvm.key = onepjdata[0].PRODUCT_GROUP;

            foreach (var item in onepjdata)
            {
                if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0
                    || string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                {
                    if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0)
                    {
                        tempvm.output += ConvertToDouble(item.Transaction_Value_Usd_1);
                    }
                    else if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                    {
                        if (string.Compare(item.REASON_NAME, SCRAPTYPE.GENERALSCRAP, true) == 0)
                        { tempvm.generalscrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                        if (string.Compare(item.REASON_NAME, SCRAPTYPE.NONCHINASCRAP, true) == 0)
                        { tempvm.nonchinascrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                        if (string.Compare(item.REASON_NAME, SCRAPTYPE.SPCORTSCRAP, true) == 0)
                        { tempvm.spcortscrap += ConvertToDouble(item.Transaction_Value_Usd_1); }
                    }
                }//end if
            }//end foreach

            return tempvm;
        }
        

        public string key { set; get; }
        public double output { set; get; }
        public double generalscrap { set; get; }
        public double nonchinascrap { set; get; }
        public double spcortscrap { set; get; }
    }
}