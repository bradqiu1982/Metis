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
                        if (line.Count > 6 && !string.IsNullOrEmpty(line[1]) && !string.IsNullOrEmpty(line[6]))
                        {
                            var vm = new ItemCostData();
                            vm.ID = line[1] + "_" + q;
                            vm.PN = line[1];
                            vm.FrozenCost = Convert.ToDouble(line[6]);

                            if (line.Count > 7 && !string.IsNullOrEmpty(line[7])) {
                                vm.FrozenMaterialCost = Convert.ToDouble(line[7]);
                            }
                            if (line.Count > 8 && !string.IsNullOrEmpty(line[8]))
                            {
                                vm.FrozenResourceCost = Convert.ToDouble(line[8]);
                            }
                            if (line.Count > 9 && !string.IsNullOrEmpty(line[9]))
                            {
                                vm.FrozenOverhead = Convert.ToDouble(line[9]);
                            }
                            if (line.Count > 34) { vm.PlannerCode = line[34]; }              
                            
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

        public static void LoadMonthlyCostData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var mdict = GetMonthDict();

            var flist = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, syscfg["MONTHLYCOSTFILE"]);
            foreach (var fs in flist)
            {
                if (fs.ToUpper().Contains("ITEM COST") && fs.ToUpper().Contains("(20"))
                {
                    var fn = System.IO.Path.GetFileNameWithoutExtension(fs);
                    var dt = fn.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    var month = dt.Substring(0, 7);
                    if (!dt.Contains("-"))
                    { month = dt.Substring(0, 4) + "-" + dt.Substring(4, 2); }

                    if (mdict.ContainsKey(month))
                    { continue; }

                    var iddict = new Dictionary<string, bool>();
                    var monthcostlist = new List<ItemCostData>();
                    var idx = 0;
                    var data = ExcelReader.RetrieveDataFromExcel(fs, null, 13);
                    foreach (var line in data)
                    {
                        if (idx == 0)
                        {
                            idx++;  continue;
                        }
                        if (line.Count > 7)
                        {
                            if (string.IsNullOrEmpty(line[6].Trim()))
                            { continue; }

                            var tempvm = new ItemCostData();
                            tempvm.PN = line[1];
                            tempvm.FrozenCost = UT.O2D(line[6]);
                            tempvm.ID = tempvm.PN + "_" + month;
                            tempvm.Quarter = month;
                            if (!iddict.ContainsKey(tempvm.ID))
                            {
                                iddict.Add(tempvm.ID, true);
                                monthcostlist.Add(tempvm);
                            }
                        }
                        
                    }

                    if (monthcostlist.Count > 0)
                    {
                        foreach (var item in monthcostlist)
                        { item.StoreMonthCost(); }
                    }
                }//end if
            }//end foreach

        }

        private void StoreMonthCost()
        {
            var sql = @"insert into WUXIMonthlyCost(ID,Cost,Months) values(@ID,@Cost,@Months)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ID", ID);
            dict.Add("@Cost", FrozenCost.ToString());
            dict.Add("@Months", Quarter);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static Dictionary<string, bool> GetMonthDict()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct Months from WUXIMonthlyCost";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var m = UT.O2S(line[0]);
                if (!ret.ContainsKey(m))
                { ret.Add(m, true); }
            }
            return ret;
        }

        public static Dictionary<string, double> GetMonthlyCost()
        {
            var ret = new Dictionary<string, double>();

            var sql = "select ID,Cost from WUXIMonthlyCost";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var id = UT.O2S(line[0]);
                var cost = UT.O2D(line[1]);
                if (!ret.ContainsKey(id))
                { ret.Add(id, cost); }
            }

            return ret;
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

        public static Dictionary<string, double> RetrieveQuartCost(string series)
        {
            var ret = new Dictionary<string, double>();
            var sql = @"select distinct ID,FrozenCost,Quarter from ItemCostData where pn in 
                      (SELECT PN FROM [BSSupport].[dbo].[PNBUMap] where series = @series)";

            var dict = new Dictionary<string, string>();
            dict.Add("@series", series);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null,dict);
            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                var cost = Convert.ToDouble(line[1]);
                var q = Convert.ToString(line[2]);

                if (!ret.ContainsKey(pn))
                {
                    ret.Add(pn, cost);
                }

                if (!ret.ContainsKey(q))
                { ret.Add(q, cost); }
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