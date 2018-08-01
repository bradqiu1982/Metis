using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class HPUMainData
    {
        public HPUMainData()
        {
            HPUOrder = 0;
            HPUCode = "";
            ProductLine = "";
            Serial = "";
            Customer = "";
            Phase = "";
            TypicalPN = "";
            WorkingHourMeasure = "";
            WorkingHourCollect = "";
            WorkingHourChecked = "";
            YieldHPU = "";
            Owner = "";
            UpdateDate = "";
            SignDate = "";
            FormMake = "";
            Remark = "";
            Family = "";
            ProcessSplit = "";

            PNLink = "";
            Quarter = "";
            QuarterDate = DateTime.Parse("1982-05-06 10:00:00");
            DetailLink = "";

            WeeklyCapacity = 0.0;
            SeasonCapacity = 0.0;
        }

        public void StoreData()
        {
            var sql = @"insert into HPUMainData(PNLink,HPUOrder,HPUCode,ProductLine,Serial,Customer,Phase,TypicalPN,WorkingHourMeasure,WorkingHourCollect,WorkingHourChecked,YieldHPU,Owner,UpdateDate,SignDate,FormMake,Remark,Family,ProcessSplit,Quarter,QuarterDate,DetailLink)  
                         values(@PNLink,@HPUOrder,@HPUCode,@ProductLine,@Serial,@Customer,@Phase,@TypicalPN,@WorkingHourMeasure,@WorkingHourCollect,@WorkingHourChecked,@YieldHPU,@Owner,@UpdateDate,@SignDate,@FormMake,@Remark,@Family,@ProcessSplit,@Quarter,@QuarterDate,@DetailLink)";
            var param = new Dictionary<string, string>();
            param.Add("@PNLink", PNLink);
            param.Add("@HPUOrder", HPUOrder.ToString());
            param.Add("@HPUCode", HPUCode);
            param.Add("@ProductLine", ProductLine);
            param.Add("@Serial", Serial);
            param.Add("@Customer", Customer);
            param.Add("@Phase", Phase);
            param.Add("@TypicalPN", TypicalPN);
            param.Add("@WorkingHourMeasure", WorkingHourMeasure);
            param.Add("@WorkingHourCollect", WorkingHourCollect);
            param.Add("@WorkingHourChecked", WorkingHourChecked);
            param.Add("@YieldHPU", YieldHPU);
            param.Add("@Owner", Owner);
            param.Add("@UpdateDate", UpdateDate);
            param.Add("@SignDate", SignDate);
            param.Add("@FormMake", FormMake);
            param.Add("@Remark", Remark);
            param.Add("@Family", Family);
            param.Add("@ProcessSplit", ProcessSplit);
            param.Add("@Quarter", Quarter);
            param.Add("@QuarterDate", QuarterDate.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@DetailLink", DetailLink);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public void UpdateData()
        {
            var sql = "update HPUMainData set YieldHPU=@YieldHPU,DetailLink=@DetailLink where PNLink=@PNLink";
            var param = new Dictionary<string, string>();
            param.Add("@PNLink", PNLink);
            param.Add("@YieldHPU", YieldHPU);
            param.Add("@DetailLink", DetailLink);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static List<string> GetAllProductLines()
        {
            var ret = new List<string>();
            var sql = "select distinct ProductLine from HPUMainData where ProductLine <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            ret.Sort();
            return ret;
        }

        public static List<string> GetAllQuarters()
        {
            var ret = new List<string>();
            var sql = "select distinct Quarter from HPUMainData where Quarter <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            ret.Sort();
            return ret;
        }

        private static string Convert2DateStr(object d)
        {
            try
            {
                return Convert.ToDateTime(d).ToString("yy/MM/dd");
            }
            catch (Exception) { return string.Empty; }
        }

        public static List<HPUMainData> RetrieveHPUData(string pdline, string quarter)
        {
            var ret = new List<HPUMainData>();
            var sql = @"select PNLink,HPUOrder,HPUCode,ProductLine,Serial,Customer,Phase,TypicalPN,WorkingHourMeasure,WorkingHourCollect
                            ,WorkingHourChecked,YieldHPU,Owner,UpdateDate,SignDate,FormMake,Remark,Family,ProcessSplit,Quarter,QuarterDate,DetailLink from HPUMainData where ProductLine=@ProductLine and Quarter=@Quarter order by HPUOrder ASC";
            var param = new Dictionary<string, string>();
            param.Add("@ProductLine",pdline);
            param.Add("@Quarter",quarter);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                try
                {
                    var tempvm = new HPUMainData();
                    tempvm.PNLink = Convert.ToString(line[0]);
                    tempvm.HPUOrder = Convert.ToInt32(line[1]);
                    tempvm.HPUCode = Convert.ToString(line[2]);
                    tempvm.ProductLine = Convert.ToString(line[3]);
                    tempvm.Serial = Convert.ToString(line[4]);
                    tempvm.Customer = Convert.ToString(line[5]);
                    tempvm.Phase = Convert.ToString(line[6]);
                    tempvm.TypicalPN = Convert.ToString(line[7]);
                    tempvm.WorkingHourMeasure = Convert.ToString(line[8]);
                    tempvm.WorkingHourCollect = Convert.ToString(line[9]);
                    tempvm.WorkingHourChecked = Convert.ToString(line[10]);
                    tempvm.YieldHPU = Math.Round(Convert.ToDouble(line[11]),4).ToString();
                    tempvm.Owner = Convert.ToString(line[12]);
                    tempvm.UpdateDate = Convert2DateStr(line[13]);
                    tempvm.SignDate = Convert.ToString(line[14]);
                    tempvm.FormMake = Convert.ToString(line[15]);
                    tempvm.Remark = Convert.ToString(line[16]);
                    tempvm.Family = Convert.ToString(line[17]);
                    tempvm.ProcessSplit = Convert.ToString(line[18]);
                    tempvm.Quarter = Convert.ToString(line[19]);
                    tempvm.QuarterDate = Convert.ToDateTime(line[20]);
                    tempvm.DetailLink = Convert.ToString(line[21]);

                    if (string.IsNullOrEmpty(tempvm.HPUCode))
                    {
                        tempvm.HPUCode = tempvm.TypicalPN;
                    }

                    ret.Add(tempvm);
                }
                catch (Exception ex) { }


            }

            return ret;
        }

        public static List<HPUMainData> RetrieveHPUDataBySerial(string serials)
        {
            var ret = new List<HPUMainData>();
            var sql = @"select PNLink,HPUOrder,HPUCode,ProductLine,Serial,Customer,Phase,TypicalPN,WorkingHourMeasure,WorkingHourCollect
                            ,WorkingHourChecked,YieldHPU,Owner,UpdateDate,SignDate,FormMake,Remark,Family,ProcessSplit,Quarter,QuarterDate,DetailLink from HPUMainData where ";

            var idx = 0;
            var ss = serials.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in ss)
            {
                if (idx == 0)
                {
                    sql += " Serial like '%"+s.Replace("'","").Trim()+"%'";
                    idx = idx + 1;
                }
                else
                {
                    sql += " or Serial like '%" + s.Replace("'", "").Trim() + "%'";
                }
            }
            
            sql += " order by Serial,QuarterDate asc";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var tempvm = new HPUMainData();
                    tempvm.PNLink = Convert.ToString(line[0]);
                    tempvm.HPUOrder = Convert.ToInt32(line[1]);
                    tempvm.HPUCode = Convert.ToString(line[2]);
                    tempvm.ProductLine = Convert.ToString(line[3]);
                    tempvm.Serial = Convert.ToString(line[4]);
                    tempvm.Customer = Convert.ToString(line[5]);
                    tempvm.Phase = Convert.ToString(line[6]);
                    tempvm.TypicalPN = Convert.ToString(line[7]);
                    tempvm.WorkingHourMeasure = Convert.ToString(line[8]);
                    tempvm.WorkingHourCollect = Convert.ToString(line[9]);
                    tempvm.WorkingHourChecked = Convert.ToString(line[10]);
                    tempvm.YieldHPU = Math.Round(Convert.ToDouble(line[11]), 4).ToString();
                    tempvm.Owner = Convert.ToString(line[12]);
                    tempvm.UpdateDate = Convert2DateStr(line[13]);
                    tempvm.SignDate = Convert.ToString(line[14]);
                    tempvm.FormMake = Convert.ToString(line[15]);
                    tempvm.Remark = Convert.ToString(line[16]);
                    tempvm.Family = Convert.ToString(line[17]);
                    tempvm.ProcessSplit = Convert.ToString(line[18]);
                    tempvm.Quarter = Convert.ToString(line[19]);
                    tempvm.QuarterDate = Convert.ToDateTime(line[20]);
                    tempvm.DetailLink = Convert.ToString(line[21]);

                    if (string.IsNullOrEmpty(tempvm.HPUCode))
                    {
                        tempvm.HPUCode = tempvm.TypicalPN;
                    }

                    ret.Add(tempvm);
                }
                catch (Exception ex) { }


            }

            return ret;
        }

        public static List<string> RetrieveAllSerial()
        {
            var sql = "select distinct Serial FROM  HPUMainData where [Serial] not like 'ASY,%'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var dict = new Dictionary<string, bool>();
            foreach (var line in dbret)
            {
                var val = Convert.ToString(line[0]);
                if (val.Contains("-FG") || val.Contains("- FG") || val.Contains("-SFG") || val.Contains("- SFG"))
                { val = val.Split(new string[] { "-FG", "- FG", "-SFG", "- SFG" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim(); }
                if (!dict.ContainsKey(val))
                {
                    dict.Add(val, true);
                }
            }//end foreach
            
            var ret = dict.Keys.ToList();
            ret.Sort();
            return ret;
        }

        public static Dictionary<string, bool> RetrieveAllPNLink()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct PNLink from HPUMainData where PNLink <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var val = Convert.ToString(line[0]);
                if (!ret.ContainsKey(val))
                {
                    ret.Add(val, true);
                }
            }
            return ret;
        }

        public static void UPdateDateFormatPN()
        {
            var sql = "select distinct PNLink from HPUMainData where PNLink like '%12:00:00 AM%'";
            var datepnlist = new List<string>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                datepnlist.Add(Convert.ToString(line[0]));
            }

            foreach (var item in datepnlist)
            {
                var datepn = item.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                var qt = item.Split(new string[] { "12:00:00 AM" }, StringSplitOptions.RemoveEmptyEntries)[1];

                try
                {
                    var numpn = ((int)DateTime.Parse(datepn).ToOADate()).ToString();
                    var newpnlik = numpn + "_" + qt;

                    sql = "update HPUMainData set DetailLink = '<DetailLink>' where DetailLink = '<DetailLinkCond>'";
                    sql = sql.Replace("<DetailLink>", newpnlik).Replace("<DetailLinkCond>", item);
                    DBUtility.ExeLocalSqlNoRes(sql);
                    sql = "update PNHPUData set PNLink = '<DetailLink>' where PNLink = '<DetailLinkCond>'";
                    sql = sql.Replace("<DetailLink>", newpnlik).Replace("<DetailLinkCond>", item);
                    DBUtility.ExeLocalSqlNoRes(sql);
                    sql = "update HPUMainData set PNLink = '<DetailLink>',TypicalPN = '<TypicalPN>' where PNLink = '<DetailLinkCond>'";
                    sql = sql.Replace("<DetailLink>", newpnlik).Replace("<DetailLinkCond>", item).Replace("<TypicalPN>",numpn);
                    DBUtility.ExeLocalSqlNoRes(sql);
                }
                catch (Exception ex) { }
            }
        }

        public string PNLink { set; get; }

        public int HPUOrder { set; get; }
        public string HPUCode { set; get; }
        public string ProductLine { set; get; }
        public string Serial { set; get; }
        public string Customer { set; get; }
        public string Phase { set; get; }
        public string TypicalPN { set; get; }
        public string WorkingHourMeasure { set; get; }
        public string WorkingHourCollect { set; get; }
        public string WorkingHourChecked { set; get; }
        public string YieldHPU { set; get; }
        public string Owner { set; get; }
        public string UpdateDate { set; get; }
        public string SignDate { set; get; }
        public string FormMake { set; get; }
        public string Remark { set; get; }
        public string Family { set; get; }
        public string ProcessSplit { set; get; }

        public string Quarter { set; get; }
        public DateTime QuarterDate { set; get; }

        public string DetailLink { set; get; }

        public double WeeklyCapacity { set; get; }
        public double SeasonCapacity { set; get; }
         
    }


    public class PNHPUData
    {
        public PNHPUData()
        {
            PNLink = "";
            DataOrder = 0;
            A_Val = "";
            B_Val = "";
            C_Val = "";
            D_Val = "";
            E_Val = "";
            F_Val = "";
            G_Val = "";
            H_Val = "";
            I_Val = "";
            J_Val = "";
            K_Val = "";
            L_Val = "";
            M_Val = "";
            N_Val = "";
            O_Val = "";
            P_Val = "";
            Q_Val = "";
            R_Val = "";
            S_Val = "";
            T_Val = "";
            U_Val = "";
            V_Val = "";
            W_Val = "";
            X_Val = "";
            Y_Val = "";
            Z_Val = "";

            Quarter = "";
            QuarterDate = DateTime.Parse("1982-05-06 10:00:00");
        }

        public void StoreData()
        {
            var sql = @"insert into PNHPUData(PNLink,DataOrder,A_Val,B_Val,C_Val,D_Val,E_Val,F_Val,G_Val,H_Val,I_Val,J_Val,K_Val,L_Val,M_Val,N_Val,O_Val,P_Val,Q_Val,R_Val,S_Val,T_Val,U_Val,V_Val,W_Val,X_Val,Y_Val,Z_Val,Quarter,QuarterDate) 
                     values(@PNLink,@DataOrder,@A_Val,@B_Val,@C_Val,@D_Val,@E_Val,@F_Val,@G_Val,@H_Val,@I_Val,@J_Val,@K_Val,@L_Val,@M_Val,@N_Val,@O_Val,@P_Val,@Q_Val,@R_Val,@S_Val,@T_Val,@U_Val,@V_Val,@W_Val,@X_Val,@Y_Val,@Z_Val,@Quarter,@QuarterDate)";
            var param = new Dictionary<string, string>();
            param.Add("@PNLink", PNLink);
            param.Add("@DataOrder", DataOrder.ToString());
            param.Add("@A_Val", A_Val);
            param.Add("@B_Val", B_Val);
            param.Add("@C_Val", C_Val);
            param.Add("@D_Val", D_Val);
            param.Add("@E_Val", E_Val);
            param.Add("@F_Val", F_Val);
            param.Add("@G_Val", G_Val);
            param.Add("@H_Val", H_Val);
            param.Add("@I_Val", I_Val);
            param.Add("@J_Val", J_Val);
            param.Add("@K_Val", K_Val);
            param.Add("@L_Val", L_Val);
            param.Add("@M_Val", M_Val);
            param.Add("@N_Val", N_Val);
            param.Add("@O_Val", O_Val);
            param.Add("@P_Val", P_Val);
            param.Add("@Q_Val", Q_Val);
            param.Add("@R_Val", R_Val);
            param.Add("@S_Val", S_Val);
            param.Add("@T_Val", T_Val);
            param.Add("@U_Val", U_Val);
            param.Add("@V_Val", V_Val);
            param.Add("@W_Val", W_Val);
            param.Add("@X_Val", X_Val);
            param.Add("@Y_Val", Y_Val);
            param.Add("@Z_Val", Z_Val);
            param.Add("@Quarter", Quarter);
            param.Add("@QuarterDate", QuarterDate.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private static List<List<string>> TrimData(List<List<string>> rawdata)
        {
            var maxcol = 0;
            var titletrow = 0;

            var ridx = 0;
            var checkrows = (rawdata.Count > 6) ? 6 : rawdata.Count;

            for (ridx = 0; ridx < checkrows; ridx++)
            {
                var tempmax = 0;
                foreach (var data in rawdata[ridx])
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        tempmax++;
                    }
                }
                if (tempmax > maxcol)
                {
                    maxcol = tempmax;
                    titletrow = ridx;
                }
            }//end for

            var ret = new List<List<string>>();
            var titleline = new List<string>();

            var validcolidxs = new List<int>();
            ridx = 0;
            foreach (var item in rawdata[titletrow])
            {
                if (!string.IsNullOrEmpty(item))
                {
                    titleline.Add(item);
                    validcolidxs.Add(ridx);
                }
                ridx++;
            }
            ret.Add(titleline);

            ridx = 0;
            for (ridx = titletrow + 1; ridx < rawdata.Count; ridx++)
            {
                var datacount = 0;
                var templine = new List<string>();
                foreach (var cidx in validcolidxs)
                {
                    var d = rawdata[ridx][cidx];
                    if (!string.IsNullOrEmpty(d) && IsDigitsOnly(d.Replace(".", "").Replace("E","").Replace("-","")))
                    {
                        d = (Math.Round(Convert.ToDouble(d), 4)).ToString();
                    }

                    templine.Add(d);
                    if (!string.IsNullOrEmpty(d))
                    {
                        datacount++;
                    }
                }

                if (datacount >= (maxcol-1)/2)
                {
                    ret.Add(templine);
                }
            }

            return ret;
        }

        public static List<List<string>> RetrieveHPUData(string pnlink)
        {
            var rawdata = new List<List<string>>();
            var sql = @"select  DataOrder,A_Val,B_Val,C_Val,D_Val,E_Val,F_Val,G_Val,H_Val,I_Val,J_Val,K_Val,L_Val,M_Val,N_Val
                            ,O_Val,P_Val,Q_Val,R_Val,S_Val,T_Val,U_Val,V_Val,W_Val,X_Val,Y_Val,Z_Val from PNHPUData where PNLink=@PNLink order by DataOrder ASC";
            var param = new Dictionary<string, string>();
            param.Add("@PNLink", pnlink);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                var templine = new List<string>();
                for (var idx = 0; idx < 27; idx++)
                {
                    templine.Add(Convert.ToString(line[idx]));
                }
                rawdata.Add(templine);
            }

            if (rawdata.Count > 0)
            {
                return TrimData(rawdata);
            }
            else
            {
                return rawdata;
            }

        }

        public static void CleanData(string pnlink)
        {
            var sql = "delete from PNHPUData where  PNLink=@PNLink ";
            var param = new Dictionary<string, string>();
            param.Add("@PNLink", pnlink);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static List<string> RetrievePNLinkList()
        {
            var sql = "select distinct PNLink from PNHPUData where PNLink <> '' order by PNLink ASC";
            var ret = new List<string>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            ret.Sort();
            return ret;
        }

        public string PNLink { set; get; }
        public int DataOrder { set; get; }
        public string A_Val { set; get; }
        public string B_Val { set; get; }
        public string C_Val { set; get; }
        public string D_Val { set; get; }
        public string E_Val { set; get; }
        public string F_Val { set; get; }
        public string G_Val { set; get; }
        public string H_Val { set; get; }
        public string I_Val { set; get; }
        public string J_Val { set; get; }
        public string K_Val { set; get; }
        public string L_Val { set; get; }
        public string M_Val { set; get; }
        public string N_Val { set; get; }
        public string O_Val { set; get; }
        public string P_Val { set; get; }
        public string Q_Val { set; get; }
        public string R_Val { set; get; }
        public string S_Val { set; get; }
        public string T_Val { set; get; }
        public string U_Val { set; get; }
        public string V_Val { set; get; }
        public string W_Val { set; get; }
        public string X_Val { set; get; }
        public string Y_Val { set; get; }
        public string Z_Val { set; get; }

        public string Quarter { set; get; }
        public DateTime QuarterDate { set; get; }
    }

}