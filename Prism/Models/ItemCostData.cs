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
            var fp = syscfg["INVENTORYTREND"];
            var localfp = ExternalDataCollector.DownloadShareFile(fp, ctrl);
            if (localfp != null)
            {
                var datalist = new List<ItemCostData>();
                var iddict = new Dictionary<string, bool>();
                var data = ExcelReader.RetrieveDataFromExcel(localfp, null);
                foreach (var line in data)
                {
                    try
                    {
                        var makebuy = line[4];
                        if (makebuy.ToUpper().Contains("MAKE"))
                        {
                            var vm = new ItemCostData();
                            vm.ID = line[1] + "_" + q;
                            vm.PN = line[1];
                            vm.FrozenCost = Convert.ToDouble(line[6]);
                            vm.FrozenMaterialCost = Convert.ToDouble(line[7]);
                            vm.FrozenResourceCost = Convert.ToDouble(line[8]);
                            vm.FrozenOverhead = Convert.ToDouble(line[9]);
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
                    var idcond = "('" + string.Join("','", iddict.Keys.ToList()) + "')";
                    DeleteData(idcond);
                }
                foreach (var item in datalist)
                { item.StoreData(); }
            }//end if
        }

        public static void DeleteData(string idcond)
        {
            var sql = "delete from ItemCostData where ID in <idcond>";
            sql = sql.Replace("<idcond>", idcond);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void StoreData()
        {
            var sql = "insert into ItemCostData(ID,PN,FrozenCost,FrozenMaterialCost,FrozenResourceCost,FrozenOverhead,PlannerCode,Quarter) values(@ID,@PN,@FrozenCost,@FrozenMaterialCost,@FrozenResourceCost,@FrozenOverhead,@PlannerCode,@Quarter)";
            var dict = new Dictionary<string, string>();
            dict.Add("@", ID);




            DBUtility.ExeLocalSqlNoRes(sql, dict);
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