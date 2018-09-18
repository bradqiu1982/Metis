using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Prism.Models
{
    public class ModuleTestData
    {
        public ModuleTestData()
        {
            Init();
        }

        public ModuleTestData(string dataid,string sn,string tm,string wt,string err,string station,
            string pf,string pn,string pndesc,string mt,string spd,string spt,string mes)
        {
            DataID = dataid;
            ModuleSN = sn;
            TestTimeStamp = tm;
            WhichTest = wt;
            ErrAbbr = err;
            TestStation = station;
            ProductFamily = pf;
            PN = pn;
            PNDesc = pndesc;
            ModuleType = mt;
            SpeedRate = spd;
            SpendTime = spt;
            MESTab = mes;
        }

        private void Init()
        {
            DataID = "";
            ModuleSN = "";
            TestTimeStamp = "";
            WhichTest = "";
            ErrAbbr = "";
            TestStation = "";
            ProductFamily = "";
            PN = "";
            PNDesc = "";
            ModuleType = "";
            SpeedRate = "";
            SpendTime = "";
            MESTab = "";
        }

        public static void StoreData(List<ModuleTestData> datalist)
        {
            var newlist = new List<object>();
            foreach (var item in datalist)
            { newlist.Add(item); }
            DBUtility.WriteDBWithTable(newlist, typeof(ModuleTestData), "ModuleTestData");
        }


        public string DataID { set; get; }
        public string ModuleSN { set; get; }
        public string TestTimeStamp { set; get; }
        public string WhichTest { set; get; }
        public string ErrAbbr { set; get; }
        public string TestStation { set; get; }
        public string ProductFamily { set; get; }

        public string PN { set; get; }
        public string PNDesc { set; get; } 
        public string ModuleType { set; get; }
        public string SpeedRate { set; get; }
        public string SpendTime { set; get; }

        public string MESTab { set; get; }

    }
}