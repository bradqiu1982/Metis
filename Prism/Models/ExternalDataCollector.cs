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

namespace Prism.Models
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
                                tempvm.PRODUCT_GROUP = line[17].Replace(",","");
                                tempvm.PRODUCT = line[18].Replace(",", "");
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

                    ScrapData_Base.UpdateProduct();
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

        public static Dictionary<string, string> GetCostCenterPMMap(Dictionary<string,string> costcenterpjdict = null)
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
                        {
                            tempdict[pjname].Key = colval;
                            if (costcenterpjdict != null)
                            {
                                var temppjname = pjname;
                                if (temppjname.Contains("/"))
                                {
                                    var idx = temppjname.IndexOf("/") + 1;
                                    temppjname = temppjname.Substring(idx);
                                }
                                if (!costcenterpjdict.ContainsKey(colval))
                                { costcenterpjdict.Add(colval, temppjname); }
                            }
                        }
                        else
                        { tempdict[pjname].Value = colval; }
                    }
                    else
                    {
                        var tempval = new KeyValueCLA();
                        tempdict.Add(pjname, tempval);
                        if (string.Compare(colname, "COST CENTER") == 0)
                        {
                            tempdict[pjname].Key = colval;
                            if (costcenterpjdict != null)
                            {
                                var temppjname = pjname;
                                if (temppjname.Contains("/"))
                                {
                                    var idx = temppjname.IndexOf("/") + 1;
                                    temppjname = temppjname.Substring(idx);
                                }
                                if (!costcenterpjdict.ContainsKey(colval))
                                { costcenterpjdict.Add(colval, temppjname); }
                            }
                        }
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

        private static Dictionary<string, int> RetrieveValidHPUCol(List<List<string>> maindata)
        {
            var dict = new Dictionary<string, int>();
            dict.Add("序号".ToUpper(), -1);
            dict.Add("NO.".ToUpper(), -1);

            dict.Add("HPU Code".ToUpper(), -1);

            dict.Add("产品线".ToUpper(), -1);
            dict.Add("Line".ToUpper(), -1);

            dict.Add("产品系列".ToUpper(), -1);
            dict.Add("产品系列1".ToUpper(), -1);
            dict.Add("Product".ToUpper(), -1);

            dict.Add("Customer".ToUpper(), -1);

            dict.Add("阶段".ToUpper(), -1);
            dict.Add("Phase".ToUpper(), -1);

            dict.Add("代表 PN".ToUpper(), -1);
            dict.Add("Typical PN".ToUpper(), -1);

            dict.Add("工时 量测".ToUpper(), -1);
            dict.Add("工时 整理".ToUpper(), -1);
            dict.Add("工时 签核".ToUpper(), -1);

            dict.Add("Yield HPU".ToUpper(), -1);

            dict.Add("PM".ToUpper(), -1);
            dict.Add("Owner".ToUpper(), -1);

            dict.Add("更新 日期".ToUpper(), -1);
            dict.Add("Update date".ToUpper(), -1);


            dict.Add("最近 签核".ToUpper(), -1);
            dict.Add("Sign date".ToUpper(), -1);

            dict.Add("Form Make".ToUpper(), -1);
            dict.Add("Remark".ToUpper(), -1);
            dict.Add("Family".ToUpper(), -1);
            dict.Add("是否需要拆分".ToUpper(), -1);

            var rowidx = 0;
            foreach (var line in maindata)
            {
                //var bhpucode = false;
                var bhpu = false;
                var bpn = false;
                foreach (var item in line)
                {
                    //if (string.Compare(item.Replace("\n", " ").ToUpper(), "HPU Code".ToUpper()) == 0)
                    //{ bhpucode = true; }
                    if (string.Compare(item.Replace("\n", " ").ToUpper(), "Yield HPU".ToUpper()) == 0)
                    { bhpu = true; }
                    if (string.Compare(item.Replace("\n", " ").ToUpper(), "代表 PN".ToUpper()) == 0)
                    { bpn = true; }
                    if (string.Compare(item.Replace("\n", " ").ToUpper(), "Typical PN".ToUpper()) == 0)
                    { bpn = true; }
                }

                if (bhpu && bpn)
                { break; }
                rowidx++;
            }

            if (maindata.Count > rowidx)
            {
                var colidx = 0;
                foreach (var item in maindata[rowidx])
                {
                    var key = item.Replace("\n", " ").ToUpper();
                    if (dict.ContainsKey(key))
                    { dict[key] = colidx; }
                    colidx++;
                }
            }

            return dict;
        }

        private static List<HPUMainData> RetrieveValidHPUValue(Dictionary<string, int> hpucol, List<List<string>> maindata,string fyearquarter)
        {
            var ret = new List<HPUMainData>();

            
            var hpuidx = hpucol["Yield HPU".ToUpper()];
            var pnidx = hpucol["代表 PN".ToUpper().ToUpper()];
            if (pnidx == -1) { pnidx = hpucol["Typical PN".ToUpper().ToUpper()]; }

            var vidx = 0;
            foreach (var line in maindata)
            {
                if (!string.IsNullOrEmpty(line[hpuidx])
                    && !string.IsNullOrEmpty(line[pnidx]))
                {
                    var tempvm = new HPUMainData();
                    
                    tempvm.DetailLink = line[line.Count - 1];
                    tempvm.Quarter = fyearquarter;
                    tempvm.QuarterDate = ActrualDateFromQuarter(fyearquarter);

                    tempvm.HPUOrder = vidx;

                    var hpucodeidx = hpucol["HPU Code".ToUpper()];
                    if (hpucodeidx != -1)
                    { tempvm.HPUCode = line[hpucodeidx]; }

                    var idx = (hpucol["产品线".ToUpper()] == -1) ? hpucol["Line".ToUpper()] : hpucol["产品线".ToUpper()];
                    if (idx != -1)
                    { tempvm.ProductLine = line[idx].Replace(",", ""); }

                    var searialidx = (hpucol["产品系列".ToUpper()] == -1) ? hpucol["产品系列1".ToUpper()] : hpucol["产品系列".ToUpper()];
                    searialidx = (searialidx == -1) ? hpucol["Product".ToUpper()] : searialidx;
                    if (searialidx != -1)
                    { tempvm.Serial = line[searialidx].Replace(",", ""); }

                    if (hpucol["Customer".ToUpper()] != -1)
                    { tempvm.Customer = line[hpucol["Customer".ToUpper()]]; }

                    idx = (hpucol["阶段".ToUpper()] == -1) ? hpucol["Phase".ToUpper()] : hpucol["阶段".ToUpper()];
                    if (idx != -1)
                    { tempvm.Phase = line[idx]; }

                    tempvm.TypicalPN = line[pnidx];
                    if (tempvm.TypicalPN.Contains("12:00:00 AM"))
                    {
                        tempvm.TypicalPN = ((int)DateTime.Parse(tempvm.TypicalPN).ToOADate()).ToString();
                    }

                    tempvm.PNLink = tempvm.TypicalPN+"_"+fyearquarter.Replace(" ","_");

                    if (hpucol["工时 量测".ToUpper()] != -1)
                    { tempvm.WorkingHourMeasure = line[hpucol["工时 量测".ToUpper()]]; }
                    if (hpucol["工时 整理".ToUpper()] != -1)
                    { tempvm.WorkingHourCollect = line[hpucol["工时 整理".ToUpper()]]; }
                    if (hpucol["工时 签核".ToUpper()] != -1)
                    { tempvm.WorkingHourChecked = line[hpucol["工时 签核".ToUpper()]]; }

                    tempvm.YieldHPU = line[hpuidx];

                    idx = (hpucol["PM".ToUpper()] == -1) ? hpucol["Owner".ToUpper()] : hpucol["PM".ToUpper()];
                    if (idx != -1)
                    { tempvm.Owner = line[idx]; }

                    idx = (hpucol["更新 日期".ToUpper()] == -1) ? hpucol["Update date".ToUpper()] : hpucol["更新 日期".ToUpper()];
                    if (idx != -1)
                    { tempvm.UpdateDate = line[idx]; }

                    idx = (hpucol["最近 签核".ToUpper()] == -1) ? hpucol["Sign date".ToUpper()] : hpucol["最近 签核".ToUpper()];
                    if (idx != -1)
                    { tempvm.SignDate = line[idx]; }

                    if (hpucol["Form Make".ToUpper()] != -1)
                    { tempvm.FormMake = line[hpucol["Form Make".ToUpper()]]; }
                    if (hpucol["Remark".ToUpper()] != -1)
                    { tempvm.Remark = line[hpucol["Remark".ToUpper()]]; }
                    if (hpucol["Family".ToUpper()] != -1)
                    { tempvm.Family = line[hpucol["Family".ToUpper()]]; }
                    if (hpucol["是否需要拆分".ToUpper()] != -1)
                    { tempvm.ProcessSplit = line[hpucol["是否需要拆分".ToUpper()]]; }

                    ret.Add(tempvm);
                    vidx++;
                }//end if
            }//end foreach

            return ret;
        }


        private static List<PNHPUData> RetrievePNHPU(Controller ctrl,List<HPUMainData> maindata,string desf)
        {
            var ret = new List<PNHPUData>();
            foreach (var hpudata in maindata)
            {
                if (!string.IsNullOrEmpty(hpudata.DetailLink))
                {
                    var sheetname = hpudata.DetailLink.Trim().Split(new string[] { "'" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var pnrawdata = RetrieveDataFromExcelWithAuth(ctrl, desf, sheetname,27);
                    if (pnrawdata.Count > 0)
                    { hpudata.DetailLink = hpudata.PNLink; }
                    else
                    { hpudata.DetailLink = ""; }

                    var idx = 0;
                    foreach (var line in pnrawdata)
                    {
                        var tempvm = new PNHPUData();
                        tempvm.DataOrder = idx;
                        tempvm.PNLink = hpudata.PNLink;
                        tempvm.Quarter = hpudata.Quarter;
                        tempvm.QuarterDate = hpudata.QuarterDate;

                        tempvm.A_Val = line[0];
                        tempvm.B_Val = line[1];
                        tempvm.C_Val = line[2];
                        tempvm.D_Val = line[3];
                        tempvm.E_Val = line[4];
                        tempvm.F_Val = line[5];
                        tempvm.G_Val = line[6];
                        tempvm.H_Val = line[7];
                        tempvm.I_Val = line[8];
                        tempvm.J_Val = line[9];
                        tempvm.K_Val = line[10];
                        tempvm.L_Val = line[11];
                        tempvm.M_Val = line[12];
                        tempvm.N_Val = line[13];
                        tempvm.O_Val = line[14];
                        tempvm.P_Val = line[15];
                        tempvm.Q_Val = line[16];
                        tempvm.R_Val = line[17];
                        tempvm.S_Val = line[18];
                        tempvm.T_Val = line[19];
                        tempvm.U_Val = line[20];
                        tempvm.V_Val = line[21];
                        tempvm.W_Val = line[22];
                        tempvm.X_Val = line[23];
                        tempvm.Y_Val = line[24];
                        tempvm.Z_Val = line[25];

                        ret.Add(tempvm);

                        idx++;
                    }
                }
            }//end foreach

            return ret;
        }
        
        public static void LoadIEHPU(Controller ctrl,string defaultquarter = null)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var datafolder = syscfg["HPUSRCFOLDER"];
            var srcfilters = syscfg["HPUPRODUCTLINE"].Split(new string[] { ";" },StringSplitOptions.RemoveEmptyEntries).ToList();

            string fyearquarter = "";
            if (defaultquarter == null)
            {
                var now = DateTime.Now;
                fyearquarter = GetFYearByTime(now) + " "+GetFQuarterByTime(now);
            }
            else
            {
                fyearquarter = defaultquarter;
            }

            var pnlinkmap = HPUMainData.RetrieveAllPNLink();

            var srcfiles = DirectoryEnumerateFiles(ctrl,datafolder);
            foreach (var src in srcfiles)
            {
                var passfilter = false;
                foreach (var f in srcfilters)
                {
                    if (src.ToUpper().Contains(f.ToUpper()))
                    {
                        passfilter = true;
                        break;
                    }
                }

                if (!passfilter)
                { continue; }

                var desf = DownloadShareFile(src, ctrl);
                if (desf != null && System.IO.File.Exists(desf))
                {
                    var maindata = RetrieveDataFromExcelWithAuth(ctrl, desf, "目录",101,true);
                    if (maindata.Count > 0)
                    {
                        var hpucol =  RetrieveValidHPUCol(maindata);
                        
                        //var hpucodeidx = hpucol["HPU Code".ToUpper()];
                        var hpuidx = hpucol["Yield HPU".ToUpper()];
                        var pnidx = hpucol["代表 PN".ToUpper().ToUpper()];
                        if (pnidx == -1) { pnidx = hpucol["Typical PN".ToUpper().ToUpper()]; }
                        if ( hpuidx == -1 || pnidx ==  -1)
                        { continue; }

                        var HPUDataList =  RetrieveValidHPUValue(hpucol,maindata, fyearquarter);
                            
                        if (HPUDataList.Count > 1)
                        {
                            HPUDataList.RemoveAt(0);
                        }
                        var PNHPUDatalist = RetrievePNHPU(ctrl,HPUDataList,desf);

                        foreach (var data in HPUDataList)
                        {
                            if (pnlinkmap.ContainsKey(data.PNLink))
                            {
                                data.UpdateData();
                            }
                            else
                            {
                                data.StoreData();
                            }
                        }

                        var cleardict = new Dictionary<string, bool>();

                        foreach (var data in PNHPUDatalist)
                        {
                            if (pnlinkmap.ContainsKey(data.PNLink))
                            {
                                if (!cleardict.ContainsKey(data.PNLink))
                                {
                                    cleardict.Add(data.PNLink,true);
                                    PNHPUData.CleanData(data.PNLink);
                                }
                            }

                            data.StoreData();
                        }
                    }//end if
                    try { System.IO.File.Delete(desf); } catch (Exception ex) { }
                }
            }//end foreach
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

        public static DateTime ActrualDateFromQuarter(string fyearquarter)
        {
            string fyear = fyearquarter.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0];
            string fquarter = fyearquarter.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];

            if (string.Compare(fquarter, FinanceQuarter.Q1, true) == 0)
            {
                return DateTime.Parse((Convert.ToInt32(fyear) - 1).ToString() + "-05-01 01:00:00");
            }
            else if (string.Compare(fquarter, FinanceQuarter.Q2, true) == 0)
            {
                return DateTime.Parse((Convert.ToInt32(fyear) - 1).ToString() + "-08-01 01:00:00");
            }
            else if (string.Compare(fquarter, FinanceQuarter.Q3, true) == 0)
            {
                return DateTime.Parse((Convert.ToInt32(fyear) - 1).ToString() + "-11-01 01:00:00");
            }
            else
            {
                return DateTime.Parse(fyear + "-03-01 01:00:00");
            }
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

        public static List<List<string>> RetrieveDataFromExcelWithAuth(Controller ctrl, string filename, string sheetname = null, int columns = 101, bool getlink=false)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return ExcelReader.RetrieveDataFromExcel(filename, sheetname, columns, getlink);
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

        public static List<string> DirectoryEnumerateFiles(Controller ctrl, string dirname)
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

        private static void logthdinfo(string info, string file_dir = "Prism")
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

    public class KeyDateCLA
    {
        public KeyDateCLA()
        {
            Key = "";
            Value = DateTime.Now;
        }

        public string Key { set; get; }
        public DateTime Value { set; get; }
    }

}