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
                    if (item.ToUpper().Contains("CAPACITY") && !item.ToUpper().Contains("USAGE"))
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
                    if (quarterlist.Count == 0)
                    {
                        data = ExcelReader.RetrieveDataFromExcel(localrmaf, "Project");
                        quarterlist = GetQuarterList(data);
                    }
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
                                mcapacity = Convert.ToInt32(Convert.ToDouble(capstr));
                                forecast = Convert.ToInt32(Convert.ToDouble(line[cidx + 1]));
                            }
                            catch (Exception ex) { }
                            if (mcapacity == 0)
                            {
                                qidx++;
                                continue;
                            }

                            var product = line[3].Replace(",","");
                            if (product.ToUpper().Contains("TOTAL"))
                            {
                                qidx++;
                                continue;
                            }

                            var prodtype = "";
                            var pn = line[2];
                            if (pn.ToUpper().Contains("T-XFP"))
                            {
                                pn = product;
                                prodtype = "Tunable";
                            }

                            if (pn.ToUpper().Contains("OSA"))
                            {
                                pn = product;
                                prodtype = "OSA";
                            }

                            if (product.ToUpper().Contains("SFP+WIRE"))
                            { prodtype = "SFP+ WIRE"; }

                            var qs = quarterlist[qidx].Split(new string[] { "FY" }, StringSplitOptions.RemoveEmptyEntries);
                            var quarter = "20" + qs[1] + " " + qs[0];

                            var id = pn + "_" + quarter;
                            var tempvm = new CapacityRawData(id, quarter, pn, product, mcapacity, forecast);
                            if (!string.IsNullOrEmpty(prodtype))
                            {
                                tempvm.ProductType = prodtype;
                            }

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

            UpdatePN(ctrl);
            UpdatePF();
        }

        public static void UpdatePN(Controller ctrl)
        {
            var namepnmap = CfgUtility.LoadNamePNConfig(ctrl);
            foreach (var kv in namepnmap)
            {
                var sql = "update CapacityRawData set PN=@PN where PN=@NAME";
                var dict = new Dictionary<string, string>();
                dict.Add("@PN",kv.Value);
                dict.Add("@NAME",kv.Key);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        private static List<string> GetAllPNList()
        {
            var ret = new List<string>();
            var sql = "select distinct PN from CapacityRawData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static void UpdatePF()
        {
            var pndict = PNProuctFamilyCache.PNPFDict();
            var pnlist = GetAllPNList();
            foreach (var pn in pnlist)
            {
                if (pndict.ContainsKey(pn))
                {
                    var sql = "update CapacityRawData set Product=@Product where PN=@PN";
                    var dict = new Dictionary<string, string>();
                    dict.Add("@PN",pn);
                    dict.Add("@Product", pndict[pn]);
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
            }
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

        public static object GetCapacityTable(List<CapacityRawData> capdatalist, bool fordepartment = true)
        {
            var quarterdict = new Dictionary<string, bool>();
            var pddict = new Dictionary<string, Dictionary<string, CapacityRawData>>();
            foreach (var cd in capdatalist)
            {
                if (!quarterdict.ContainsKey(cd.Quarter))
                { quarterdict.Add(cd.Quarter, true); }

                var pd = cd.ProductType;
                if (!fordepartment)
                { pd = cd.Product; }


                if (pddict.ContainsKey(pd))
                {
                    var qdict = pddict[pd];
                    if (qdict.ContainsKey(cd.Quarter))
                    {
                        qdict[cd.Quarter].MaxCapacity += cd.MaxCapacity;
                        qdict[cd.Quarter].ForeCast += cd.ForeCast;
                    }
                    else
                    {
                        var tempvm = new CapacityRawData();
                        tempvm.MaxCapacity = cd.MaxCapacity;
                        tempvm.ForeCast = cd.ForeCast;
                        qdict.Add(cd.Quarter, tempvm);
                    }
                }
                else
                {
                    var tempvm = new CapacityRawData();
                    tempvm.MaxCapacity = cd.MaxCapacity;
                    tempvm.ForeCast = cd.ForeCast;
                    var qdict = new Dictionary<string, CapacityRawData>();
                    qdict.Add(cd.Quarter, tempvm);
                    pddict.Add(pd, qdict);
                }
            }

            var qlist = quarterdict.Keys.ToList();
            qlist.Sort(delegate (string q1, string q2)
            {
                var qd1 = QuarterCLA.RetrieveDateFromQuarter(q1);
                var qd2 = QuarterCLA.RetrieveDateFromQuarter(q2);
                return qd1[0].CompareTo(qd2[0]);
            });

            var titlelist = new List<object>();
            titlelist.Add("Capacity");
            titlelist.Add("");
            titlelist.AddRange(qlist);

            var linelist = new List<object>();
            var pdlist = pddict.Keys.ToList();

            foreach (var pd in pdlist)
            {
                linelist = new List<object>();
                if (fordepartment)
                {
                    linelist.Add("<a href='/Capacity/DepartmentCapacity' target='_blank'>" + pd + "</a>");
                }
                else
                {
                    linelist.Add("<a href='/Capacity/ProductCapacity?producttype=" + HttpUtility.UrlEncode(pd) + "' target='_blank'>" + pd + "</a>");
                }

                linelist.Add("<span class='YFPY'>Max Capacity</span><br><span class='YFY'>Forecast</span><br><span class='YINPUT'>Cusume Rate%</span><br><span class='YINPUT'>Buffer</span>");

                var qtdata = pddict[pd];
                foreach (var q in qlist)
                {
                    if (qtdata.ContainsKey(q))
                    {
                        var BUFFTAG = "YINPUT";
                        if (qtdata[q].Usage > 90)
                        {
                            BUFFTAG = "NOBUFF";
                        }

                        linelist.Add("<span class='YFPY'>" + String.Format("{0:n0}", qtdata[q].MaxCapacity) + "</span><br><span class='YFY'>" + String.Format("{0:n0}", qtdata[q].ForeCast) + "</span><br><span class='" + BUFFTAG + "'>" + qtdata[q].Usage + "%</span><br><span class='" + BUFFTAG + "'>" + String.Format("{0:n0}", qtdata[q].GAP) + "</span>");
                    }
                    else
                    { linelist.Add(" "); }
                }
            }

            return new
            {
                tabletitle = titlelist,
                tablecontent = linelist,
            };
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