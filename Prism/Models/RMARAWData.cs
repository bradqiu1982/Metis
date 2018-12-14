using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class RMARAWData
    {
        public static void LoadRMARawData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var rmafolder = syscfg["RMASHAREFOLDER"];
            var flist = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, rmafolder);
            var rmafiles = new List<string>();
            foreach (var fn in flist)
            {
                if (fn.ToUpper().Contains("RMA")
                    && fn.ToUpper().Contains("COMPLAINT")
                    && fn.ToUpper().Contains("NEWDATA")
                    && !fn.ToUpper().Contains("~$"))
                {
                    rmafiles.Add(fn);
                }
            }

            var rmadata = new List<RMARAWData>();
            foreach (var rmf in rmafiles)
            {
                var localrmaf = ExternalDataCollector.DownloadShareFile(rmf, ctrl);
                if (localrmaf != null)
                {
                    var data = ExcelReader.RetrieveDataFromExcel(localrmaf, null);
                    var idx = 0;
                    foreach (var line in data)
                    {
                        if (idx == 0)
                        { idx++; continue; }

                        rmadata.Add(ParseRMARAWDataValue(line));
                        idx++;
                    }//end foreach
                }//end if
            }//end foreach

            var rmanumdict = new Dictionary<string, bool>();
            foreach (var item in rmadata)
            {
                if (!rmanumdict.ContainsKey(item.AppV_B))
                { rmanumdict.Add(item.AppV_B,true); }
            }

            var rmanumlist = rmanumdict.Keys.ToList();
            if (rmanumlist.Count > 0)
            {
                var rmacond = "('" + string.Join("','", rmanumlist) + "')";
                CleanData(rmacond);
                foreach (var item in rmadata)
                {
                    item.StoreData();
                }
            }//end if

        }

        private static void CleanData(string rmacond)
        {
            var sql = "delete from RMARAWData where AppV_B in <rmacond>";
            sql = sql.Replace("<rmacond>", rmacond);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreData()
        {
            var sql = "insert into RMARAWData(AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O"
                    + ",AppV_P,AppV_Q,AppV_R,AppV_S,AppV_T,AppV_U,AppV_V,AppV_W,AppV_X"
                    + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC,AppV_AD,AppV_AE,AppV_AF,AppV_AG,AppV_AH)"
                    + " values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>',N'<AppV_E>',N'<AppV_F>'"
                    + ",N'<AppV_G>',N'<AppV_H>',N'<AppV_I>',N'<AppV_J>',N'<AppV_K>',N'<AppV_L>',N'<AppV_M>',N'<AppV_N>',N'<AppV_O>'"
                    + ",'<AppV_P>','<AppV_Q>','<AppV_R>','<AppV_S>','<AppV_T>',N'<AppV_U>','<AppV_V>','<AppV_W>',N'<AppV_X>'"
                    + ",N'<AppV_Y>',N'<AppV_Z>',N'<AppV_AA>',N'<AppV_AB>','<AppV_AC>',N'<AppV_AD>',N'<AppV_AE>',N'<AppV_AF>',N'<AppV_AG>',N'<AppV_AH>')";

            sql = sql.Replace("<AppV_A>", AppV_A).Replace("<AppV_B>", AppV_B).Replace("<AppV_C>", AppV_C)
                .Replace("<AppV_D>", AppV_D).Replace("<AppV_E>", AppV_E).Replace("<AppV_F>", AppV_F)
                .Replace("<AppV_G>", AppV_G).Replace("<AppV_H>", AppV_H).Replace("<AppV_I>", AppV_I)
                .Replace("<AppV_J>", AppV_J).Replace("<AppV_K>", AppV_K).Replace("<AppV_L>", AppV_L)
                .Replace("<AppV_M>", AppV_M).Replace("<AppV_N>", AppV_N).Replace("<AppV_O>", AppV_O)
                .Replace("<AppV_P>", AppV_P).Replace("<AppV_Q>", AppV_Q).Replace("<AppV_R>", AppV_R)
                .Replace("<AppV_S>", AppV_S).Replace("<AppV_T>", AppV_T).Replace("<AppV_U>", AppV_U)
                .Replace("<AppV_V>", AppV_V).Replace("<AppV_W>", AppV_W).Replace("<AppV_X>", AppV_X)
                .Replace("<AppV_Y>", AppV_Y).Replace("<AppV_Z>", AppV_Z).Replace("<AppV_AA>", AppV_AA)
                .Replace("<AppV_AB>", AppV_AB).Replace("<AppV_AC>", AppV_AC).Replace("<AppV_AD>", AppV_AD)
                .Replace("<AppV_AE>", AppV_AE).Replace("<AppV_AF>", AppV_AF).Replace("<AppV_AG>", AppV_AG).Replace("<AppV_AH>", AppV_AH);

            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static Dictionary<string, int> RetrieveParallelRMACntByMonth(string sdate, string edate)
        {
            var productcond = "AppV_F like 'Parallel'";
            return RetrieveRMACntByMonth(sdate, edate, productcond);
        }

        public static Dictionary<string, int> RetrieveTunableRMACntByMonth(string sdate, string edate)
        {
            var productcond = "AppV_F like 'Coherent Transceiver' or AppV_F like 'COHERENT'  or AppV_F like '10G Tunable' or AppV_F like '10G T-XFP' or AppV_F like 'TXFP' ";
            return RetrieveRMACntByMonth(sdate, edate, productcond);
        }

        private static Dictionary<string, int> RetrieveRMACntByMonth(string sdate, string edate, string productcond)
        {
            var ret = new Dictionary<string, int>();
            var sql = "select AppV_W,AppV_J from RMARAWData where AppV_W >= @sdate and AppV_W <=@edate and AppV_Y <> 'NTF' and  AppV_Y <> '' and  AppV_X <> 'NTF' and  AppV_X <> '' and  AppV_X <> 'ESD' and  AppV_Z <> 'NTF' and  AppV_Z not like '%custom%' and (<productcond>) ";
            sql = sql.Replace("<productcond>", productcond);

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var m = Convert.ToDateTime(line[0]).ToString("yyyy-MM");
                var qty = Convert.ToInt32(Convert2DB(line[1]));
                if (ret.ContainsKey(m))
                {
                    ret[m] = ret[m] + qty;
                }
                else
                {
                    ret.Add(m, qty);
                }
            }
            return ret;
        }

        private static List<RMADppmData> RetrieveRMADataByMonth(string sdate, string edate, string productcond)
        {
            var ret = new List<RMADppmData>();

            var sql = "select AppV_B,AppV_F,AppV_G,AppV_H,AppV_I,AppV_J,AppV_W,AppV_Y from RMARAWData where AppV_W >= @sdate and AppV_W <=@edate and AppV_Y <> 'NTF' and  AppV_Y <> '' and  AppV_X <> 'NTF' and  AppV_X <> '' and  AppV_X <> 'ESD' and  AppV_Z <> 'NTF' and  AppV_Z not like '%custom%' and (<productcond>) order by AppV_W asc";
            sql = sql.Replace("<productcond>", productcond);
            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var rootcause = Convert2Str(line[7]);
                rootcause = rootcause.Length > 48 ? rootcause.Substring(0, 48) : rootcause;
                var sn = Convert2Str(line[4]);
                sn = sn.Length > 24 ? sn.Substring(0, 24) : sn;

                ret.Add(new RMADppmData(Convert2Str(line[0]), Convert2Str(line[1]), Convert2Str(line[2])
                    , Convert2Str(line[3]), sn, Convert2DB(line[5]), Convert2DT(line[6]), rootcause));
            }
            return ret;
        }

        public static List<RMADppmData> RetrieveRMARawDataByMonth(string sdate, string edate, string producttype)
        {
            if (string.Compare(producttype, SHIPPRODTYPE.PARALLEL, true) == 0)
            {
                var productcond = "AppV_F like 'Parallel'";
                return RetrieveRMADataByMonth(sdate, edate, productcond);
            }
            else if (string.Compare(producttype, SHIPPRODTYPE.TUNABLE, true) == 0)
            {
                var productcond = "AppV_F like 'Coherent Transceiver' or AppV_F like 'COHERENT'  or AppV_F like '10G Tunable' or AppV_F like '10G T-XFP' or AppV_F like 'TXFP' ";
                return RetrieveRMADataByMonth(sdate, edate, productcond);
            }
            return new List<RMADppmData>();
        }

        private static List<RMADppmData> RetrieveWorkLoadDataByMonth(string sdate, string edate, string productcond)
        {
            var ret = new List<RMADppmData>();

            var sql = "select AppV_B,AppV_F,AppV_G,AppV_H,AppV_I,AppV_J,AppV_P,AppV_Y,AppV_R from RMARAWData where AppV_P >= @sdate and AppV_P <=@edate and (<productcond>) order by AppV_B";
            sql = sql.Replace("<productcond>", productcond);
            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var rootcause = Convert2Str(line[7]);
                rootcause = rootcause.Length > 48 ? rootcause.Substring(0, 48) : rootcause;
                var sn = Convert2Str(line[4]);
                sn = sn.Length > 24 ? sn.Substring(0, 24) : sn;

                var item = new RMADppmData(Convert2Str(line[0]), Convert2Str(line[1]), Convert2Str(line[2])
                    , Convert2Str(line[3]), sn, Convert2DB(line[5]), Convert2DT(line[6]), rootcause);
                item.InitFAR = Convert2DT(line[8]);
                ret.Add(item);

            }
            return ret;
        }

        public static List<RMADppmData> RetrieveRMAWorkLoadDataByMonth(string sdate, string edate, string producttype)
        {
            if (string.Compare(producttype, SHIPPRODTYPE.PARALLEL, true) == 0)
            {
                var productcond = "AppV_F like 'Parallel'";
                return RetrieveWorkLoadDataByMonth(sdate, edate, productcond);
            }
            else if (string.Compare(producttype, SHIPPRODTYPE.TUNABLE, true) == 0)
            {
                var productcond = "AppV_F like 'Coherent Transceiver' or AppV_F like 'COHERENT'  or AppV_F like '10G Tunable' or AppV_F like '10G T-XFP' or AppV_F like 'TXFP' ";
                return RetrieveWorkLoadDataByMonth(sdate, edate, productcond);
            }
            return new List<RMADppmData>();
        }

        private static RMARAWData ParseRMARAWDataValue(List<string> line)
        {
            var tempdata = new RMARAWData();
            tempdata.AppV_A = line[0];
            tempdata.AppV_B = line[1].Replace("'","");
            tempdata.AppV_C = line[2];
            tempdata.AppV_D = line[3];
            tempdata.AppV_E = line[4];
            tempdata.AppV_F = line[5];
            tempdata.AppV_G = line[6];
            tempdata.AppV_H = line[7];
            tempdata.AppV_I = line[8];
            tempdata.AppV_J = line[9];
            tempdata.AppV_K = line[10];
            tempdata.AppV_L = line[11];
            tempdata.AppV_M = line[12];
            tempdata.AppV_N = line[13];
            tempdata.AppV_O = line[14];
            tempdata.AppV_P = ConvertToDateStr(line[15]);
            tempdata.AppV_Q = ConvertToDateStr(line[16]);
            tempdata.AppV_R = ConvertToDateStr(line[17]);
            tempdata.AppV_S = ConvertToDateStr(line[18]);
            tempdata.AppV_T = ConvertToDateStr(line[19]);
            tempdata.AppV_U = line[20];
            tempdata.AppV_V = ConvertToDateStr(line[21]);
            tempdata.AppV_W = ConvertToDateStr(line[22]);
            tempdata.AppV_X = line[23];
            tempdata.AppV_Y = line[24];
            tempdata.AppV_Z = line[25];
            tempdata.AppV_AA = line[26];
            tempdata.AppV_AB = line[27];
            tempdata.AppV_AC = ConvertToDateStr(line[28]);
            tempdata.AppV_AD = line[29];
            tempdata.AppV_AE = line[30];
            tempdata.AppV_AF = line[31];
            tempdata.AppV_AG = line[32];
            tempdata.AppV_AH = line[33];
            return tempdata;
        }

        private static string Convert2Str(object obj)
        {
            if (obj == null)
            { return string.Empty; }
            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }

        private static DateTime Convert2DT(object obj)
        {
            if (obj == null)
            { return DateTime.Parse("1982-05-06 10:00:00"); }
            try
            {
                return Convert.ToDateTime(obj);
            }
            catch (Exception ex) { return DateTime.Parse("1982-05-06 10:00:00"); }
        }

        private static double Convert2DB(object obj)
        {
            if (obj == null)
            { return 0.0; }
            try
            {
                return Convert.ToDouble(obj);
            }
            catch (Exception ex) { return 0.0; }
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

        public RMARAWData()
        {
            AppV_A = string.Empty;
            AppV_B = string.Empty;
            AppV_C = string.Empty;
            AppV_D = string.Empty;
            AppV_E = string.Empty;
            AppV_F = string.Empty;
            AppV_G = string.Empty;
            AppV_H = string.Empty;
            AppV_I = string.Empty;
            AppV_J = string.Empty;
            AppV_K = string.Empty;
            AppV_L = string.Empty;
            AppV_M = string.Empty;
            AppV_N = string.Empty;
            AppV_O = string.Empty;
            AppV_P = string.Empty;
            AppV_Q = string.Empty;
            AppV_R = string.Empty;
            AppV_S = string.Empty;
            AppV_T = string.Empty;
            AppV_U = string.Empty;
            AppV_V = string.Empty;
            AppV_W = string.Empty;
            AppV_X = string.Empty;
            AppV_Y = string.Empty;
            AppV_Z = string.Empty;
            AppV_AA = string.Empty;
            AppV_AB = string.Empty;
            AppV_AC = string.Empty;
            AppV_AD = string.Empty;
            AppV_AE = string.Empty;
            AppV_AF = string.Empty;
            AppV_AG = string.Empty;
            AppV_AH = string.Empty;
        }

        public string AppV_A { set; get; }
        public string AppV_B { set; get; }
        public string AppV_C { set; get; }
        public string AppV_D { set; get; }
        public string AppV_E { set; get; }
        public string AppV_F { set; get; }
        public string AppV_G { set; get; }
        public string AppV_H { set; get; }
        public string AppV_I { set; get; }
        public string AppV_J { set; get; }
        public string AppV_K { set; get; }
        public string AppV_L { set; get; }
        public string AppV_M { set; get; }
        public string AppV_N { set; get; }
        public string AppV_O { set; get; }
        public string AppV_P { set; get; }
        public string AppV_Q { set; get; }
        public string AppV_R { set; get; }
        public string AppV_S { set; get; }
        public string AppV_T { set; get; }
        public string AppV_U { set; get; }
        public string AppV_V { set; get; }
        public string AppV_W { set; get; }
        public string AppV_X { set; get; }
        public string AppV_Y { set; get; }
        public string AppV_Z { set; get; }
        public string AppV_AA { set; get; }
        public string AppV_AB { set; get; }
        public string AppV_AC { set; get; }
        public string AppV_AD { set; get; }
        public string AppV_AE { set; get; }
        public string AppV_AF { set; get; }
        public string AppV_AG { set; get; }
        public string AppV_AH { set; get; }
    }
}