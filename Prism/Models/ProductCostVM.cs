using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prism.Models
{
    public class ProductCostVM
    {
        public static List<string> GetCostFields()
        {
            var ret = new List<string>();
            ret.Add("QuarterType");
            ret.Add("ProcessHPU");
            ret.Add("Yield");
            ret.Add("LobEff");
            ret.Add("OralceHPU");
            ret.Add("BOM");
            ret.Add("LabFOther");
            ret.Add("OverheadFOther");
            ret.Add("DLFG");
            ret.Add("DLSFG");
            ret.Add("SMFG");
            ret.Add("SMSFG");
            ret.Add("IMFG");
            ret.Add("IMSFG");
            ret.Add("VairableCost");
            ret.Add("DOHFG");
            ret.Add("DOHSFG");
            ret.Add("IOHFG");
            ret.Add("IOHSFG");
            ret.Add("IOHSNYFG");
            ret.Add("IOHSNYSFG");
            ret.Add("UMCost");
            ret.Add("Qty");
            ret.Add("ASP");
            return ret;
        }

        public static Dictionary<string, string> GetCostFieldNameMap()
        {
            var ret = new Dictionary<string, string>();
            ret.Add("QuarterType", "Unit_Cost");
            ret.Add("ProcessHPU", "Process_HPU");
            ret.Add("Yield", "Yield");
            ret.Add("LobEff", "Labor_EF");
            ret.Add("OralceHPU", "Oracle_HPU");
            ret.Add("BOM", "BOM");
            ret.Add("LabFOther", "Labor_FOS");
            ret.Add("OverheadFOther", "Overhead_FOS");
            ret.Add("DLFG", "DL_FG");
            ret.Add("DLSFG", "DL_SFG");
            ret.Add("SMFG", "SM_FG");
            ret.Add("SMSFG", "SM_SFG");
            ret.Add("IMFG", "IM_FG");
            ret.Add("IMSFG", "IM_SFG");
            ret.Add("VairableCost", "Variable_Cost");
            ret.Add("DOHFG", "DOH_FG");
            ret.Add("DOHSFG", "DOH_SFG");
            ret.Add("IOHFG", "IOH_FG");
            ret.Add("IOHSFG", "IOH_SFG");
            ret.Add("IOHSNYFG", "IOH_SNY_FG");
            ret.Add("IOHSNYSFG", "IOH_SNY_SFG");
            ret.Add("UMCost", "Unit_Mfg_Cost");
            ret.Add("Qty", "Qty_Built");
            ret.Add("ASP", "ASP");
            return ret;
        }

        public static List<string> PMList()
        {
            var ret = new List<string>();
            var sql = "select distinct PM from ProductCostVM";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(UT.O2S(line[0])); }
            return ret;
        }

        public static List<string> PNList(string pm=null)
        {
            var ret = new List<string>();
            var sql = "select distinct PN from ProductCostVM";

            if(pm != null)
            {
                sql = sql + " where PM = '"+pm.Trim()+"'";
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(UT.O2S(line[0])); }
            return ret;
        }

        public static List<string> GetPMByPN(string PN)
        {
            var ret = new List<string>();
            var sql = "select distinct PM from ProductCostVM where PN = @PN";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null, dict);
            foreach (var line in dbret)
            { ret.Add(UT.O2S(line[0])); }
            return ret;
        }

        public static Dictionary<string,List<ProductCostVM>> GetProdctCostData(string PM, List<string> PNLIST)
        {
            var ret = new Dictionary<string, List<ProductCostVM>>();

            var sql = @"select PN,PM,QuarterType,ProcessHPU,Yield,LobEff,OralceHPU,BOM
                        ,LabFOther,OverheadFOther,DLFG,DLSFG,SMFG,SMSFG,IMFG,IMSFG,VairableCost,DOHFG,DOHSFG
                        ,IOHFG,IOHSFG,IOHSNYFG,IOHSNYSFG,UMCost,Qty,ASP from ProductCostVM ";
            var dict = new Dictionary<string, string>();

            if (PNLIST.Count > 0)
            {
                sql = sql + " where PN in ('"+string.Join("','",PNLIST)+"') order by PN,UpdateTime,DataType";
            }
            else  if (!string.IsNullOrEmpty(PM))
            {
                sql = sql + " where PM=@PM  order by PN,UpdateTime,DataType";
                dict.Add("@PM", PM);
            }
            else
            { return ret; }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new ProductCostVM();
                tempvm.PN = UT.O2S(line[0]);
                tempvm.PM = UT.O2S(line[1]);
                tempvm.QuarterType = UT.O2S(line[2]);
                tempvm.ProcessHPU = UT.O2S(line[3]);
                tempvm.Yield = UT.O2S(line[4]);
                tempvm.LobEff = UT.O2S(line[5]);
                tempvm.OralceHPU = UT.O2S(line[6]);
                tempvm.BOM = UT.O2S(line[7]);
                tempvm.LabFOther = UT.O2S(line[8]);
                tempvm.OverheadFOther = UT.O2S(line[9]);
                tempvm.DLFG = UT.O2S(line[10]);
                tempvm.DLSFG = UT.O2S(line[11]);
                tempvm.SMFG = UT.O2S(line[12]);
                tempvm.SMSFG = UT.O2S(line[13]);
                tempvm.IMFG = UT.O2S(line[14]);
                tempvm.IMSFG = UT.O2S(line[15]);
                tempvm.VairableCost = UT.O2S(line[16]);
                tempvm.DOHFG = UT.O2S(line[17]);
                tempvm.DOHSFG = UT.O2S(line[18]);
                tempvm.IOHFG = UT.O2S(line[19]);
                tempvm.IOHSFG = UT.O2S(line[20]);
                tempvm.IOHSNYFG = UT.O2S(line[21]);
                tempvm.IOHSNYSFG = UT.O2S(line[22]);
                tempvm.UMCost = UT.O2S(line[23]);
                tempvm.Qty = UT.O2S(line[24]);
                tempvm.ASP = UT.O2S(line[25]);

                if (ret.ContainsKey(tempvm.PN))
                {
                    ret[tempvm.PN].Add(tempvm);
                }
                else
                {
                    var templist = new List<ProductCostVM>();
                    templist.Add(tempvm);
                    ret.Add(tempvm.PN, templist);
                }
            }
            return ret;
        }

        public static  List<ProductCostVM> GetOneProdctCostData(string pn, string qt,string datatype)
        {
            var ret = new List<ProductCostVM>();

            var sql = @"select PN,PM,QuarterType,ProcessHPU,Yield,LobEff,OralceHPU,BOM
                        ,LabFOther,OverheadFOther,DLFG,DLSFG,SMFG,SMSFG,IMFG,IMSFG,VairableCost,DOHFG,DOHSFG
                        ,IOHFG,IOHSFG,IOHSNYFG,IOHSNYSFG,UMCost,Qty,ASP from ProductCostVM where PN=@PN and Quarter=@Quarter and DataType=@DataType";

            var dict = new Dictionary<string, string>();
            dict.Add("@PN",pn);
            dict.Add("@Quarter",qt);
            dict.Add("@DataType",datatype);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new ProductCostVM();
                tempvm.PN = UT.O2S(line[0]);
                tempvm.PM = UT.O2S(line[1]);
                tempvm.QuarterType = UT.O2S(line[2]);
                tempvm.ProcessHPU = UT.O2S(line[3]);
                tempvm.Yield = UT.O2S(line[4]);
                tempvm.LobEff = UT.O2S(line[5]);
                tempvm.OralceHPU = UT.O2S(line[6]);
                tempvm.BOM = UT.O2S(line[7]);
                tempvm.LabFOther = UT.O2S(line[8]);
                tempvm.OverheadFOther = UT.O2S(line[9]);
                tempvm.DLFG = UT.O2S(line[10]);
                tempvm.DLSFG = UT.O2S(line[11]);
                tempvm.SMFG = UT.O2S(line[12]);
                tempvm.SMSFG = UT.O2S(line[13]);
                tempvm.IMFG = UT.O2S(line[14]);
                tempvm.IMSFG = UT.O2S(line[15]);
                tempvm.VairableCost = UT.O2S(line[16]);
                tempvm.DOHFG = UT.O2S(line[17]);
                tempvm.DOHSFG = UT.O2S(line[18]);
                tempvm.IOHFG = UT.O2S(line[19]);
                tempvm.IOHSFG = UT.O2S(line[20]);
                tempvm.IOHSNYFG = UT.O2S(line[21]);
                tempvm.IOHSNYSFG = UT.O2S(line[22]);
                tempvm.UMCost = UT.O2S(line[23]);
                tempvm.Qty = UT.O2S(line[24]);
                tempvm.ASP = UT.O2S(line[25]);

                ret.Add(tempvm);
            }
            return ret;
        }

        public void StoreData()
        {
            if (HasData())
            {
                var datadate = QuarterCLA.RetrieveDateFromQuarter(Quarter);
                if (datadate[0] > DateTime.Now)
                {
                    UpdateData();
                }
                else {
                    var currentq = QuarterCLA.RetrieveQuarterFromDate(DateTime.Now);
                    if (string.Compare(currentq, Quarter, true) == 0)
                    {
                        UpdateData();
                    }
                }
            }
            else
            {
                FirstTimesComingData = true;
                InsertData();
            }
        }

        private bool HasData()
        {
            var dict = new Dictionary<string, string>();
            var sql = "select PN,PM,QuarterType from ProductCostVM where PN=@PN and QuarterType=@QuarterType";
            dict.Add("@PN", PN);
            dict.Add("@QuarterType", QuarterType);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null, dict);
            if (dbret.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InsertData()
        {
            var dict = new Dictionary<string, string>();

            var sql = @"insert into ProductCostVM(PN,PM,QuarterType,Quarter,DataType,ProcessHPU,Yield,LobEff,OralceHPU,BOM
                                                ,LabFOther,OverheadFOther,DLFG,DLSFG,SMFG,SMSFG,IMFG,IMSFG,VairableCost,DOHFG
                                                ,DOHSFG,IOHFG,IOHSFG,IOHSNYFG,IOHSNYSFG,UMCost,Qty,ASP,UpdateTime) 
                        values(@PN,@PM,@QuarterType,@Quarter,@DataType,@ProcessHPU,@Yield,@LobEff,@OralceHPU,@BOM,@LabFOther
                        ,@OverheadFOther,@DLFG,@DLSFG,@SMFG,@SMSFG,@IMFG,@IMSFG,@VairableCost,@DOHFG,@DOHSFG,@IOHFG,@IOHSFG
                        ,@IOHSNYFG,@IOHSNYSFG,@UMCost,@Qty,@ASP,@UpdateTime)";

            dict.Add("@PN", PN);
            dict.Add("@PM", PM);
            dict.Add("@QuarterType", QuarterType);
            dict.Add("@Quarter", Quarter);
            dict.Add("@DataType", DataType);
            dict.Add("@ProcessHPU", ProcessHPU);
            dict.Add("@Yield", Yield);
            dict.Add("@LobEff", LobEff);
            dict.Add("@OralceHPU", OralceHPU);
            dict.Add("@BOM", BOM);
            dict.Add("@LabFOther", LabFOther);
            dict.Add("@OverheadFOther", OverheadFOther);
            dict.Add("@DLFG", DLFG);
            dict.Add("@DLSFG", DLSFG);
            dict.Add("@SMFG", SMFG);
            dict.Add("@SMSFG", SMSFG);
            dict.Add("@IMFG", IMFG);
            dict.Add("@IMSFG", IMSFG);
            dict.Add("@VairableCost", VairableCost);
            dict.Add("@DOHFG", DOHFG);
            dict.Add("@DOHSFG", DOHSFG);
            dict.Add("@IOHFG", IOHFG);
            dict.Add("@IOHSFG", IOHSFG);
            dict.Add("@IOHSNYFG", IOHSNYFG);
            dict.Add("@IOHSNYSFG", IOHSNYSFG);
            dict.Add("@UMCost", UMCost);
            dict.Add("@Qty", Qty);
            dict.Add("@ASP", ASP);
            dict.Add("@UpdateTime", UpdateTime);

            DBUtility.ExeLocalSqlNoRes(sql, dict);

        }

        private void UpdateData()
        {
            var dict = new Dictionary<string, string>();
            var sql = @"update ProductCostVM set PM=@PM,ProcessHPU=@ProcessHPU,LobEff=@LobEff,OralceHPU=@OralceHPU,BOM=@BOM
                        ,LabFOther=@LabFOther,OverheadFOther=@OverheadFOther,DLFG=@DLFG,DLSFG=@DLSFG,SMFG=@SMFG,SMSFG=@SMSFG,IMFG=@IMFG
                        ,IMSFG=@IMSFG,VairableCost=@VairableCost,DOHFG=@DOHFG,DOHSFG=@DOHSFG,IOHFG=@IOHFG,IOHSFG=@IOHSFG,IOHSNYFG=@IOHSNYFG
                        ,IOHSNYSFG=@IOHSNYSFG,UMCost=@UMCost,Qty=@Qty,UpdateTime=@UpdateTime where PN=@PN and QuarterType=@QuarterType";
            dict.Add("@PN", PN);
            dict.Add("@PM", PM);
            dict.Add("@QuarterType", QuarterType);
            dict.Add("@Quarter", Quarter);
            dict.Add("@DataType", DataType);
            dict.Add("@ProcessHPU", ProcessHPU);
            //dict.Add("@Yield", Yield);
            dict.Add("@LobEff", LobEff);
            dict.Add("@OralceHPU", OralceHPU);
            dict.Add("@BOM", BOM);
            dict.Add("@LabFOther", LabFOther);
            dict.Add("@OverheadFOther", OverheadFOther);
            dict.Add("@DLFG", DLFG);
            dict.Add("@DLSFG", DLSFG);
            dict.Add("@SMFG", SMFG);
            dict.Add("@SMSFG", SMSFG);
            dict.Add("@IMFG", IMFG);
            dict.Add("@IMSFG", IMSFG);
            dict.Add("@VairableCost", VairableCost);
            dict.Add("@DOHFG", DOHFG);
            dict.Add("@DOHSFG", DOHSFG);
            dict.Add("@IOHFG", IOHFG);
            dict.Add("@IOHSFG", IOHSFG);
            dict.Add("@IOHSNYFG", IOHSNYFG);
            dict.Add("@IOHSNYSFG", IOHSNYSFG);
            dict.Add("@UMCost", UMCost);
            dict.Add("@Qty", Qty);
            //dict.Add("@ASP", ASP);
            dict.Add("@UpdateTime", UpdateTime);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void CreateEPCost(string qart,string eppn,double prochpu,double epyield
            ,double eplab,double epbom,double eplabfos,double epoverheadfos,double epqty,double epasp,ProductCostVM crtfcost)
        {
            var eporaclehpu = prochpu / eplab * 1.075;
            var dlfgrate = UT.O2D(crtfcost.DLFG) / UT.O2D(crtfcost.OralceHPU);
            var dlsfgrate = UT.O2D(crtfcost.DLSFG) / UT.O2D(crtfcost.OralceHPU);
            var smfgrate = UT.O2D(crtfcost.SMFG) / UT.O2D(crtfcost.OralceHPU);
            var smsfgrate = UT.O2D(crtfcost.SMSFG) / UT.O2D(crtfcost.OralceHPU);
            var imfgrate = UT.O2D(crtfcost.IMFG) / UT.O2D(crtfcost.OralceHPU);
            var imsfgrate = UT.O2D(crtfcost.IMSFG) / UT.O2D(crtfcost.OralceHPU);

            var dohfgrate = UT.O2D(crtfcost.DOHFG) / UT.O2D(crtfcost.OralceHPU);
            var dohsfgrate = UT.O2D(crtfcost.DOHSFG) / UT.O2D(crtfcost.OralceHPU);
            var iohfgrate = UT.O2D(crtfcost.IOHFG) / UT.O2D(crtfcost.OralceHPU);
            var iohsfgrate = UT.O2D(crtfcost.IOHSFG) / UT.O2D(crtfcost.OralceHPU);
            var iohsnyfgrate = UT.O2D(crtfcost.IOHSNYFG) / UT.O2D(crtfcost.OralceHPU);
            var iohsnysfgrate = UT.O2D(crtfcost.IOHSNYSFG) / UT.O2D(crtfcost.OralceHPU);

            var DLFG = eporaclehpu * dlfgrate;
            var DLSFG = eporaclehpu * dlsfgrate;
            var SMFG = eporaclehpu * smfgrate * (UT.O2D(crtfcost.Yield) / epyield);
            var SMSFG = eporaclehpu * smsfgrate;
            var IMFG = eporaclehpu * imfgrate;
            var IMSFG = eporaclehpu * imsfgrate;

            var variablecost = epbom + eplabfos + DLFG + DLSFG + SMFG + SMSFG + IMFG + IMSFG;

            var DOHFG = eporaclehpu * dohfgrate;
            var DOHSFG = eporaclehpu * dohsfgrate;
            var IOHFG = eporaclehpu * iohfgrate;
            var IOHSFG = eporaclehpu * iohsfgrate;
            var IOHSNYFG = eporaclehpu * iohsnyfgrate;
            var IOHSNYSFG = eporaclehpu * iohsnysfgrate;

            var UMFCost = variablecost + epoverheadfos + DOHFG + DOHSFG + IOHFG + IOHSFG + IOHSNYFG + IOHSNYSFG;

            var quartertype = qart.Substring(5) + qart.Substring(2, 2) + " (EP) WUXI";

            var tempvm = new ProductCostVM(eppn, crtfcost.PM, quartertype, prochpu.ToString(), epyield.ToString()
               , eplab.ToString(), eporaclehpu.ToString(), epbom.ToString(), eplabfos.ToString(), epoverheadfos.ToString()
               , DLFG.ToString(), DLSFG.ToString(), SMFG.ToString(), SMSFG.ToString(), IMFG.ToString()
               , IMSFG.ToString(), variablecost.ToString(), DOHFG.ToString(), DOHSFG.ToString(), IOHFG.ToString()
               , IOHSFG.ToString(), IOHSNYFG.ToString(), IOHSNYSFG.ToString(), UMFCost.ToString(), epqty.ToString(), epasp.ToString());
            tempvm.StoreData();
        }

        private static bool UpdateFCost(string pn,string pm,string qtype,FCostModel vm)
        {
            var tempvm = new ProductCostVM(pn, pm, qtype, vm.ProcessHPU.ToString(), (0.85).ToString()
                       , vm.LabEff.ToString(), vm.OralceHPU.ToString(), vm.BOM.ToString(), vm.LabFOther.ToString(), vm.OverheadFOther.ToString()
                       , vm.DLFG.ToString(), vm.DLSFG.ToString(), vm.SMFG.ToString(), vm.SMSFG.ToString(), vm.IMFG.ToString()
                       , vm.IMSFG.ToString(), vm.VairableCost.ToString(), vm.DOHFG.ToString(), vm.DOHSFG.ToString(), vm.IOHFG.ToString()
                       , vm.IOHSFG.ToString(), vm.IOHSNYFG.ToString(), vm.IOHSNYSFG.ToString(), vm.UMCost.ToString(), vm.Qty.ToString(), (vm.UMCost*1.3).ToString());
            tempvm.StoreData();
            return tempvm.FirstTimesComingData;
        }

        public static void SendAddingEPNotice(string pm, string pn, string CFQ, Controller ctrl)
        {
            try
            {
                var syscfg = CfgUtility.GetSysConfig(ctrl);
                var tolist = new List<string>();
                if (syscfg.ContainsKey("EPNOTICELIST"))
                {
                    tolist.AddRange(syscfg["EPNOTICELIST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList());
                }
                tolist.Add(pm.Replace(" ", ".") + "@finisar.com");

                var routevalue = new RouteValueDictionary();
                routevalue.Add("pn", pn);
                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string url = ctrl.Url.Action("ProductCost", "Inventory", routevalue, scheme);

                var netcomputername = EmailUtility.RetrieveCurrentMachineName();
                url = url.Replace("//localhost", "//" + netcomputername);
                var econtent = EmailUtility.CreateTableHtml("Hi Cost Maintainer", "The financial cost forcast for pn - "+pn+" is updated,please go to below link to add your EP for next 3 quarters:", url, null);
                EmailUtility.SendEmail(ctrl, "Adding Cost EP Notice", tolist, econtent);
                new System.Threading.ManualResetEvent(false).WaitOne(500);
            }
            catch (Exception ex) { }
        }

        public static void RefreshFCost(Controller ctrl)
        {
            var dayinmonth = DateTime.Now.Day;
            if (dayinmonth >= 4 && dayinmonth <= 27)
            {
                var pnlist = ProductCostVM.PNList();

                var Q = QuarterCLA.RetrieveQuarterFromDate(DateTime.Now);
                var FQ = Q.Substring(5, 2) + "FY" + Q.Substring(2, 2);
                foreach (var pn in pnlist)
                {
                    var pm = ProductCostVM.GetPMByPN(pn)[0];

                    var costdata = FCostModel.LoadDataFromFDB(FQ, pn);
                    if (costdata["F"].IsOK && costdata["F"].ProcessHPU != 0)
                    {
                        string qtype = FQ.Replace("FY", "") + " (F) WUXI";
                        var firsttimes = ProductCostVM.UpdateFCost(pn, pm, qtype, costdata["F"]);
                        if (firsttimes)
                        {
                            ProductCostVM.SendAddingEPNotice(pm, pn, FQ, ctrl);
                        }
                    }

                    if (costdata["FM"].IsOK && costdata["FM"].ProcessHPU != 0)
                    {
                        var qtype = FQ.Replace("FY", "") + " (F)Material Update";
                        ProductCostVM.UpdateFCost(pn, pm, qtype, costdata["FM"]);
                    }

                    if (costdata["A"].IsOK && costdata["A"].ProcessHPU != 0)
                    {
                        var qtype = FQ.Replace("FY", "") + " (A) WUXI";
                        ProductCostVM.UpdateFCost(pn, pm, qtype, costdata["A"]);
                    }

                }//end foreach
            }//end if
        }

        public static bool AddFCostByPN(string pm, string pn,Controller ctrl)
        {
            var CQ = QuarterCLA.RetrieveQuarterFromDate(DateTime.Now);
            var CFQ = CQ.Substring(5, 2) + "FY" + CQ.Substring(2, 2);

            var hasdata = false;
            var qlist = QuarterCLA.GetQuerterFrom19Q3();
            foreach (var q in qlist)
            {
                var costdata = FCostModel.LoadDataFromFDB(q, pn);
                if (costdata["F"].IsOK && costdata["F"].ProcessHPU != 0)
                {
                    hasdata = true;
                    string qtype = q.Replace("FY", "") + " (F) WUXI";
                    var firsttimes = ProductCostVM.UpdateFCost(pn, pm, qtype, costdata["F"]);
                    if (string.Compare(q, CFQ, true) == 0 && firsttimes)
                    {
                        ProductCostVM.SendAddingEPNotice(pm, pn, CFQ, ctrl);
                    }

                    qtype = q.Replace("FY", "") + " (EP) WUXI";
                    ProductCostVM.UpdateFCost(pn, pm, qtype, costdata["F"]);
                }

                if (costdata["FM"].IsOK && costdata["FM"].ProcessHPU != 0)
                {
                    hasdata = true;
                    var qtype = q.Replace("FY", "") + " (F)Material Update";
                    ProductCostVM.UpdateFCost(pn, pm, qtype, costdata["FM"]);
                }

                if (costdata["A"].IsOK && costdata["A"].ProcessHPU != 0)
                {
                    hasdata = true;
                    var qtype = q.Replace("FY", "") + " (A) WUXI";
                    ProductCostVM.UpdateFCost(pn, pm, qtype, costdata["A"]);
                }
            }//end foreach

            return hasdata;
        }


        public ProductCostVM()
        {
            PN = "";
            PM = "";
            QuarterType = "";
            ProcessHPU = "";
            Yield = "";
            LobEff = "";
            OralceHPU = "";
            BOM = "";
            LabFOther = "";
            OverheadFOther = "";
            DLFG = "";
            DLSFG = "";
            SMFG = "";
            SMSFG = "";
            IMFG = "";
            IMSFG = "";
            VairableCost = "";
            DOHFG = "";
            DOHSFG = "";
            IOHFG = "";
            IOHSFG = "";
            IOHSNYFG = "";
            IOHSNYSFG = "";
            UMCost = "";
            Qty = "";
            ASP = "";

            AppVal1 = "";
            AppVal2 = "";
            AppVal3 = "";
            AppVal4 = "";
            AppVal5 = "";
            FirstTimesComingData = false;
        }

        public ProductCostVM(string pn,string pm,string qt,string prochpu,string yield
               ,string lobeff,string oraclehpu,string bom,string labfos,string overfos
               ,string dlfg,string dlsfg,string smfg,string smsfg,string imfg
               ,string imsfg,string variablecost,string dohfg,string dohsfg,string iohfg
               ,string iohsfg,string iohsnyfg,string iohsnysfg,string umcost,string qty,string asp)
        {
            PN = pn;
            PM = pm;
            QuarterType = qt;
            ProcessHPU = prochpu;
            Yield = yield;

            LobEff = lobeff;
            OralceHPU = oraclehpu;
            BOM = bom;
            LabFOther = labfos;
            OverheadFOther = overfos;

            DLFG = dlfg;
            DLSFG = dlsfg;
            SMFG = smfg;
            SMSFG = smsfg;
            IMFG = imfg;

            IMSFG = imsfg;
            VairableCost = variablecost;
            DOHFG = dohfg;
            DOHSFG = dohsfg;
            IOHFG = iohfg;

            IOHSFG = iohsfg;
            IOHSNYFG = iohsnyfg;
            IOHSNYSFG = iohsnysfg;
            UMCost = umcost;
            Qty = qty;

            ASP = asp;

            FirstTimesComingData = false;
        }

        public string PN { set; get; }
        public string PM { set; get; }
        public string QuarterType { set; get; }
        public string Quarter { get {
                return "20" + QuarterType.Substring(2, 2) + " " + QuarterType.Substring(0, 2);
            } }
        public string DataType {
            get {
                var items = QuarterType.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
                var type = items[1].ToUpper();
                if (string.Compare(type, "F") == 0)
                {
                    if (QuarterType.ToUpper().Contains("MATE"))
                    { return "3FM"; }
                    else
                    { return "2F"; }
                }
                else if(string.Compare(type,"EP")==0)
                {
                    return "1EP";
                }
                else
                {
                    return "4A";
                }
            }
        }

        public string ProcessHPU { set; get; }
        public string Yield { set; get; }
        public string LobEff { set; get; }
        public string OralceHPU { set; get; }
        public string BOM { set; get; }
        public string LabFOther { set; get; }
        public string OverheadFOther { set; get; }
        public string DLFG { set; get; }
        public string DLSFG { set; get; }
        public string SMFG { set; get; }
        public string SMSFG { set; get; }
        public string IMFG { set; get; }
        public string IMSFG { set; get; }
        public string VairableCost { set; get; }
        public string DOHFG { set; get; }
        public string DOHSFG { set; get; }
        public string IOHFG { set; get; }
        public string IOHSFG { set; get; }
        public string IOHSNYFG { set; get; }
        public string IOHSNYSFG { set; get; }
        public string UMCost { set; get; }
        public string Qty { set; get; }
        public string ASP { set; get; }
        public string UpdateTime { get {
                return QuarterCLA.RetrieveDateFromQuarter(Quarter)[0].ToString("yyyy-MM-dd HH:mm:ss");
            } }

        public string AppVal1 { set; get; }
        public string AppVal2 { set; get; }
        public string AppVal3 { set; get; }
        public string AppVal4 { set; get; }
        public string AppVal5 { set; get; }
        public bool FirstTimesComingData { set; get; }
    }
}