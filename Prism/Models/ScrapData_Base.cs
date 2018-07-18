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
            var sql = "SELECT distinct ORIGINAL_PROJECT_CODE+':::'+PRODUCT from ScrapData_Base where ORIGINAL_PROJECT_CODE <> '' and PRODUCT <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempval = Convert.ToString(line[0]);
                var co = tempval.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries)[0];
                var pd = tempval.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries)[1];
                if (!ret.ContainsKey(co))
                {
                    ret.Add(co, pd);
                }
            }
            return ret;
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