using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
            var sql = @"update ProductCostVM set PM=@PM,ProcessHPU=@ProcessHPU,Yield=@Yield,LobEff=@LobEff,OralceHPU=@OralceHPU,BOM=@BOM
                        ,LabFOther=@LabFOther,OverheadFOther=@OverheadFOther,DLFG=@DLFG,DLSFG=@DLSFG,SMFG=@SMFG,SMSFG=@SMSFG,IMFG=@IMFG
                        ,IMSFG=@IMSFG,VairableCost=@VairableCost,DOHFG=@DOHFG,DOHSFG=@DOHSFG,IOHFG=@IOHFG,IOHSFG=@IOHSFG,IOHSNYFG=@IOHSNYFG
                        ,IOHSNYSFG=@IOHSNYSFG,UMCost=@UMCost,Qty=@Qty,ASP=@ASP,UpdateTime=@UpdateTime where PN=@PN and QuarterType=@QuarterType";
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
    }
}