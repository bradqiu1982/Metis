using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class CostPoolType
    {
        public static string FM = "F_M";
        public static string ACTURAL = "Actual";
    }

    public class HPUType
    {
        public static string ProcHPU = "Process HPU";
        public static string OracleHPU = "Oracle HPU";
    }

    public class BOMCLA
    {
        public static string BOM = "BOM";
        public static string LABOS = "Labor from other sites";
        public static string OFOS = "Overhead from other sites";
        public static string OSP = "OSP";
    }

    public class FGLEVEL
    {
        public static string FG = "FG";
        public static string SFG = "SFG";
    }

    public class FCostModel
    {
        public static Dictionary<string,FCostModel> LoadDataFromFDB(string quarter,string pn)
        {
            var ret = new Dictionary<string, FCostModel>();
            ret.Add("F", new FCostModel());
            ret.Add("FM", new FCostModel());
            ret.Add("A", new FCostModel());

            var ok = GetSupportDL(quarter, ret);
            if (!ok) { return ret; }

            ok = GetHPU(quarter,pn,HPUType.ProcHPU, ret);
            if (!ok) { return ret; }

            ok = GetHPU(quarter,pn,HPUType.OracleHPU, ret);
            if (!ok) { return ret; }

            GetLabEff(ret);

            ok = GetBOMs(quarter, pn, BOMCLA.BOM, ret);
            if (!ok) { return ret; }

            ok = GetBOMs(quarter, pn, BOMCLA.LABOS, ret);
            if (!ok) { return ret; }

            ok = GetBOMs(quarter, pn, BOMCLA.OFOS, ret);
            if (!ok) { return ret; }

            ok = GetBOMs(quarter, pn, BOMCLA.OSP, ret);
            if (!ok) { return ret; }

            ok = GetDLs(quarter, pn, FGLEVEL.FG, CostPoolType.FM, ret);
            //if (!ok) { return ret; }
            ok = GetDLs(quarter, pn, FGLEVEL.SFG, CostPoolType.FM, ret);
            //if (!ok) { return ret; }

            ok = GetDLs(quarter, pn, FGLEVEL.FG, CostPoolType.ACTURAL, ret);
            //if (!ok) { return ret; }
            ok = GetDLs(quarter, pn, FGLEVEL.SFG, CostPoolType.ACTURAL, ret);
            //if (!ok) { return ret; }

            GetQTY(quarter, pn, ret);
            GetVariableCost(ret);
            GetUnitMfgCost(ret);

            return ret;
        }

        private static bool GetSupportDL(string q, Dictionary<string, FCostModel> cd)
        {
            var sql = @"SELECT [Supporting DL]  FROM [WebFinance].[dbo].[SupportingDL] where [Quarter]= @quarter";
            var dict = new Dictionary<string, string>();
            dict.Add("@quarter", q);
            var dbret = DBUtility.ExeFSqlWithRes(sql, dict);
            if (dbret.Count == 0)
            {
                SetNoOK(cd, "No Support DL");
                return false;
            }

            foreach (var line in dbret)
            {
                var v = UT.O2D(line[0]);
                foreach (var kv in cd)
                { kv.Value.SupportDL = v; }
                break;
            }
            return true;
        }

        private static bool GetHPU(string q, string pn, string hputype, Dictionary<string, FCostModel> cd)
        {
            var sql = @"select sum(h.[ProcessHPU]*d.[BOMQty_F]) HPU_F ,
                        sum(h.[ProcessHPU]*d.[BOMQty_M]) HPU_M ,
                        sum(h.[ProcessHPU]*d.[BOMQty_A]) HPU_A 
                        FROM [WebFinance].[dbo].[DataBase] d(nolock)
                        inner join [WebFinance].[dbo].[HPUByItem] h(nolock) on d.item=h.[ItemPartNumber]
                        where h.timespan= @quarter
                        and d.timespan=h.timespan 
                        and d.[Parent] = @pn
                        and d.[Category] = @hputype";

            if (hputype.Contains(HPUType.OracleHPU))
            {
                sql = @"select sum(h.[oracelHPU]*d.[BOMQty_F]) HPU_F , 
                        sum(h.[oracelHPU]*d.[BOMQty_M]) HPU_M , 
                        sum(h.[oracelHPU]*d.[BOMQty_A]) HPU_A  
                        FROM [WebFinance].[dbo].[DataBase] d(nolock)
                        inner join [WebFinance].[dbo].[HPUByItem] h(nolock) on d.item=h.[ItemPartNumber]
                        where h.timespan= @quarter
                        and d.timespan=h.timespan 
                        and d.[Parent] = @pn
                        and d.[Category] = @hputype";
            }

            var dict = new Dictionary<string, string>();
            dict.Add("@quarter", q);
            dict.Add("@pn", pn);
            dict.Add("@hputype", hputype);
            var dbret = DBUtility.ExeFSqlWithRes(sql, dict);

            if (dbret.Count == 0)
            {
                SetNoOK(cd, "No HPU "+hputype);
                return false;
            }

            foreach (var line in dbret)
            {
                var f = UT.O2D(line[0]);
                var fm = UT.O2D(line[1]);
                var a = UT.O2D(line[2]);
                if (hputype.Contains(HPUType.ProcHPU))
                {
                    cd["F"].ProcessHPU = f;
                    cd["FM"].ProcessHPU = fm;
                    cd["A"].ProcessHPU = a;
                }
                else
                {
                    cd["F"].OralceHPU = f;
                    cd["FM"].OralceHPU = fm;
                    cd["A"].OralceHPU = a;
                }
                break;
            }

            return true;
        }

        private static bool GetLabEff(Dictionary<string, FCostModel> cd)
        {
            foreach (var kv in cd)
            {
                kv.Value.LabEff = 0;
                if (kv.Value.ProcessHPU != 0 && kv.Value.OralceHPU != 0)
                {
                    kv.Value.LabEff = (kv.Value.ProcessHPU / kv.Value.OralceHPU) * (1 + kv.Value.SupportDL);
                }
            }
            return true;
        }


        private static bool GetBOMs(string q, string pn, string bomtype, Dictionary<string, FCostModel> cd)
        {
            var sql = @"select  sum([BOMQty_F]*F) F, sum([BOMQty_M]*M) M,sum ([BOMQty_A]*A) A
                        FROM [WebFinance].[dbo].[DataBase]
                        where  TimeSpan=@quarter   and [Parent] =@pn
                        and  Category =@bomtype";
            var dict = new Dictionary<string, string>();
            dict.Add("@quarter", q);
            dict.Add("@pn", pn);
            dict.Add("@bomtype", bomtype);
            var dbret = DBUtility.ExeFSqlWithRes(sql, dict);

            if (dbret.Count == 0)
            {
                SetNoOK(cd, "No BOM " + bomtype);
                return false;
            }

            foreach (var line in dbret)
            {
                var f = UT.O2D(line[0]);
                var fm = UT.O2D(line[1]);
                var a = UT.O2D(line[2]);

                if (bomtype.Contains(BOMCLA.BOM))
                {
                    cd["F"].BOM = f;
                    cd["FM"].BOM = fm;
                    cd["A"].BOM = a;
                }
                else if (bomtype.Contains(BOMCLA.LABOS))
                {
                    cd["F"].LabFOther = f;
                    cd["FM"].LabFOther = fm;
                    cd["A"].LabFOther = a;
                }
                else if (bomtype.Contains(BOMCLA.OFOS))
                {
                    cd["F"].OverheadFOther = f;
                    cd["FM"].OverheadFOther = fm;
                    cd["A"].OverheadFOther = a;
                }
                else if (bomtype.Contains(BOMCLA.OSP))
                {
                    cd["F"].OSP = f;
                    cd["FM"].OSP = fm;
                    cd["A"].OSP = a;
                }

                break;
            }

            return true;
        }

        private static bool GetDLs(string q, string pn, string fglevel,string costpooltype, Dictionary<string, FCostModel> cd)
        {
            var sql = @"select d.item,h.[oracelHPU]*d.[BOMQty_F] HPU_F , 
                    h.[oracelHPU]*d.[BOMQty_M] HPU_M , 
                    h.[oracelHPU]*d.[BOMQty_A] HPU_A  
                    FROM [WebFinance].[dbo].[DataBase] d(nolock)
                    inner join [WebFinance].[dbo].[HPUByItem] h(nolock) on d.item=h.[ItemPartNumber]
                    where h.timespan= @quarter 
                    and d.timespan=h.timespan 
                    and d.[Parent] = @pn
                    and d.[Category] ='Oracle HPU'
                    and d.[level] = @fglevel";

            var dict = new Dictionary<string, string>();
            dict.Add("@quarter", q);
            dict.Add("@pn", pn);
            dict.Add("@fglevel", fglevel);
            var hpudict = new Dictionary<string,Dictionary<string, double>>();
            var dbret = DBUtility.ExeFSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var item = UT.O2S(line[0]);
                var f = UT.O2D(line[1]);
                var fm = UT.O2D(line[2]);
                var a = UT.O2D(line[3]);
                if (!hpudict.ContainsKey(item))
                {
                    var tempdict = new Dictionary<string, double>();
                    tempdict.Add("F", f);
                    tempdict.Add("FM", fm);
                    tempdict.Add("A", a);
                    hpudict.Add(item, tempdict);
                }
            }

            dict.Add("@costpooltype", costpooltype);
            sql = @"select distinct d.item, h.DL/h.Workhours dl,h.IM/h.Workhours im ,h.SM/h.Workhours sm,h.DOH/h.Workhours doh,h.IOH/h.Workhours IOH,h.[IOHSNY]/h.Workhours [IOHSNY]                       
                        FROM [WebFinance].[dbo].[DataBase] d(nolock)
                        left join [WebFinance].[dbo].[CostPool] h(nolock) on d.Family=h.[ProductCode]
                        where h.[TimeSpan]= @quarter 
                        and d.[Parent] = @pn
                        and  h.CostPoolType= @costpooltype 
                        and d.Category='Oracle HPU' 
                        and d. [Level] = @fglevel
                        and d.timespan= h.[TimeSpan]   
                        and h.Workhours !=0 ";

            dbret = DBUtility.ExeFSqlWithRes(sql,dict);
            if (dbret.Count == 0)
            {
                if (costpooltype.Contains(CostPoolType.FM))
                {
                    cd["F"].IsOK = false;
                    cd["FM"].IsOK = false;
                    cd["F"].MSG = "No " + costpooltype + " " + fglevel + " DLs";
                    cd["FM"].MSG = "No " + costpooltype + " " + fglevel + " DLs";
                }
                else
                {
                    cd["A"].IsOK = false;
                    cd["A"].MSG = "No " + costpooltype + " " + fglevel + " DLs";
                }
                return false;
            }

            var klist = new List<string>();
            if (costpooltype.Contains(CostPoolType.FM))
            {
                klist.Add("F");
                klist.Add("FM");
            }
            else
            {
                klist.Add("A");
            }

            foreach (var line in dbret)
            {
                var item = UT.O2S(line[0]);
                var dl = UT.O2D(line[1]);
                var im = UT.O2D(line[2]);
                var sm = UT.O2D(line[3]);
                var doh = UT.O2D(line[4]);
                var ioh = UT.O2D(line[5]);
                var iohsny = UT.O2D(line[6]);

                if (!hpudict.ContainsKey(item))
                { continue; }

                if (fglevel.Contains(FGLEVEL.SFG))
                {
                    foreach (var k in klist)
                    {
                        cd[k].DLSFG += dl * hpudict[item][k];
                        cd[k].IMSFG += im * hpudict[item][k];
                        cd[k].SMSFG += sm * hpudict[item][k];
                        cd[k].DOHSFG += doh * hpudict[item][k];
                        cd[k].IOHSFG += ioh * hpudict[item][k];
                        cd[k].IOHSNYSFG += iohsny * hpudict[item][k];
                    }
                }
                else
                {
                    foreach (var k in klist)
                    {
                        cd[k].DLFG += dl * hpudict[item][k];
                        cd[k].IMFG += im * hpudict[item][k];
                        cd[k].SMFG += sm * hpudict[item][k];
                        cd[k].DOHFG += doh * hpudict[item][k];
                        cd[k].IOHFG += ioh * hpudict[item][k];
                        cd[k].IOHSNYFG += iohsny * hpudict[item][k];
                    }
                }

            }

            return true;
        }

        private static bool GetVariableCost(Dictionary<string, FCostModel> cd)
        {
            foreach (var kv in cd)
            {
                if (kv.Value.IsOK)
                {
                    kv.Value.VairableCost = kv.Value.BOM + kv.Value.LabFOther + kv.Value.OSP + kv.Value.DLFG + kv.Value.DLSFG 
                        + kv.Value.SMFG + kv.Value.SMSFG + kv.Value.IMFG + kv.Value.IMSFG;
                }
            }

            return true;
        }

        private static bool GetUnitMfgCost(Dictionary<string, FCostModel> cd)
        {
            foreach (var kv in cd)
            {
                if (kv.Value.IsOK)
                {
                    kv.Value.UMCost = kv.Value.VairableCost + kv.Value.OverheadFOther + kv.Value.DOHFG + kv.Value.DOHSFG 
                        + kv.Value.IOHFG + kv.Value.IOHSFG + kv.Value.IOHSNYFG + kv.Value.IOHSNYSFG;
                }
            }
            return true;
        }

        private static bool GetQTY(string q, string pn, Dictionary<string, FCostModel> cd)
        {
            var sql = @"select distinct CostPoolType,h.[BuildQty]
                FROM [WebFinance].[dbo].[DataBase] d(nolock)
                left join [WebFinance].[dbo].[CostPool] h(nolock) on d.Family=h.[ProductCode]
                where h.[TimeSpan]= @quarter
                and d.[Item] = @pn 
                and h.[TimeSpan]=d.[TimeSpan]";
            var dict = new Dictionary<string, string>();
            dict.Add("@quarter", q);
            dict.Add("@pn", pn);
            var dbret = DBUtility.ExeFSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var pooltype = UT.O2S(line[0]);
                var qty = UT.O2D(line[1]);
                if (pooltype.ToUpper().Contains(CostPoolType.FM.ToUpper()))
                {
                    cd["F"].Qty = qty;
                    cd["FM"].Qty = qty;
                }
                else if (pooltype.ToUpper().Contains(CostPoolType.ACTURAL.ToUpper()))
                {
                    cd["A"].Qty = qty;
                }
            }
            return true;
        }

        private static void SetNoOK(Dictionary<string, FCostModel> data,string msg)
        {
            foreach (var kv in data)
            {
                kv.Value.IsOK = false;
                kv.Value.MSG = msg;
            }
        }

        public FCostModel()
        {
            SupportDL = 0.0;
            ProcessHPU = 0.0;
            LabEff = 0.0;
            OralceHPU = 0.0;
            BOM = 0.0;
            LabFOther = 0.0;
            OverheadFOther = 0.0;
            OSP = 0.0;
            DLFG = 0.0;
            DLSFG = 0.0;
            SMFG = 0.0;
            SMSFG = 0.0;
            IMFG = 0.0;
            IMSFG = 0.0;
            VairableCost = 0.0;
            DOHFG = 0.0;
            DOHSFG = 0.0;
            IOHFG = 0.0;
            IOHSFG = 0.0;
            IOHSNYFG = 0.0;
            IOHSNYSFG = 0.0;
            UMCost = 0.0;
            Qty = 0.0;
            IsOK = true;
            MSG = "";
        }

        public double SupportDL { set; get; }
        public double ProcessHPU { set; get; }
        public double LabEff { set; get; }
        public double OralceHPU { set; get; }
        public double BOM { set; get; }
        public double LabFOther { set; get; }
        public double OverheadFOther { set; get; }
        public double OSP { set; get; }
        public double DLFG { set; get; }
        public double DLSFG { set; get; }
        public double SMFG { set; get; }
        public double SMSFG { set; get; }
        public double IMFG { set; get; }
        public double IMSFG { set; get; }
        public double VairableCost { set; get; }
        public double DOHFG { set; get; }
        public double DOHSFG { set; get; }
        public double IOHFG { set; get; }
        public double IOHSFG { set; get; }
        public double IOHSNYFG { set; get; }
        public double IOHSNYSFG { set; get; }
        public double UMCost { set; get; }
        public double Qty { set; get; }

        public bool IsOK { set; get; }
        public string MSG { set; get; }
        

    }
}