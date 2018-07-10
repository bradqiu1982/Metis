using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace Metis.Models
{
    public class ExternalDataCollector
    {
        public static void LoadScrapData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var srcfile = syscfg["SCRAPDATAFILE"];
            var desfile = DownloadShareFile(srcfile, ctrl);
            if (!string.IsNullOrEmpty(desfile) && File.Exists(desfile))
            {
                var rawdata = RetrieveDataFromExcelWithAuth(ctrl, desfile);
                if (rawdata != null && rawdata.Count > 0)
                {
                    var newdata = new List<ScrapData_Base>();

                    var keydict = ScrapData_Base.GetKeyDict();
                    var idx = 0;
                    foreach (var line in rawdata)
                    {
                        if (idx == 0)
                        {
                            idx = idx + 1;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line[5])
                            && !string.IsNullOrEmpty(line[9])
                            && !string.IsNullOrEmpty(line[16])
                            && !string.IsNullOrEmpty(line[27])
                            && !string.IsNullOrEmpty(line[29]))
                        {
                            //pn,transid,qty,assembly,wk
                            var tempkey = line[4] + "_" + line[5] + "_" + line[9] + "_" + line[11] + "_" + line[12] + "_" + line[29];
                            if (!keydict.ContainsKey(tempkey))
                            {
                                var tempvm = new ScrapData_Base();
                                tempvm.DataKey = tempkey;
                                tempvm.ORGANIZATION_ID = line[0];
                                tempvm.PERIOD_NAME = line[1];
                                tempvm.TRANSACTION_DATE = DateTime.Parse(ConvertToDateStr(line[2]));
                                tempvm.ACCOUNT_COMBINATION = line[3];
                                tempvm.ACCOUNT = line[4];
                                tempvm.ITEM = line[5];
                                tempvm.ITEM_DESCRIPTION = line[6];
                                tempvm.TRANSACTION_TYPE = line[7];
                                tempvm.SUBINVENTORY_CODE = line[8];
                                tempvm.TRANSACTION_ID = line[9];
                                tempvm.JOB = line[10];
                                tempvm.PRIMARY_QUANTITY_1 = line[11];
                                tempvm.ASSEMBLY = line[12];
                                tempvm.REASON_NAME = line[13];
                                tempvm.REFERENCE = line[14];
                                tempvm.JOB_PREFIX = line[15];
                                tempvm.ORIGINAL_PROJECT_CODE = line[16];
                                tempvm.PRODUCT_GROUP = line[17];
                                tempvm.PRODUCT = line[18];
                                tempvm.JOB_POSTFIX = line[19];
                                tempvm.PLM = line[20];
                                tempvm.Scrap_Or_Output = line[21];
                                tempvm.Current_Total_Cost_USD = line[22];
                                tempvm.Actual_Q1Output = line[23];
                                tempvm.Actual_Q1Scrap = line[24];
                                tempvm.Transaction_Value = line[25];
                                tempvm.Transaction_Value_Usd = line[26];
                                tempvm.Transaction_Value_Usd_1 = line[27];
                                tempvm.value = line[28];
                                tempvm.Week = line[29].Replace("+","").Trim();
                                tempvm.CrtYear = GetFYearByTime(tempvm.TRANSACTION_DATE);
                                tempvm.CrtQuarter = GetFQuarterByTime(tempvm.TRANSACTION_DATE);
                                if (string.IsNullOrEmpty(tempvm.CrtYear) || string.IsNullOrEmpty(tempvm.CrtQuarter)) { continue; }

                                newdata.Add(tempvm);
                            }//check dup
                        }//check empty row
                    }//end foreach

                    foreach (var item in newdata)
                    {
                        item.StoreData();
                    }
                }//end if

                try { File.Delete(desfile); } catch (Exception ex) { }
            }//end if
        }

        public static Dictionary<string, string> GetPlannerCodePJMap()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select [ProjectName],[ColumnValue] from [NebulaTrace].[dbo].[ProjectVM] where [ColumnName] = 'Planner Code'";
            var dbret = DBUtility.ExeNebulaSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var pjname = Convert.ToString(line[0]);
                    if (pjname.Contains("/"))
                    {
                        var idx = pjname.IndexOf("/") + 1;
                        pjname = pjname.Substring(idx);
                    }
                    var plannercodes = Convert.ToString(line[1]).Trim().ToUpper();
                    var plannercodelist = plannercodes.Split(new string[] { "/", " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (var pl in plannercodelist)
                    {
                        if (!ret.ContainsKey(pl))
                        {
                            ret.Add(pl, pjname);
                        }
                    }
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public static Dictionary<string, string> GetCostCenterPMMap()
        {
            var ret = new Dictionary<string, string>();
            var sql = @"SELECT [ProjectName] ,[ColumnName] ,[ColumnValue]
                         FROM [NebulaTrace].[dbo].[ProjectVM] where ColumnName in ('COST CENTER','PM') order by ProjectName";

            var tempdict = new Dictionary<string, KeyValueCLA>();

            var dbret = DBUtility.ExeNebulaSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var pjname = Convert.ToString(line[0]);
                    var colname = Convert.ToString(line[1]);
                    var colval = Convert.ToString(line[2]);

                    if (tempdict.ContainsKey(pjname))
                    {
                        if (string.Compare(colname, "COST CENTER") == 0)
                        { tempdict[pjname].Key = colval; }
                        else
                        { tempdict[pjname].Value = colval; }
                    }
                    else
                    {
                        var tempval = new KeyValueCLA();
                        tempdict.Add(pjname, tempval);
                        if (string.Compare(colname, "COST CENTER") == 0)
                        { tempdict[pjname].Key = colval; }
                        else
                        { tempdict[pjname].Value = colval; }
                    }
                }
                catch (Exception ex) { }
            }

            foreach (var kv in tempdict)
            {
                if (!ret.ContainsKey(kv.Value.Key))
                {
                    ret.Add(kv.Value.Key, kv.Value.Value);
                }
            }//end foreach

            return ret;
        }

        public static void LoadPNPlannerData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var srcfile = syscfg["PNPLANNERCODEMAP"];
            var desfile = DownloadShareFile(srcfile, ctrl);
            if (!string.IsNullOrEmpty(desfile) && File.Exists(desfile))
            {
                var rawdata = RetrieveDataFromExcelWithAuth(ctrl, desfile);
                if (rawdata != null && rawdata.Count > 0)
                {
                    var plannerpjmap = GetPlannerCodePJMap();
                    var allmap = PNPlannerCodeMap.RetrieveAllMaps();
                    var maplist = new List<PNPlannerCodeMap>();
                    foreach (var line in rawdata)
                    {
                        if (!string.IsNullOrEmpty(line[2])
                            && !string.IsNullOrEmpty(line[5]))
                        {
                            var pn = line[2];
                            if (!allmap.ContainsKey(pn))
                            {
                                var plcode = line[5].ToUpper();
                                if (plcode.Length > 7)
                                { plcode = plcode.Substring(0, 7); }

                                var tempvm = new PNPlannerCodeMap();
                                tempvm.PN = pn;
                                tempvm.PlannerCode = plcode;

                                if (plannerpjmap.ContainsKey(plcode))
                                {
                                    tempvm.PJName = plannerpjmap[plcode];
                                }
                                allmap.Add(pn, tempvm);
                                maplist.Add(tempvm);
                            }//end if
                        }//end if
                    }//end foreach

                    foreach (var item in maplist)
                    {
                        item.StoreData();
                    }
                }//end if

                try { File.Delete(desfile); } catch (Exception ex) { }
            }//end if
                    
        }

        private static double ConvertToDouble(string val)
        {
            try
            {
                return Math.Round(Convert.ToDouble(val), 2);
            }
            catch (Exception ex) { return 0.0; }
        }

        public static void LoadIEScrapBuget(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var now = DateTime.Now;
            var fyear = GetFYearByTime(now);
            var fquarter = GetFQuarterByTime(now);
            var srcfile = syscfg["IESCRAPBUGETPATH"] + fyear + "_" + fquarter + ".xlsx";
            var desfile = DownloadShareFile(srcfile, ctrl);
            if (!string.IsNullOrEmpty(desfile) && File.Exists(desfile))
            {
                var rawdata = RetrieveDataFromExcelWithAuth(ctrl, desfile, "Scrap WUXI");
                if (rawdata != null && rawdata.Count > 0)
                {
                    var allkeydict = IEScrapBuget.RetrieveAllKey();
                    var alldata = new List<IEScrapBuget>();

                    var idx = 0;
                    foreach (var line in rawdata)
                    {
                        if (idx == 0)
                        { idx = idx + 1; continue; }

                        var PN = line[0];
                        var CostCenter = line[2];
                        var OutPut = line[37].Replace("$", "").Replace("-", "").Trim();
                        var Scrap = line[46].Replace("$", "").Replace("-", "").Trim();
                        var dest = line[8];
                        var doutput = ConvertToDouble(OutPut);

                        if (!string.IsNullOrEmpty(PN)
                            && !string.IsNullOrEmpty(CostCenter)
                            && !string.IsNullOrEmpty(OutPut)
                            && doutput != 0.0)
                        {
                            if (string.IsNullOrEmpty(Scrap))
                            { Scrap = "0.0"; }
                            var key = PN + "_" +fyear+ "_" +fquarter;
                            var tempvm = new IEScrapBuget();
                            tempvm.DataKey = key;
                            tempvm.PN = PN;
                            tempvm.CostCenter = CostCenter;
                            tempvm.OutPut = OutPut;
                            tempvm.Scrap = Scrap;
                            tempvm.Destination = dest;
                            alldata.Add(tempvm);
                        }
                    }//end foreach

                    IEScrapBuget.UpdateData(alldata, allkeydict);

                }//end if
                try { File.Delete(desfile); } catch (Exception ex) { }
            }//end if
        }

        public static void LoadDataSample(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var srcfile = syscfg["WUXIOTSFILE"];
            var desfile = DownloadShareFile(srcfile, ctrl);
            if (!string.IsNullOrEmpty(desfile))
            {
                var colname = new List<string>();
                colname.Add("Order Type Name".ToUpper());
                colname.Add("Ordered Date".ToUpper());
                colname.Add("Customer Number".ToUpper());
                colname.Add("Customer Name".ToUpper());
                colname.Add("Cust Po Number".ToUpper());
                colname.Add("Location".ToUpper());
                colname.Add("So Number".ToUpper());
                colname.Add("Line".ToUpper());
                colname.Add("Item".ToUpper());
                colname.Add("Shipped Qty".ToUpper());

                var rawdata = RetrieveDataFromExcelWithAuthByColmnName(ctrl, desfile, colname);
                if (rawdata != null && rawdata.Count > 0)
                {
                    //solve raw data
                }
            }
        }

        #region FINANCE DATE

        public static string FQuarterByWK(string wk)
        {
            try
            {
                var wkint =Convert.ToInt32(wk.Substring(wk.Length - 2, 2));
                if (wkint >= 1 && wkint <= 13)
                {
                    return FinanceQuarter.Q1;
                }
                else if (wkint >= 14 && wkint <= 26)
                {
                    return FinanceQuarter.Q2;
                }
                else if (wkint >= 27 && wkint <= 39)
                {
                    return FinanceQuarter.Q3;
                }
                else if (wkint >= 40 && wkint <= 52)
                {
                    return FinanceQuarter.Q4;
                }
                else
                { return string.Empty; }

            }
            catch (Exception ex) { return string.Empty; }
        }

        public static string GetFQuarterByTime(DateTime sdate)
        {
            try
            {
                var sval = Convert.ToDouble(sdate.ToString("MM") + "." + sdate.ToString("dd"));
                if (sval >= 5.01 && sval <= 7.3)
                {
                    return FinanceQuarter.Q1;
                }
                else if (sval >= 7.31 && sval <= 10.29)
                {
                    return FinanceQuarter.Q2;
                }
                else if (sval >= 10.3 && sval <= 12.31)
                {
                    return FinanceQuarter.Q3;
                }
                else if (sval >= 1.01 && sval <= 1.28)
                {
                    return FinanceQuarter.Q3;
                }
                else if (sval >= 1.29 && sval <= 4.29)
                {
                    return FinanceQuarter.Q4;
                }
                else if (sval == 4.3)
                {
                    return FinanceQuarter.Q1;
                }
                else
                { return string.Empty; }

            }
            catch (Exception ex) { return string.Empty; }
        }

        public static string GetFYearByTime(DateTime sdate)
        {
            try
            {
                var sval = Convert.ToDouble(sdate.ToString("MM") + "." + sdate.ToString("dd"));
                if (sval >= 5.01 && sval <= 7.3)
                {
                    return sdate.AddYears(1).ToString("yyyy");
                }
                else if (sval >= 7.31 && sval <= 10.29)
                {
                    return sdate.AddYears(1).ToString("yyyy");
                }
                else if (sval >= 10.3 && sval <= 12.31)
                {
                    return sdate.AddYears(1).ToString("yyyy");
                }
                else if (sval >= 1.01 && sval <= 1.28)
                {
                    return sdate.ToString("yyyy");
                }
                else if (sval >= 1.29 && sval <= 4.29)
                {
                    return sdate.ToString("yyyy");
                }
                else if (sval == 4.3)
                {
                    return sdate.AddYears(1).ToString("yyyy");
                }
                else
                { return string.Empty; }

            }
            catch (Exception ex) { return string.Empty; }
        }

        #endregion

        #region FILEOPERATE
        public static string DownloadShareFile(string srcfile, Controller ctrl)
        {
            try
            {
                if (ExternalDataCollector.FileExist(ctrl, srcfile))
                {
                    var filename = System.IO.Path.GetFileName(srcfile);
                    var descfolder = ctrl.Server.MapPath("~/userfiles") + "\\docs\\ShareFile\\";
                    if (!ExternalDataCollector.DirectoryExists(ctrl, descfolder))
                        ExternalDataCollector.CreateDirectory(ctrl, descfolder);
                    var descfile = descfolder + System.IO.Path.GetFileNameWithoutExtension(srcfile) + DateTime.Now.ToString("yyyy-MM-dd") + System.IO.Path.GetExtension(srcfile);
                    ExternalDataCollector.FileCopy(ctrl, srcfile, descfile, true);
                    return descfile;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static List<List<string>> RetrieveDataFromExcelWithAuth(Controller ctrl, string filename, string sheetname = null, int columns = 101)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return ExcelReader.RetrieveDataFromExcel(filename, sheetname, columns);
                }
            }
            catch (Exception ex)
            {
                return new List<List<string>>();
            }
        }

        private static List<List<string>> RetrieveDataFromExcelWithAuthByColmnName(Controller ctrl, string filename,List<string> colname, string sheetname = null, int columns = 101)
        {
            var ret = new List<List<string>>();
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    var rawdata = ExcelReader.RetrieveDataFromExcel(filename, sheetname, columns);
                    if (rawdata.Count > 0)
                    {
                        var coldict = new Dictionary<string, int>();
                        var colidx = 0;
                        foreach (var item in rawdata[0])
                        {
                            if (!string.IsNullOrEmpty(item) && !coldict.ContainsKey(item.ToUpper()))
                            { coldict.Add(item.ToUpper(), colidx); }
                            colidx = colidx + 1;
                        }

                        foreach (var name in colname)
                        {
                            if (!coldict.ContainsKey(name))
                            {
                                logthdinfo("FILE: " + filename + " does not contains column: " + name);
                                return new List<List<string>>();
                            }
                        }

                        var rowidx = 0;
                        foreach (var line in rawdata)
                        {
                            if (rowidx == 0)
                            {
                                rowidx = rowidx + 1;
                                continue;
                            }

                            var templine = new List<string>();
                            foreach (var name in colname)
                            {
                                templine.Add(line[coldict[name]]);
                            }
                            ret.Add(templine);
                        }

                        return ret;
                    }
                    else
                    {
                        return new List<List<string>>();
                    }
                }//end using
            }
            catch (Exception ex)
            {
                return new List<List<string>>();
            }
        }

        private static bool FileExist(Controller ctrl, string filename)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return File.Exists(filename);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static void FileCopy(Controller ctrl, string src, string des, bool overwrite, bool checklocal = false)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    if (checklocal)
                    {
                        if (File.Exists(des))
                        {
                            return;
                        }
                    }

                    File.Copy(src, des, overwrite);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static void CreateDirectory(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    Directory.CreateDirectory(dirname);
                }
            }
            catch (Exception ex)
            { }

        }

        private static bool DirectoryExists(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return Directory.Exists(dirname);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static List<string> DirectoryEnumerateFiles(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    var ret = new List<string>();
                    ret.AddRange(Directory.EnumerateFiles(dirname));
                    return ret;
                }
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        private static string ConvertToDateStr(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return "1982-05-06 10:00:00";
            }
            try
            {
                return DateTime.Parse(datestr).ToString();
            }
            catch (Exception ex) { return "1982-05-06 10:00:00"; }
        }

        private static double ConvertToDoubleVal(string val)
        {
            if (string.IsNullOrEmpty(val))
                return -99999;
            try
            {
                return Convert.ToDouble(val);
            }
            catch (Exception ex)
            {
                return -99999;
            }
        }

        private static void logthdinfo(string info, string file_dir = "metis")
        {
            try
            {
                var filename = "d:\\log\\" + file_dir + "-" + DateTime.Now.ToString("yyyy-MM-dd");
                if (File.Exists(filename))
                {
                    var content = System.IO.File.ReadAllText(filename);
                    content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
                    System.IO.File.WriteAllText(filename, content);
                }
                else
                {
                    System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
                }
            }
            catch (Exception ex)
            { }

        }

        #endregion
    }

    public class NativeMethods : IDisposable
    {

        // obtains user token  

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]

        static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword,

            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);



        // closes open handes returned by LogonUser  

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]

        extern static bool CloseHandle(IntPtr handle);
        [DllImport("Advapi32.DLL")]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        [DllImport("Advapi32.DLL")]
        static extern bool RevertToSelf();
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEWCREDENTIALS = 2;

        private bool disposed;

        public NativeMethods(string sUsername, string sDomain, string sPassword)
        {

            // initialize tokens  

            IntPtr pExistingTokenHandle = new IntPtr(0);
            IntPtr pDuplicateTokenHandle = new IntPtr(0);
            try
            {
                // get handle to token  
                bool bImpersonated = LogonUser(sUsername, sDomain, sPassword,

                    LOGON32_LOGON_NEWCREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref pExistingTokenHandle);
                if (true == bImpersonated)
                {

                    if (!ImpersonateLoggedOnUser(pExistingTokenHandle))
                    {
                        int nErrorCode = Marshal.GetLastWin32Error();
                        throw new Exception("ImpersonateLoggedOnUser error;Code=" + nErrorCode);
                    }
                }
                else
                {
                    int nErrorCode = Marshal.GetLastWin32Error();
                    throw new Exception("LogonUser error;Code=" + nErrorCode);
                }

            }

            finally
            {
                // close handle(s)  
                if (pExistingTokenHandle != IntPtr.Zero)
                    CloseHandle(pExistingTokenHandle);
                if (pDuplicateTokenHandle != IntPtr.Zero)
                    CloseHandle(pDuplicateTokenHandle);
            }

        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                RevertToSelf();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public class FinanceQuarter
    {
        public static string CURRENTQx = "Current Quarter";
        public static string Q1 = "Q1";
        public static string Q2 = "Q2";
        public static string Q3 = "Q3";
        public static string Q4 = "Q4";
    }

    public class KeyValueCLA {
        public KeyValueCLA()
        {
            Key = "";
            Value = "";
        }

        public string Key { set; get; }
        public string Value { set; get; }
    }
}