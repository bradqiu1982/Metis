using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class PLMMatrix
    {
        public static void LoadData(Controller ctrl)
        {
            var plmdata = new Dictionary<string, PLMMatrix>();
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var forcastfolder = syscfg["FORCASTDATA"];
            var allfiles = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, forcastfolder);
            foreach (var f in allfiles)
            {
                var p = System.IO.Path.GetFileNameWithoutExtension(f).ToUpper();
                if (p.Contains("PLM MATRIC") && !p.Contains("$"))
                {
                    var alldata = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, f);
                    foreach (var line in alldata)
                    {
                        var bu = line[0];
                        var mkf = line[1];
                        var plm = line[2].ToUpper();
                        var pn = line[3];
                        var pndesc = line[4];
                        if (!string.IsNullOrEmpty(plm) && !string.IsNullOrEmpty(pn) && !plm.Contains("N/A"))
                        {
                            if (!plmdata.ContainsKey(pn))
                            { plmdata.Add(pn, new PLMMatrix(bu,mkf,plm,pn,pndesc)); }
                        }
                    }//END FOREACH
                }//END IF
            }//END FOREACH

            if (plmdata.Count > 1500)
            {
                CleanData();
                foreach (var kv in plmdata)
                {
                    kv.Value.StoreData();
                }
            }

        }

        private static void CleanData()
        {
            var sql = "delete from PLMMatrix";
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreData()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@BU", BU);
            dict.Add("@MKF", MKF);
            dict.Add("@PLM", PLM);
            dict.Add("@PN", PN);
            dict.Add("@PNDesc", PNDesc);

            var sql = "insert into PLMMatrix(BU,MKF,PLM,PN,PNDesc) values(@BU,@MKF,@PLM,@PN,@PNDesc)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }


        public PLMMatrix(string bu,string mkf,string plm,string pn,string pndesc)
        {
            BU = bu;
            MKF = mkf;
            PLM = plm;
            PN = pn;
            PNDesc = pndesc;
        }

        public PLMMatrix()
        {
            BU = "";
            MKF = "";
            PLM = "";
            PN = "";
            PNDesc = "";
        }

        public string BU { set; get; }
        public string MKF { set; get; }
        public string PLM { set; get; }
        public string PN { set; get; }
        public string PNDesc { set; get; }
    }
}