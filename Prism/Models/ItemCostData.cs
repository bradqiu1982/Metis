using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class ItemCostData
    {
        public static void LoadCostData(Controller ctrl)
        {
            var q = QuarterCLA.RetrieveQuarterFromDate(DateTime.Now);
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var fp = syscfg["ITEMCOSTFILE"];
            var localfp = ExternalDataCollector.DownloadShareFile(fp, ctrl);
            if (localfp != null)
            {
                var datalist = new List<ItemCostData>();
                var iddict = new Dictionary<string, bool>();
                var data = ExcelReader.RetrieveDataFromExcel(localfp, null,40);
                foreach (var line in data)
                {
                    try
                    {
                        var makebuy = line[4];
                        if (makebuy.ToUpper().Contains("MAKE") && !string.IsNullOrEmpty(line[6]))
                        {
                            var vm = new ItemCostData();
                            vm.ID = line[1] + "_" + q;
                            vm.PN = line[1];
                            vm.FrozenCost = Convert.ToDouble(line[6]);
                            if (!string.IsNullOrEmpty(line[7])) {
                                vm.FrozenMaterialCost = Convert.ToDouble(line[7]);
                            }
                            if (!string.IsNullOrEmpty(line[8]))
                            {
                                vm.FrozenResourceCost = Convert.ToDouble(line[8]);
                            }
                            if (!string.IsNullOrEmpty(line[9]))
                            {
                                vm.FrozenOverhead = Convert.ToDouble(line[9]);
                            }
                                                        
                            vm.PlannerCode = line[34];
                            vm.Quarter = q;
                            if (!iddict.ContainsKey(vm.ID))
                            {
                                iddict.Add(vm.ID, true);
                                datalist.Add(vm);
                            }
                        }
                    }
                    catch (Exception ex) { }
                }//end foreach

                if (iddict.Count > 0)
                {
                    DeleteData(q);
                }
                foreach (var item in datalist)
                { item.StoreData(); }
            }//end if
        }

        public static void DeleteData(string q)
        {
            var sql = "delete from ItemCostData where Quarter = '<Quarter>'";
            sql = sql.Replace("<Quarter>", q);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void StoreData()
        {
            var sql = "insert into ItemCostData(ID,PN,FrozenCost,FrozenMaterialCost,FrozenResourceCost,FrozenOverhead,PlannerCode,Quarter) values(@ID,@PN,@FrozenCost,@FrozenMaterialCost,@FrozenResourceCost,@FrozenOverhead,@PlannerCode,@Quarter)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ID", ID);
            dict.Add("@PN", PN);
            dict.Add("@FrozenCost", FrozenCost.ToString());
            dict.Add("@FrozenMaterialCost", FrozenMaterialCost.ToString());

            dict.Add("@FrozenResourceCost", FrozenResourceCost.ToString());
            dict.Add("@FrozenOverhead", FrozenOverhead.ToString());
            dict.Add("@PlannerCode", PlannerCode);
            dict.Add("@Quarter", Quarter);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, double> RetrieveStandardCost()
        {
            var ret = new Dictionary<string, double>();
            var q = QuarterCLA.RetrieveQuarterFromDate(DateTime.Now);
            var sql = "select PN,FrozenCost from ItemCostData where Quarter = '<Quarter>'";
            sql = sql.Replace("<Quarter>", q);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                var cost = Convert.ToDouble(line[1]);
                if (!ret.ContainsKey(pn)) {
                    ret.Add(pn, cost);
                }
            }
            return ret;
        }
        
        public ItemCostData()
        {
            ID = "";
            PN = "";
            FrozenCost = 0.0;
            FrozenMaterialCost = 0.0;
            FrozenResourceCost = 0.0;
            FrozenOverhead = 0.0;
            PlannerCode = "";
            Quarter = "";
        }

        public string ID { set; get; }
        public string PN { set; get; }
        public double FrozenCost { set; get; }
        public double FrozenMaterialCost { set; get; }
        public double FrozenResourceCost { set; get; }
        public double FrozenOverhead { set; get; }
        public string PlannerCode { set; get; }
        public string Quarter { set; get; }
    }

}