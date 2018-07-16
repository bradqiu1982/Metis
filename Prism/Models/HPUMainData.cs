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
        }

        public void StoreData()
        {
            var sql = @"insert into HPUMainData(PNLink,HPUOrder,HPUCode,ProductLine,Serial,Customer,Phase,TypicalPN,WorkingHourMeasure,WorkingHourCollect,WorkingHourChecked,YieldHPU,Owner,UpdateDate,SignDate,FormMake,Remark,Family,ProcessSplit,Quarter,QuarterDate)  
                         values(@PNLink,@HPUOrder,@HPUCode,@ProductLine,@Serial,@Customer,@Phase,@TypicalPN,@WorkingHourMeasure,@WorkingHourCollect,@WorkingHourChecked,@YieldHPU,@Owner,@UpdateDate,@SignDate,@FormMake,@Remark,@Family,@ProcessSplit,@Quarter,@QuarterDate)";
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
            DBUtility.ExeLocalSqlNoRes(sql, param);
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