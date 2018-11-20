using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class InventoryData
    {
        public static void LoadInventoryTrend(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var fp = syscfg["INVENTORYTREND"];
            var localfp = ExternalDataCollector.DownloadShareFile(fp, ctrl);
            if (localfp != null)
            {
                var datalist = new List<InventoryData>();
                var iddict = new Dictionary<string, bool>();

                var data = ExcelReader.RetrieveDataFromExcel(localfp, null);
                foreach (var line in data)
                {
                    try
                    {
                        if (line[0].ToUpper().Contains("FY") && line[0].ToUpper().Contains("Q"))
                        {
                            var quarter = line[0].ToUpper().Trim().Replace("FY", "20").Replace("-ACTUAL", "");
                            var department = line[1];
                            var id = quarter + "_" + department;
                            var cogs = Convert.ToDouble(line[2].Replace("$", ""));
                            var inventory = Convert.ToDouble(line[3].Replace("$", ""));
                            var inventoryturns = Convert.ToDouble(line[4]);

                            datalist.Add(new InventoryData(id,quarter,department,"",cogs,inventory,inventoryturns));
                            if (!iddict.ContainsKey(id))
                            { iddict.Add(id, true); }
                        }
                    }
                    catch (Exception ex) { }
                }//end foreach

                if (datalist.Count > 0)
                {
                    var idcond = "('" + string.Join("','", iddict.Keys.ToList())+"')";
                    CleanTrendTable(idcond);
                    foreach (var item in datalist)
                    {
                        item.StoreTrendData();
                    }
                }
            }
        }

        public static void LoadInventoryDetail(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var fp = syscfg["INVENTORYDETAIL"];
            var localfp = ExternalDataCollector.DownloadShareFile(fp, ctrl);
            if (localfp != null)
            {
                var datalist = new List<InventoryData>();
                var iddict = new Dictionary<string, bool>();

                var data = ExcelReader.RetrieveDataFromExcel(localfp, null);
                foreach (var line in data)
                {
                    try
                    {
                        if (line[0].ToUpper().Contains("FY") && line[0].ToUpper().Contains("Q"))
                        {
                            var quarter = line[0].ToUpper().Trim().Replace("FY", "20").Replace("-ACTUAL", "");
                            var department = line[1];
                            var product = line[2];
                            var id = quarter + "_" + department+"_"  + product;
                            var cogs = Convert.ToDouble(line[3].Replace("$", ""));
                            var inventory = Convert.ToDouble(line[4].Replace("$", ""));
                            var inventoryturns = Convert.ToDouble(line[5]);

                            datalist.Add(new InventoryData(id, quarter, department, product, cogs, inventory, inventoryturns));
                            if (!iddict.ContainsKey(id))
                            { iddict.Add(id, true); }
                        }
                    }
                    catch (Exception ex) { }
                }//end foreach

                if (datalist.Count > 0)
                {
                    var idcond = "('" + string.Join("','", iddict.Keys.ToList()) + "')";
                    CleanDetailTable(idcond);
                    foreach (var item in datalist)
                    {
                        item.StoreDetailData();
                    }
                }
            }
        }

        private static void CleanTable(string idcond,string tablename = "InventoryTrend")
        {
            var sql = "delete from "+tablename+" where ID in <idcond>";
            sql = sql.Replace("<idcond>", idcond);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreData(string tablename= "InventoryTrend")
        {
            var sql = "insert into "+ tablename + "(ID,Quarter,Department,Product,COGS,Inventory,InventoryTurns) values(@ID,@Quarter,@Department,@Product,@COGS,@Inventory,@InventoryTurns)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ID",ID);
            dict.Add("@Quarter",Quarter);
            dict.Add("@Department",Department);
            dict.Add("@Product",Product);
            dict.Add("@COGS",COGS.ToString());
            dict.Add("@Inventory",Inventory.ToString());
            dict.Add("@InventoryTurns",InventoryTurns.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void CleanTrendTable(string idcond)
        {
            CleanTable(idcond);
        }

        private static void CleanDetailTable(string idcond)
        {
            CleanTable(idcond, "InventoryDetail");
        }

        private void StoreTrendData()
        {
            StoreData();
        }

        private void StoreDetailData()
        {
            StoreData("InventoryDetail");
        }

        public static List<InventoryData> RetrieveAllTrendData()
        {
            var ret = new List<InventoryData>();
            var sql = "select ID,Quarter,Department,Product,COGS,Inventory,InventoryTurns from InventoryTrend order by Department";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(new InventoryData(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3])
                    , Convert.ToDouble(line[4]), Convert.ToDouble(line[5]), Convert.ToDouble(line[6])));
            }

            return ret;
        }

        public static List<InventoryData> RetrieveDetailDataByDP(string department, string quarter)
        {
            var ret = new List<InventoryData>();
            var sql = "select ID,Quarter,Department,Product,COGS,Inventory,InventoryTurns from InventoryDetail where Department = '<Department>' ";
            if (!string.IsNullOrEmpty(quarter))
            {
                sql += " and Quarter = '<Quarter>' ";
                sql = sql.Replace("<Quarter>", quarter);
            }
            sql = sql.Replace("<Department>",department);
            sql += " order by InventoryTurns desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(new InventoryData(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3])
                    , Convert.ToDouble(line[4]), Convert.ToDouble(line[5]), Convert.ToDouble(line[6])));
            }

            return ret;
        }

        public static List<string> GetAllProductList()
        {
            var ret = new List<string>();
            var sql = "select distinct Product from InventoryDetail order by Product";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public InventoryData(string id, string qt, string dp, string pd,double co, double iv, double ivt)
        {
            ID = id;
            Quarter = qt;
            Department = dp;
            Product = pd;
            COGS = co;
            Inventory = iv;
            InventoryTurns = ivt;
        }

        public InventoryData()
        {
            ID = "";
            Quarter = "";
            Department = "";
            Product = "";
            COGS = 0;
            Inventory = 0;
            InventoryTurns = 0;
        }

        public string ID { set; get; }
        public string Quarter { set; get; }
        public string Department { set; get; }
        public string Product { set; get; }
        public double COGS { set; get; }
        public double Inventory { set; get; }
        public double InventoryTurns { set; get; }

        //public double ActualTurns {
        //    get {
        //        if (COGS == 0)
        //        { return 0; }
        //        if (Inventory == 0)
        //        { return 0; }

        //        var CurrentQ = QuarterCLA.RetrieveQuarterFromDate(DateTime.Now);
                
        //        if (string.Compare(CurrentQ, Quarter) == 0)
        //        {
        //            var startdate = QuarterCLA.RetrieveDateFromQuarter(CurrentQ)[0];
        //            var days = Convert.ToInt32((DateTime.Now - startdate).Days + 1);
        //            var ActualInventory = (double)days / (double)(7 * 13) * Inventory;
        //            return Math.Round(4.0 * COGS / ActualInventory,2);
        //        }
        //        else
        //        {
        //            return InventoryTurns;
        //        }
        //    }
        //}
    }
}