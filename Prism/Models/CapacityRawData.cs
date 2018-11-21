using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class CapacityRawData
    {
        private static List<string> GetQuarterList(List<List<string>> data)
        {
            var ret = new List<string>();
            foreach (var line in data)
            {
                foreach (var item in line)
                {
                    if (item.ToUpper().Contains("Q1FY")
                        || item.ToUpper().Contains("Q2FY")
                        || item.ToUpper().Contains("Q3FY")
                        || item.ToUpper().Contains("Q4FY"))
                    {
                        ret.Add(item.ToUpper());
                    }
                }
                if (ret.Count > 0)
                { return ret; }
            }
            return ret;
        }

        private static List<int> GetMaxCapacityList(List<List<string>> data)
        {
            var ret = new List<int>();
            foreach (var line in data)
            {
                var idx = 0;
                foreach (var item in line)
                {
                    if (item.ToUpper().Contains("MAX")
                        && item.ToUpper().Contains("CAPACITY"))
                    {
                        ret.Add(idx);
                    }
                    idx++;
                }

                if (ret.Count > 0)
                { return ret; }
            }
            return ret;
        }

        public static void LoadCapacityRawData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var rmafolder = syscfg["CAPACITYSRCFOLDER"];
            var flist = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, rmafolder);
            var rmafiles = new List<string>();
            foreach (var fn in flist)
            {
                if (!fn.ToUpper().Contains("~$"))
                {
                    rmafiles.Add(fn);
                }
            }

            var capdata = new List<CapacityRawData>();
            var capidlist = new List<string>();
            foreach (var rmf in rmafiles)
            {
                var localrmaf = ExternalDataCollector.DownloadShareFile(rmf, ctrl);
                if (localrmaf != null)
                {
                    var data = ExcelReader.RetrieveDataFromExcel(localrmaf, null);
                    var quarterlist = GetQuarterList(data);
                    var capacityidxlist = GetMaxCapacityList(data);
                    if (quarterlist.Count != capacityidxlist.Count)
                    { continue; }
                    foreach (var line in data)
                    {
                        var qidx = 0;
                        foreach (var cidx in capacityidxlist)
                        {
                            var capstr = line[cidx];
                            if (string.IsNullOrEmpty(capstr))
                            {
                                qidx++;
                                continue;
                            }

                            if (string.IsNullOrEmpty(line[2])||string.IsNullOrEmpty(line[3]))
                            {
                                qidx++;
                                continue;
                            }
                            if (quarterlist[qidx].Length != 6)
                            {
                                qidx++;
                                continue;
                            }

                            var mcapacity = 0;
                            var forecast = 0;
                            try
                            {
                                mcapacity = Convert.ToInt32(capstr);
                                forecast = Convert.ToInt32(line[cidx + 1]);
                            }
                            catch (Exception) { }
                            if (mcapacity == 0)
                            {
                                qidx++;
                                continue;
                            }
                            var pn = line[2];
                            var product = line[3].Replace(",","");
                            var qs = quarterlist[qidx].Split(new string[] { "FY" }, StringSplitOptions.RemoveEmptyEntries);
                            var quarter = "20" + qs[1] + " " + qs[0];
                            var id = pn + "_" + quarter;
                            var tempvm = new CapacityRawData(id, quarter, pn, product, mcapacity, forecast);
                            if (product.ToUpper().Contains("SFP+WIRE"))
                            { tempvm.ProductType = "SFP+ WIRE"; }
                            capdata.Add(tempvm);
                            capidlist.Add(id);
                            qidx++;
                        }
                    }//end foreach line
                }//end if
            }//end foreach file

            if (capidlist.Count > 0)
            {
                CleanData(capidlist);
            }
            foreach (var data in capdata)
            { data.StoreData(); }
        }

        private static void CleanData(List<string> ids)
        {
            var idcond = "('" + string.Join("','", ids) + "')";
            var sql = "delete from CapacityRawData where ID in <idcond>";
            sql = sql.Replace("<idcond>", idcond);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreData()
        {
            var sql = "insert into CapacityRawData(ID,Quarter,PN,Product,MaxCapacity,ForeCast,ProductType) values(@ID,@Quarter,@PN,@Product,@MaxCapacity,@ForeCast,@ProductType)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ID", ID);
            dict.Add("@Quarter", Quarter);
            dict.Add("@PN", PN);
            dict.Add("@Product", Product);
            dict.Add("@MaxCapacity", MaxCapacity.ToString());
            dict.Add("@ForeCast", ForeCast.ToString());
            dict.Add("@ProductType", ProductType);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<CapacityRawData> RetrieveDataByProd(List<string> pdlist)
        {
            var ret = new List<CapacityRawData>();
            var pdcond = "('" + string.Join("','", pdlist) + "')";
            var sql = "select ID,Quarter,PN,Product,MaxCapacity,ForeCast,ProductType from CapacityRawData where Product in <pdcond> order by Product,Quarter";
            sql = sql.Replace("<pdcond>", pdcond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(new CapacityRawData(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]),
                    Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToString(line[6])));
            }
            return ret;
        }

        public static List<CapacityRawData> RetrieveDataByProductType(string prodtype)
        {
            var ret = new List<CapacityRawData>();
            var sql = "select ID,Quarter,PN,Product,MaxCapacity,ForeCast,ProductType from CapacityRawData where ProductType = @ProductType order by Product,Quarter";
            var dict = new Dictionary<string, string>();
            dict.Add("@ProductType", prodtype);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null,dict);
            foreach (var line in dbret)
            {
                ret.Add(new CapacityRawData(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]),
                    Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToString(line[6])));
            }
            return ret;
        }

        public static List<CapacityRawData> RetrieveAllData()
        {
            var ret = new List<CapacityRawData>();
            var sql = "select ID,Quarter,PN,Product,MaxCapacity,ForeCast,ProductType from CapacityRawData order by Product,Quarter";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(new CapacityRawData(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]),
                    Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToString(line[6])));
            }
            return ret;
        }

        public static List<string> GetAllProductList()
        {
            var ret = new List<string>();
            var sql = "select distinct Product from CapacityRawData order by Product";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public CapacityRawData()
        {
            ID = "";
            Quarter = "";
            PN = "";
            Product = "";
            MaxCapacity = 0;
            ForeCast = 0;
            ProductType = "Parallel";
        }

        public CapacityRawData(string id,string qt,string pn,string pd,int cap,int forecast,string prodtype= "Parallel")
        {
            ID = id;
            Quarter = qt;
            PN = pn;
            Product = pd;
            MaxCapacity = cap;
            ForeCast = forecast;
            ProductType = prodtype;
        }

        public string ID { set; get; }
        public string Quarter { set; get; }
        public string PN { set; get; }
        public string Product { set; get; }
        public double MaxCapacity { set; get; }
        public double ForeCast { set; get; }
        public string ProductType { set; get; }
        public double Usage { get {
                if (MaxCapacity == 0)
                { return 0; }
                return Math.Round(ForeCast / MaxCapacity * 100.0,2);
            } }
        public double GAP { get { return MaxCapacity-ForeCast; } }
    }
}