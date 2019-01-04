using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class VCSELRATE
    {
        public static string r25G = "25G";
        public static string r10G = "10G";
        public static string r14G = "14G";
        public static string r48G = "48G";
    }

    public class SHIPPRODTYPE
    {
        public static string AZNA_CML = "AZNA-CML";
        public static string U2T = "U2T";
        public static string IPX = "IPX";
        public static string INTERLEAVE = "INTERLEAVE";
        public static string SYNTUNE = "SYNTUNE";
        public static string OPTIUM = "OPTIUM";
        public static string DWDMPASS = "DWDMPASS";
        public static string RED_C = "RED-C";
        public static string CWDMPASS = "CWDMPASS";
        public static string PARALLEL = "PARALLEL";
        public static string ICDR = "ICDR";
        public static string OTHER = "OTHER";
        public static string I32X = "I32X";
        public static string SMART_LDPA = "SMART LDPA";
        public static string TUNABLE = "TUNABLE";
        public static string SFPWIRE = "SFP+ WIRE";
        public static string EDFA = "EDFA";
    }

    public class FsrShipData
    {
        public FsrShipData()
        {
            ShipID = string.Empty;
            ShipQty = 0;
            PN = string.Empty;
            ProdDesc = string.Empty;
            MarketFamily = string.Empty;
            Configuration = string.Empty;
            ShipDate = DateTime.Parse("1982-05-06 10:00:00");
            CustomerNum = string.Empty;
            Customer1 = string.Empty;
            Customer2 = string.Empty;
            OrderedDate = DateTime.Parse("1982-05-06 10:00:00");
            DelieveNum = string.Empty;
            VcselType = string.Empty;
            SN = string.Empty;
            Wafer = string.Empty;
            OrderQty = 0;
            OPD = DateTime.Parse("1982-05-06 10:00:00");
            OTD = "NO";
            ShipTo = string.Empty;
            Cost = 0;
            Output = 0;
        }

        public FsrShipData(string id, int qty, string pn, string pndesc, string family, string cfg
            , DateTime shipdate, string custnum, string cust1, string cust2, DateTime orddate, string delievenum, int orderqty, DateTime opd)
        {
            ShipID = id;
            ShipQty = qty;
            PN = pn;
            ProdDesc = pndesc;
            MarketFamily = family;
            Configuration = cfg;
            ShipDate = shipdate;
            CustomerNum = custnum;
            Customer1 = cust1;
            Customer2 = cust2;
            OrderedDate = orddate;
            DelieveNum = delievenum;
            VcselType = string.Empty;
            SN = string.Empty;
            Wafer = string.Empty;
            OrderQty = orderqty;
            OPD = opd;
            OTD = "NO";
        }



        private static string RetrieveCustome(string cust1, string cust2, Dictionary<string, string> custdict)
        {
            var ckeylist = custdict.Keys.ToList();
            var realcust = string.Empty;
            foreach (var c in ckeylist)
            {
                if (cust1.Contains(c) || cust2.Contains(c))
                {
                    realcust = c;
                }
            }
            if (string.IsNullOrEmpty(realcust))
            {
                return "OTHERS";
            }
            else
            {
                return custdict[realcust];
            }
        }

        //<date,<customer,int>>
        public static Dictionary<string, Dictionary<string, double>> RetrieveShipDataByMonth(string rate, string producttype, string sdate, string edate, Controller ctrl)
        {
            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            if (pnlist.Count == 0)
            {
                return new Dictionary<string, Dictionary<string, double>>();
            }

            var pncond = "('" + string.Join("','", pnlist) + "')";

            var ret = new Dictionary<string, Dictionary<string, double>>();
            var custdict = CfgUtility.GetAllCustConfig(ctrl);
            var sql = @"select ShipQty,Customer1,Customer2,ShipDate from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate  and PN in <pncond>  ";

            if (string.Compare(rate, VCSELRATE.r14G, true) == 0)
            { sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')"; }
            else if (string.Compare(rate, VCSELRATE.r25G, true) == 0)
            { sql = sql + " and VcselType = '" + rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            sql = sql.Replace("<pncond>", pncond);

            var sumqty = 0.0;
            var realcustdict = new Dictionary<string, double>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null, dict);
            foreach (var line in dbret)
            {
                var shipdate = Convert.ToDateTime(line[3]).ToString("yyyy-MM");
                var qty = Convert.ToDouble(line[0]);
                var cust1 = Convert.ToString(line[1]).ToUpper();
                var cust2 = Convert.ToString(line[2]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);

                if (!realcustdict.ContainsKey(realcust))
                { realcustdict.Add(realcust, qty); }
                else
                { realcustdict[realcust] += qty; }
                sumqty += qty;

                if (ret.ContainsKey(shipdate))
                {
                    var shipdict = ret[shipdate];
                    if (shipdict.ContainsKey(realcust))
                    { shipdict[realcust] = shipdict[realcust] + qty; }
                    else
                    { shipdict.Add(realcust, qty); }
                }
                else
                {
                    var custcntdict = new Dictionary<string, double>();
                    custcntdict.Add(realcust, qty);
                    ret.Add(shipdate, custcntdict);
                }
            }

            //find the small customer, < 1%
            var removecustdict = new Dictionary<string, bool>();
            foreach (var kv in realcustdict)
            {
                if (kv.Value / sumqty < 0.01)
                {
                    removecustdict.Add(kv.Key, true);
                }
            }


            var shipdatelist = ret.Keys.ToList();
            var realcustlist = realcustdict.Keys.ToList();
            foreach (var sd in shipdatelist)
            {
                var custcntdict = ret[sd];
                var movetoothersqty = 0.0; 
                foreach (var c in removecustdict)
                {
                    if (custcntdict.ContainsKey(c.Key))
                    {
                        movetoothersqty += custcntdict[c.Key];
                        //clean customer
                        custcntdict.Remove(c.Key);
                    }
                }

                if (custcntdict.ContainsKey("OTHERS"))
                { custcntdict["OTHERS"] += movetoothersqty; }
                else
                { custcntdict.Add("OTHERS",movetoothersqty); }
            }

            //clean small customer from total customer list
            foreach (var ckv in removecustdict)
            {
                if (realcustdict.ContainsKey(ckv.Key))
                {
                    realcustdict.Remove(ckv.Key);
                }
            }

            realcustlist = realcustdict.Keys.ToList();
            //complete the big customer with 0,if it does not buy module at that month
            foreach (var sd in shipdatelist)
            {
                var custcntdict = ret[sd];
                foreach (var c in realcustlist)
                {
                    if (!custcntdict.ContainsKey(c))
                    {
                        custcntdict.Add(c, 0.0);
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, Dictionary<string, double>> RetrieveLineCardShipDataByMonth(string sdate, string edate, Controller ctrl)
        {
            var ret = new Dictionary<string, Dictionary<string, double>>();
            var custdict = CfgUtility.GetAllCustConfig(ctrl);
            var sql = @"select ShipQty,Customer1,Customer2,ShipDate from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate and ProdDesc like '%LINECARD%' ";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);


            var realcustdict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var shipdate = Convert.ToDateTime(line[3]).ToString("yyyy-MM");
                var qty = Convert.ToDouble(line[0]);
                var cust1 = Convert.ToString(line[1]).ToUpper();
                var cust2 = Convert.ToString(line[2]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);

                if (!realcustdict.ContainsKey(realcust))
                { realcustdict.Add(realcust, true); }

                if (ret.ContainsKey(shipdate))
                {
                    var shipdict = ret[shipdate];
                    if (shipdict.ContainsKey(realcust))
                    { shipdict[realcust] = shipdict[realcust] + qty; }
                    else
                    { shipdict.Add(realcust, qty); }
                }
                else
                {
                    var custcntdict = new Dictionary<string, double>();
                    custcntdict.Add(realcust, qty);
                    ret.Add(shipdate, custcntdict);
                }
            }

            var shipdatelist = ret.Keys.ToList();
            var realcustlist = realcustdict.Keys.ToList();

            foreach (var sd in shipdatelist)
            {
                var custcntdict = ret[sd];
                foreach (var c in realcustlist)
                {
                    if (!custcntdict.ContainsKey(c))
                    {
                        custcntdict.Add(c, 0.0);
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, Dictionary<string, double>> RetrieveOrderDataByMonth(string rate, string producttype, string sdate, string edate, Controller ctrl)
        {

            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            if (pnlist.Count == 0)
            {
                return new Dictionary<string, Dictionary<string, double>>();
            }

            var pncond = "('" + string.Join("','", pnlist) + "')";


            var ret = new Dictionary<string, Dictionary<string, double>>();
            var custdict = CfgUtility.GetAllCustConfig(ctrl);
            var sql = @"select Appv_1,Customer1,Customer2,OrderedDate from FsrShipData where OrderedDate >= @sdate and OrderedDate <= @edate and PN in <pncond>  ";

            if (string.Compare(rate, VCSELRATE.r14G, true) == 0)
            { sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')"; }
            else if (string.Compare(rate, VCSELRATE.r25G, true) == 0)
            { sql = sql + " and VcselType = '" + rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            sql = sql.Replace("<pncond>", pncond);

            var realcustdict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var shipdate = Convert.ToDateTime(line[3]).ToString("yyyy-MM");
                var qty = Convert.ToDouble(line[0]);
                var cust1 = Convert.ToString(line[1]).ToUpper();
                var cust2 = Convert.ToString(line[2]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);

                if (!realcustdict.ContainsKey(realcust))
                { realcustdict.Add(realcust, true); }

                if (ret.ContainsKey(shipdate))
                {
                    var shipdict = ret[shipdate];
                    if (shipdict.ContainsKey(realcust))
                    { shipdict[realcust] = shipdict[realcust] + qty; }
                    else
                    { shipdict.Add(realcust, qty); }
                }
                else
                {
                    var custcntdict = new Dictionary<string, double>();
                    custcntdict.Add(realcust, qty);
                    ret.Add(shipdate, custcntdict);
                }
            }

            var shipdatelist = ret.Keys.ToList();
            var realcustlist = realcustdict.Keys.ToList();

            foreach (var sd in shipdatelist)
            {
                var custcntdict = ret[sd];
                foreach (var c in realcustlist)
                {
                    if (!custcntdict.ContainsKey(c))
                    {
                        custcntdict.Add(c, 0.0);
                    }
                }
            }

            return ret;
        }

        public static List<FsrShipData> RetrieveOTDByMonth( string producttype, string sdate, string edate, Controller ctrl)
        {
            var custdict = CfgUtility.GetAllCustConfig(ctrl);
            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            if (pnlist.Count == 0)
            {
                return new List<FsrShipData>();
            }

            var pncond = "('" + string.Join("','", pnlist) + "')";

            var ret = new List<FsrShipData>();
            var sql = @"select ShipDate,Appv_5,PN,ProdDesc,Appv_1,MarketFamily,ShipID,Customer1,Customer2 from FsrShipData where Appv_5 >= @sdate and Appv_5 <= @edate and PN in <pncond>   
                        and Customer1  not like '%FINISAR%' and Customer2 not like  '%FINISAR%' ";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);

            sql = sql.Replace("<pncond>", pncond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new FsrShipData();
                tempvm.ShipDate = Convert.ToDateTime(line[0]);
                tempvm.OPD = Convert.ToDateTime(line[1]);
                tempvm.PN = Convert.ToString(line[2]);
                tempvm.ProdDesc = Convert.ToString(line[3]);
                tempvm.OrderQty = Convert.ToDouble(line[4]);
                tempvm.MarketFamily = Convert.ToString(line[5]);
                tempvm.ShipID = Convert.ToString(line[6]);
                var cust1 = Convert.ToString(line[7]).ToUpper();
                var cust2 = Convert.ToString(line[8]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);
                tempvm.Customer1 = realcust;
                tempvm.OTD = "NO";

                if (string.Compare(tempvm.OPD.ToString("yyyy-MM"), "1982-05") == 0)
                { continue; }

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<FsrShipData> RetrieveLBSDataByMonth(string producttype, string sdate, string edate, Controller ctrl)
        {
            var custdict = CfgUtility.GetAllCustConfig(ctrl);
            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            if (pnlist.Count == 0)
            {
                return new List<FsrShipData>();
            }

            var pncond = "('" + string.Join("','", pnlist) + "')";

            var ret = new List<FsrShipData>();
            var sql = @"select ShipQty,Appv_2,Customer1,Customer2 from FsrShipData where  ShipDate >= @sdate and ShipDate <= @edate and PN in <pncond>  
                        and Customer1  not like '%FINISAR%' and Customer2 not like  '%FINISAR%' and  Appv_2 <> ''";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);

            sql = sql.Replace("<pncond>", pncond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new FsrShipData();
                tempvm.ShipQty = Convert.ToDouble(line[0]);
                tempvm.ShipTo = Convert.ToString(line[1]);
                if (string.Compare(tempvm.ShipTo, "HK",true) == 0)
                { tempvm.ShipTo = "CN"; }

                var cust1 = Convert.ToString(line[2]).ToUpper();
                var cust2 = Convert.ToString(line[3]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);
                tempvm.Customer1 = realcust;
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<FsrShipData> RetrieveAllShipDataByMonth(string sdate, string edate, Controller ctrl)
        {
            var ret = new List<FsrShipData>();
            var custdict = CfgUtility.GetAllCustConfig(ctrl);
            var sql = @"select ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,ShipDate,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum,VcselType,Appv_1,Appv_5 
                         from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate order by ShipDate ASC";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);

            var realcustdict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var cust1 = Convert.ToString(line[8]).ToUpper();
                var cust2 = Convert.ToString(line[9]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);
                var tempvm = new FsrShipData(Convert.ToString(line[0]), Convert.ToInt32(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToDateTime(line[6]), Convert.ToString(line[7]), realcust
                    , Convert.ToString(line[9]), Convert.ToDateTime(line[10]), Convert.ToString(line[11]), Convert.ToInt32(line[13]), Convert.ToDateTime(line[14]));
                tempvm.VcselType = Convert.ToString(line[12]);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static Dictionary<string, Dictionary<string, double>> RetrieveOutputData(Controller ctrl,string startdate,string enddate)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var costdict = ItemCostData.RetrieveStandardCost();
            var dplist = syscfg["OUTPUTDEPARTMENTS"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var ret = new Dictionary<string, Dictionary<string, double>>();
            foreach (var dp in dplist)
            {
                if (dp.Contains("COMPONENT")
                    || dp.Contains("DATACOM LW TRX"))
                {
                    var dpdata = RetrieveOutputDataByMonthWithScrapPN(dp,startdate,enddate , costdict, ctrl);
                    if (dpdata.Count > 0)
                    {
                        ret.Add(dp, dpdata);
                    }
                }
                else if (dp.Contains("WSS")
                    || dp.Contains("TELECOM TRX")
                    || dp.Contains("PARALLEL")
                    || dp.Contains("OSA")
                    || dp.Contains("LNCD") 
                    || dp.Contains("SFP+ WIRE"))
                {
                    //var sdate = DateTime.Parse(startdate);
                    //if (sdate < DateTime.Parse("2018-05-01 00:00:00"))
                    //{//more accuracy before 2019 Q1

                        var dpdata = RetrieveOutputDataByMonth(dp, startdate, enddate, costdict, ctrl);
                        if (dpdata.Count > 0)
                        {
                            ret.Add(dp, dpdata);
                        }
                    //}
                    //else
                    //{//more accuracy after 2019 Q1
                    //    var dpdata = RetrieveOutputDataByMonthWithScrapPN(dp, startdate, enddate, costdict, ctrl);
                    //    if (dpdata.Count > 0)
                    //    {
                    //        ret.Add(dp, dpdata);
                    //    }
                    //}
                }
                else
                {
                    var dpdata = RetrieveOutputDataByMonth(dp, startdate, enddate, costdict, ctrl);
                    if (dpdata.Count > 0)
                    {
                        ret.Add(dp, dpdata);
                    }
                }

            }
            return ret;
        }

        private static  Dictionary<string, double> RetrieveOutputDataByMonth(string producttype, string sdate, string edate,Dictionary<string,double> costdict, Controller ctrl)
        {
            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            if (pnlist.Count == 0)
            {
                return new Dictionary<string, double>();
            }

            var pncond = "('" + string.Join("','", pnlist) + "')";

            var ret = new  Dictionary<string, double>();
            var usdrate = CfgUtility.GetUSDRate(ctrl);
            var sql = @"select ShipQty,ShipDate,PN from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate and PN in <pncond> ";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            sql = sql.Replace("<pncond>", pncond);

            var realcustdict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                
                var qty = Convert.ToDouble(line[0]);
                var shipdate = Convert.ToDateTime(line[1]);
                var pn = Convert.ToString(line[2]).Trim();
                var q = QuarterCLA.RetrieveQuarterFromDate(shipdate);

                var m = shipdate.ToString("yyyy-MM");
                var urate = 7.0;
                if (usdrate.ContainsKey(m))
                {
                    urate = usdrate[m];
                }
                else
                { urate = usdrate["CURRENT"]; }

                if (costdict.ContainsKey(pn))
                {
                    if (ret.ContainsKey(q))
                    {
                        ret[q] += qty * costdict[pn]/urate;
                    }
                    else
                    {
                        ret.Add(q, qty * costdict[pn] /urate);
                    }
                }
            }

            return ret;
        }

        private static Dictionary<string, double> RetrieveOutputDataByMonthWithScrapPN(string producttype, string sdate, string edate, Dictionary<string, double> costdict, Controller ctrl)
        {
            var pnlist = ScrapData_Base.RetrievePNByPG(producttype);
            if (pnlist.Count == 0)
            {
                return new Dictionary<string, double>();
            }

            var pncond = "('" + string.Join("','", pnlist) + "')";

            var ret = new Dictionary<string, double>();
            var usdrate = CfgUtility.GetUSDRate(ctrl);

            var sql = @"select ShipQty,ShipDate,PN from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate and PN in <PNCOND>";
            sql = sql.Replace("<PNCOND>", pncond);

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var realcustdict = new Dictionary<string, bool>();

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                
                var qty = Convert.ToDouble(line[0]);
                var shipdate = Convert.ToDateTime(line[1]);
                var pn = Convert.ToString(line[2]).Trim();
                var q = QuarterCLA.RetrieveQuarterFromDate(shipdate);

                var m = shipdate.ToString("yyyy-MM");
                var urate = 7.0;
                if (usdrate.ContainsKey(m))
                {
                    urate = usdrate[m];
                }
                else
                { urate = usdrate["CURRENT"]; }

                if (costdict.ContainsKey(pn))
                {
                    if (ret.ContainsKey(q))
                    {
                        ret[q] += qty * costdict[pn] / urate;
                    }
                    else
                    {
                        ret.Add(q, qty * costdict[pn] / urate);
                    }
                }
            }

            return ret;
        }

        public static List<FsrShipData> RetrieveOutputDetailData(Controller ctrl,string dp, string startdate, string enddate)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var costdict = ItemCostData.RetrieveStandardCost();
            var ret = new List<FsrShipData>();

                if (dp.Contains("COMPONENT")
                    || dp.Contains("DATACOM LW TRX")
                    )
                {
                    return RetrieveOutputDetailDataWithScrapPN(dp, startdate, enddate, costdict, ctrl);
                }
                else if (dp.Contains("WSS")
                    || dp.Contains("TELECOM TRX")
                    || dp.Contains("PARALLEL")
                    || dp.Contains("OSA")
                    || dp.Contains("LNCD")
                    || dp.Contains("SFP+ WIRE"))
                {
                    //var sdate = DateTime.Parse(startdate);
                    //if (sdate < DateTime.Parse("2018-05-01 00:00:00"))
                    //{//more accuracy before 2019 Q1

                        return RetrieveOutputDetailData(dp, startdate, enddate, costdict, ctrl);
                    //}
                    //else
                    //{//more accuracy after 2019 Q1
                    //    return RetrieveOutputDetailDataWithScrapPN(dp, startdate, enddate, costdict, ctrl);
                    //}
                }
                else
                {
                    return RetrieveOutputDetailData(dp, startdate, enddate, costdict, ctrl);
                }
        }

        private static List<FsrShipData> RetrieveOutputDetailData(string producttype, string sdate, string edate, Dictionary<string, double> costdict, Controller ctrl)
        {
            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            if (pnlist.Count == 0)
            {
                return new List<FsrShipData>();
            }
            var pncond = "('" + string.Join("','", pnlist) + "')";

            var retdata = new Dictionary<string, FsrShipData>();
            var usdrate = CfgUtility.GetUSDRate(ctrl);
            var pnpjmap = PNPlannerCodeMap.RetrieveAllMaps();
            var pnpdmap = PNProuctFamilyCache.PNPFDict();

            var sql = @"select ShipQty,ShipDate,PN,MarketFamily from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate and PN in <pncond> ";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            sql = sql.Replace("<pncond>", pncond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                
                var qty = Convert.ToDouble(line[0]);
                var shipdate = Convert.ToDateTime(line[1]);
                var pn = Convert.ToString(line[2]).Trim();
                var mf = Convert.ToString(line[3]);

                var pj = "";
                if (pnpdmap.ContainsKey(pn))
                {
                    pj = pnpdmap[pn];
                }
                else if (pnpjmap.ContainsKey(pn))
                {
                    pj = pnpjmap[pn].PJName;
                }
                else
                {
                    if (!string.IsNullOrEmpty(mf))
                    { pj = mf; }
                    else
                    { pj = pn; }
                }


                var m = shipdate.ToString("yyyy-MM");
                var urate = 7.0;
                if (usdrate.ContainsKey(m))
                {
                    urate = usdrate[m];
                }
                else
                { urate = usdrate["CURRENT"]; }

                if (costdict.ContainsKey(pn))
                {
                    var output = qty * costdict[pn] / urate;
                    if (retdata.ContainsKey(pj))
                    {
                        retdata[pj].ShipQty += qty;
                        retdata[pj].Output += output;
                    }
                    else
                    {
                        var vm = new FsrShipData();
                        vm.MarketFamily = pj;
                        vm.ShipQty = qty;
                        vm.PN = pn;
                        vm.Cost = costdict[pn]/urate;
                        vm.Output = output;
                        retdata.Add(pj, vm);
                    }
                }
            }

            var ret = retdata.Values.ToList();
            ret.Sort(delegate (FsrShipData obj1, FsrShipData obj2)
            {
                return obj2.Output.CompareTo(obj1.Output);
            });
            foreach (var item in ret)
            {
                item.Output = Math.Round(item.Output, 3);
            }
            return ret;
        }

        private static List<FsrShipData> RetrieveOutputDetailDataWithScrapPN(string producttype, string sdate, string edate, Dictionary<string, double> costdict, Controller ctrl)
        {
            var pnlist = ScrapData_Base.RetrievePNByPG(producttype);
            if (pnlist.Count == 0)
            {
                return new List<FsrShipData>();
            }

            var pncond = "('" + string.Join("','", pnlist) + "')";

            var retdata = new Dictionary<string, FsrShipData>();
            var usdrate = CfgUtility.GetUSDRate(ctrl);

            var pnpjmap = PNPlannerCodeMap.RetrieveAllMaps();
            var pnpdmap = PNProuctFamilyCache.PNPFDict();

            var sql = @"select ShipQty,ShipDate,PN,MarketFamily from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate and PN in <PNCOND>";
            sql = sql.Replace("<PNCOND>", pncond);

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                
                var qty = Convert.ToDouble(line[0]);
                var shipdate = Convert.ToDateTime(line[1]);
                var pn = Convert.ToString(line[2]).Trim();
                var mf = Convert.ToString(line[3]);

                var pj = "";
                if (pnpdmap.ContainsKey(pn))
                {
                    pj = pnpdmap[pn];
                }
                else if (pnpjmap.ContainsKey(pn))
                {
                    pj = pnpjmap[pn].PJName;
                }
                else
                {
                    if (!string.IsNullOrEmpty(mf))
                    { pj = mf; }
                    else
                    { pj = pn; }
                }

                var m = shipdate.ToString("yyyy-MM");
                var urate = 7.0;
                if (usdrate.ContainsKey(m))
                {
                    urate = usdrate[m];
                }
                else
                { urate = usdrate["CURRENT"]; }

                if (costdict.ContainsKey(pn))
                {
                    var output = qty * costdict[pn] / urate;
                    if (retdata.ContainsKey(pj))
                    {
                        retdata[pj].ShipQty += qty;
                        retdata[pj].Output += output;
                    }
                    else
                    {
                        var vm = new FsrShipData();
                        vm.MarketFamily = pj;
                        vm.ShipQty = qty;
                        vm.PN = pn;
                        vm.Cost = costdict[pn] / urate;
                        vm.Output = output;
                        retdata.Add(pj, vm);
                    }
                }
            }

            var ret = retdata.Values.ToList();
            ret.Sort(delegate (FsrShipData obj1, FsrShipData obj2)
            {
                return obj2.Output.CompareTo(obj1.Output);
            });
            foreach (var item in ret)
            {
                item.Output = Math.Round(item.Output, 3);
            }
            return ret;
        }


        public static List<Dictionary<string, double>> RetrieveOutputDataByQuarter(string producttype, Controller ctrl)
        {
            var ret = new List<Dictionary<string, double>>();

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var costdict = ItemCostData.RetrieveStandardCost();
            var usdrate = CfgUtility.GetUSDRate(ctrl);

            var startdate = searchcfg["SHIP_STARTDATE"];
            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            var pncond = "('" + string.Join("','", pnlist) + "')";

            var outputdict = new Dictionary<string, double>();
            var qtydict = new Dictionary<string, double>();

            var sql = @"select ShipQty,ShipDate,PN from FsrShipData where ShipDate >= @sdate 
                        and PN in <pncond> order by ShipDate";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", startdate);

            sql = sql.Replace("<pncond>", pncond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var qty = Convert.ToDouble(line[0]);
                var shipdate = Convert.ToDateTime(line[1]);
                var pn = Convert.ToString(line[2]).Trim();
                var q = QuarterCLA.RetrieveQuarterFromDate(shipdate);

                var m = shipdate.ToString("yyyy-MM");
                var urate = 7.0;
                if (usdrate.ContainsKey(m))
                {
                    urate = usdrate[m];
                }
                else
                { urate = usdrate["CURRENT"]; }

                if (costdict.ContainsKey(pn))
                {
                    if (outputdict.ContainsKey(q))
                    {
                        outputdict[q] += qty * costdict[pn] / urate;
                        qtydict[q] += qty;
                    }
                    else
                    {
                        outputdict.Add(q, qty * costdict[pn] / urate);
                        qtydict.Add(q, qty);
                    }
                }
            }

            ret.Add(qtydict);
            ret.Add(outputdict);
            return ret;
        }

        public static object GetShipoutTable(List<Dictionary<string, double>> outputdata, string pd, bool fordepartment)
        {
            var qtydict = outputdata[0];
            var outputdict = outputdata[1];

            var qlist = qtydict.Keys.ToList();
            qlist.Sort(delegate (string obj1, string obj2)
            {
                var i1 = QuarterCLA.RetrieveDateFromQuarter(obj1)[0];
                var i2 = QuarterCLA.RetrieveDateFromQuarter(obj2)[0];
                return i1.CompareTo(i2);
            });

            var titlelist = new List<object>();
            titlelist.Add("Shipment");
            titlelist.Add("");
            titlelist.AddRange(qlist);

            var linelist = new List<object>();
            if (fordepartment)
            {
                linelist.Add("<a href='/Shipment/ShipOutputTrend' target='_blank'>" + pd + "</a>");
            }
            else
            {
                linelist.Add(pd);
            }

            linelist.Add("<span class='YFPY'>Ship_Qty</span><br><span class='YFY'>Ship_Output_$</span>");

            //var outputiszero = false;
            var sumscraplist = new List<SCRAPSUMData>();
            foreach (var q in qlist)
            {
                linelist.Add("<span class='YFPY'>"+ String.Format("{0:n0}", Math.Round(qtydict[q],0))+"</span><br><span class='YFY'>"+ String.Format("{0:n0}", Math.Round(outputdict[q],0))+"</span>");
            }

            return new
            {
                tabletitle = titlelist,
                tablecontent = linelist
            };

        }


        public static List<Dictionary<string, double>> RetrieveOTDDataByQuarter(string producttype, Controller ctrl)
        {
            var ret = new List<Dictionary<string, double>>();

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var costdict = ItemCostData.RetrieveStandardCost();
            var usdrate = CfgUtility.GetUSDRate(ctrl);

            var otdqtydict = new Dictionary<string, double>();
            var totalqtydict = new Dictionary<string, double>();
            var otdoutputdict = new Dictionary<string, double>();
            var totaloutputdict = new Dictionary<string, double>();

            var startdate = searchcfg["SHIP_STARTDATE"];
            var pnlist = PNProuctFamilyCache.GetPNListByPF(producttype);
            var pncond = "('" + string.Join("','", pnlist) + "')";

            var sql = @"select ShipDate,Appv_5,PN,Appv_1 from FsrShipData where Appv_5 >= @sdate and Appv_5 <= @edate and PN in <pncond>  
                        and Customer1  not like '%FINISAR%' and Customer2 not like  '%FINISAR%' ";
            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", startdate);
            dict.Add("@edate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            sql = sql.Replace("<pncond>", pncond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);

            foreach (var line in dbret)
            {
                var ShipDate = Convert.ToDateTime(line[0]);
                var OPD = Convert.ToDateTime(line[1]);
                var PN = Convert.ToString(line[2]);
                var OrderQty = Convert.ToDouble(line[3]);
                var q = QuarterCLA.RetrieveQuarterFromDate(OPD);

                if (string.Compare(OPD.ToString("yyyy-MM"), "1982-05") == 0)
                { continue; }

                if (ShipDate <= OPD)
                {
                    if (totalqtydict.ContainsKey(q))
                    { totalqtydict[q] += OrderQty; }
                    else
                    { totalqtydict.Add(q, OrderQty); }

                    if (otdqtydict.ContainsKey(q))
                    { otdqtydict[q] += OrderQty; }
                    else
                    { otdqtydict.Add(q, OrderQty); }
                }
                else
                {
                    if (totalqtydict.ContainsKey(q))
                    { totalqtydict[q] += OrderQty; }
                    else
                    { totalqtydict.Add(q, OrderQty); }

                    if (!otdqtydict.ContainsKey(q))
                    { otdqtydict.Add(q, 0); }
                }


                var m = OPD.ToString("yyyy-MM");
                var urate = 7.0;
                if (usdrate.ContainsKey(m))
                {
                    urate = usdrate[m];
                }
                else
                { urate = usdrate["CURRENT"]; }


                if (costdict.ContainsKey(PN))
                {

                    if (ShipDate <= OPD)
                    {
                        if (totaloutputdict.ContainsKey(q))
                        { totaloutputdict[q] += OrderQty * costdict[PN] / urate; }
                        else
                        { totaloutputdict.Add(q, OrderQty * costdict[PN] / urate); }

                        if (otdoutputdict.ContainsKey(q))
                        { otdoutputdict[q] += OrderQty * costdict[PN] / urate; }
                        else
                        { otdoutputdict.Add(q, OrderQty * costdict[PN] / urate); }
                    }
                    else
                    {
                        if (totaloutputdict.ContainsKey(q))
                        { totaloutputdict[q] += OrderQty * costdict[PN] / urate; }
                        else
                        { totaloutputdict.Add(q, OrderQty * costdict[PN] / urate); }

                        if (!otdoutputdict.ContainsKey(q))
                        { otdoutputdict.Add(q, 0); }
                    }
                }
            }

            ret.Add(otdqtydict);
            ret.Add(totalqtydict);

            ret.Add(otdoutputdict);
            ret.Add(totaloutputdict);

            return ret;
        }

        public static object GetOTDTable(List<Dictionary<string, double>> otddata, string pd, bool fordepartment)
        {
            var otdqtydict = otddata[0];
            var totalqtydict = otddata[1];

            var otdoutputdict = otddata[2];
            var totaloutputdict = otddata[3];

            var qlist = totalqtydict.Keys.ToList();
            qlist.Sort(delegate (string obj1, string obj2)
            {
                var i1 = QuarterCLA.RetrieveDateFromQuarter(obj1)[0];
                var i2 = QuarterCLA.RetrieveDateFromQuarter(obj2)[0];
                return i1.CompareTo(i2);
            });

            var titlelist = new List<object>();
            titlelist.Add("OTD-Data");
            titlelist.Add("");
            titlelist.AddRange(qlist);

            var linelist = new List<object>();
            if (fordepartment)
            {
                linelist.Add("<a href='/Shipment/OTDData' target='_blank'>" + pd + "</a>");
            }
            else
            {
                linelist.Add(pd);
            }

            linelist.Add("<span class='YFPY'>QTY_OTD_RATE</span><br><span class='YFPY'>ORDER_QTY</span><br><span class='YFY'>$_OTD_RATE</span><br><span class='YFY'>ORDER_$</span>");

            foreach (var q in qlist)
            {
                var otdqty = otdqtydict[q];
                var totalqty = totalqtydict[q];
                var qtyrate = Math.Round(otdqty / totalqty * 100.0, 2);

                var outputrate = "";
                var totaloutput = "";
                if (otdoutputdict.ContainsKey(q))
                {
                    var output = otdoutputdict[q];
                    var toutput = totaloutputdict[q];
                    outputrate = Math.Round(output / toutput * 100.0, 2).ToString();
                    totaloutput =String.Format("{0:n0}",  Math.Round(toutput, 2));
                }

                linelist.Add("<span class='YFPY'>"+ qtyrate + "%</span><br><span class='YFPY'>"+ String.Format("{0:n0}", totalqty) + "</span><br><span class='YFY'>"+outputrate+"%</span><br><span class='YFY'>"+ totaloutput + "</span>");
            }

            return new
            {
                tabletitle = titlelist,
                tablecontent = linelist
            };

        }

        #region LOAD_SHIP_DATA

        public static void RefreshShipData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var shipsrcfile = syscfg["FINISARSHIPDATA"];
            var shipdesfile = ExternalDataCollector.DownloadShareFile(shipsrcfile, ctrl);

            var parallelpndict = PNProuctFamilyCache.GetPNDictByPF("Parallel");
            var linecardpndict = PNProuctFamilyCache.GetPNDictByPF("Linecard");
            var osapndict = PNProuctFamilyCache.GetPNDictByPF("OSA");
            var wsspndict = PNProuctFamilyCache.GetPNDictByPF("WSS");
            var componentpndict = PNProuctFamilyCache.GetPNDictByPF("passive");
            var tunabledict = PNProuctFamilyCache.GetPNDictByPF("TUNABLE");
            var sfpwiredict = PNProuctFamilyCache.GetPNDictByPF("SFP+ WIRE");

            if (!string.IsNullOrEmpty(shipdesfile))
            {
                var shipiddict = FsrShipData.RetrieveAllShipID();
                var data = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, shipdesfile);
                var shipdatalist = new List<FsrShipData>();
                var pnsndict = new Dictionary<string, string>();

                var opddict = new Dictionary<string, string>();

                foreach (var line in data)
                {
                    try
                    {
                        var shipid = Convert2Str(line[8]) + "-" + Convert2Str(line[9]) + "-" + Convert2Str(line[14]);
                        if (!shipiddict.ContainsKey(shipid))
                        {
                            var cpo = Convert2Str(line[5]).ToUpper();
                            var makebuy = Convert2Str(line[27]).ToUpper();
                            var family = Convert2Str(line[30]);
                            var orderqty = Convert.ToInt32(line[13]);
                            var shipqty = Convert.ToInt32(line[14]);
                            var pn = Convert2Str(line[10]);

                            if (!cpo.Contains("RMA") && !cpo.Contains("STOCK")
                                && makebuy.Contains("MAKE")
                                && shipqty > 0 && !string.IsNullOrEmpty(pn))
                            {
                                var cfg = Convert2Str(line[32]);

                                {
                                    if (parallelpndict.ContainsKey(pn))
                                    { cfg = "PARALLEL"; }
                                    else if (osapndict.ContainsKey(pn))
                                    { cfg = "OSA"; }
                                    else if (wsspndict.ContainsKey(pn))
                                    { cfg = "WSS"; }
                                    else if (componentpndict.ContainsKey(pn))
                                    { cfg = "COMPONENT"; }
                                    else if (linecardpndict.ContainsKey(pn))
                                    {
                                        if (!cfg.ToUpper().Contains(SHIPPRODTYPE.RED_C))
                                        { cfg = "LINECARD"; }
                                        else
                                        { cfg = "EDFA"; }
                                    }
                                    else if (tunabledict.ContainsKey(pn))
                                    { cfg = "TUNABLE"; }
                                    else if (sfpwiredict.ContainsKey(pn))
                                    { cfg = "SFP+ WIRE"; }

                                    if (string.IsNullOrEmpty(cfg))
                                    { continue; }
                                }

                                var ordereddate = Convert.ToDateTime(line[2]);
                                var customernum = Convert2Str(line[3]);
                                var customer1 = Convert2Str(line[4]);
                                var customer2 = Convert2Str(line[6]);
                                var pndesc = Convert2Str(line[12]);
                                if (pndesc.Contains("ASY,DIE,") && customer1.Contains("FINISAR"))
                                { continue; }

                                var opd = Convert.ToDateTime(line[17]);
                                var shipdate = Convert.ToDateTime(line[19]);


                                var delievenum = Convert2Str(line[24]);
                                var shipto = Convert2Str(line[33]);

                                if (!pnsndict.ContainsKey(pn))
                                {
                                    pnsndict.Add(pn, string.Empty);
                                }

                                shipdatalist.Add(new FsrShipData(shipid, shipqty, pn, pndesc, family, cfg
                                    , shipdate, customernum, customer1, customer2, ordereddate, delievenum, orderqty, opd, shipto));

                            }//end if
                        }//end if

                    }
                    catch (Exception ex) { }
                }//end foreach

                //var pn_mpn_dict = PN2MPn(pnsndict);
                //var pn_vtype_dict = RetrieveVcselPNInfo();
                //var pnratedict = new Dictionary<string, string>();
                //foreach (var pnmpnkv in pn_mpn_dict)
                //{
                //    var rate = "";
                //    foreach (var mpn in pnmpnkv.Value)
                //    {
                //        if (pn_vtype_dict.ContainsKey(mpn))
                //        {
                //            rate = pn_vtype_dict[mpn];
                //            break;
                //        }
                //    }
                //    if (!string.IsNullOrEmpty(rate))
                //    { pnratedict.Add(pnmpnkv.Key,rate); }
                //}

                var pnratedict = PNRateMap.RetrievePNRateMap(pnsndict.Keys.ToList(),ctrl);

                foreach (var item in shipdatalist)
                {
                    if (pnratedict.ContainsKey(item.PN))
                    {
                        item.VcselType = pnratedict[item.PN];
                    }
                }

                var storedid = new Dictionary<string, bool>();
                foreach (var item in shipdatalist)
                {
                    if (storedid.ContainsKey(item.ShipID))
                    { continue; }

                    storedid.Add(item.ShipID, true);
                    if (string.IsNullOrEmpty(item.VcselType))
                    {
                        item.VcselType = RetrieveRateFromDesc(item.ProdDesc.ToUpper());
                    }
                    item.StoreShipData();
                }

            }//end if
        }

        //public static void UpdateShipDataRate()
        //{
        //    var sql = "select distinct PN from FsrShipData where Configuration = 'PARALLEL' and VcselType = '' and PN <> ''";
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
        //    var pnlist = new List<string>();
        //    foreach (var line in dbret)
        //    {
        //        pnlist.Add(Convert.ToString(line[0]));
        //    }
        //    var pnratedict = PNRateMap.RetrievePNRateMap(pnlist);
        //    foreach (var kv in pnratedict)
        //    {
        //        sql = "update FsrShipData set VcselType = @VcselType where PN = @PN";
        //        var dict = new Dictionary<string, string>();
        //        dict.Add("@PN", kv.Key);
        //        dict.Add("@VcselType", kv.Value);
        //        DBUtility.ExeLocalSqlNoRes(sql, dict);
        //    }
        //}

        private static string Convert2Str(object obj)
        {
            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }
        private static Dictionary<string, bool> RetrieveAllShipID()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct ShipID from FsrShipData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0]), true); }
            return ret;
        }

        private void StoreShipData()
        {
            var sql = @"insert into FsrShipData(ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,VcselType,ShipDate,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum,SN,Wafer,Appv_1,Appv_5,Appv_2) values(
                        @ShipID,@ShipQty,@PN,@ProdDesc,@MarketFamily,@Configuration,@VcselType,@ShipDate,@CustomerNum,@Customer1,@Customer2,@OrderedDate,@DelieveNum,@SN,@Wafer,@OrderQty,@OPD,@ShipTo)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ShipID", ShipID);
            dict.Add("@ShipQty", ShipQty.ToString());
            dict.Add("@PN", PN);
            dict.Add("@ProdDesc", ProdDesc);
            dict.Add("@MarketFamily", MarketFamily);
            dict.Add("@Configuration", Configuration);
            dict.Add("@VcselType", VcselType);
            dict.Add("@ShipDate", ShipDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@CustomerNum", CustomerNum);
            dict.Add("@Customer1", Customer1);
            dict.Add("@Customer2", Customer2);
            dict.Add("@OrderedDate", OrderedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@DelieveNum", DelieveNum);
            dict.Add("@SN", SN);
            dict.Add("@Wafer", Wafer);
            dict.Add("@OrderQty", OrderQty.ToString());
            dict.Add("@OPD", OPD.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ShipTo", ShipTo);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static string RetrieveRateFromDesc(string desc)
        {
            if (desc.Contains("GBPS,"))
            {
                try
                {
                    var splitstr = desc.Split(new string[] { "GBPS," }, StringSplitOptions.RemoveEmptyEntries);
                    var xsplitstr = splitstr[0].Split(new string[] { "X" }, StringSplitOptions.RemoveEmptyEntries);
                    var rate = Convert.ToDouble(xsplitstr[xsplitstr.Length - 1]);
                    if (rate < 12.0)
                    { return VCSELRATE.r10G; }
                    if (rate >= 12.0 && rate < 20.0)
                    { return VCSELRATE.r14G; }
                    if (rate >= 20.0 && rate < 30.0)
                    { return VCSELRATE.r25G; }
                    if (rate >= 30.0)
                    { return VCSELRATE.r48G; }
                }
                catch (Exception ex) { }
            }
            return string.Empty;
        }

        private FsrShipData(string id, int qty, string pn, string pndesc, string family, string cfg
            , DateTime shipdate, string custnum, string cust1, string cust2, DateTime orddate, string delievenum, int orderqty, DateTime opd, string shipto)
        {
            ShipID = id;
            ShipQty = qty;
            PN = pn;
            ProdDesc = pndesc;
            MarketFamily = family;
            Configuration = cfg;
            ShipDate = shipdate;
            CustomerNum = custnum;
            Customer1 = cust1;
            Customer2 = cust2;
            OrderedDate = orddate;
            DelieveNum = delievenum;
            VcselType = string.Empty;
            SN = string.Empty;
            Wafer = string.Empty;
            OrderQty = orderqty;
            OPD = opd;
            OTD = "NO";
            ShipTo = shipto;
        }

        public static Dictionary<string, List<string>> PN2MPn(Dictionary<string, string> pnsndict)
        {
            var sb = new StringBuilder();
            sb.Append("('");
            foreach (var kv in pnsndict)
            {
                sb.Append(kv.Key + "','");
            }

            if (sb.ToString().Length != 2)
            {
                var pncond = sb.ToString(0, sb.Length - 2) + ")";

                var sql = @" select max(c.ContainerName),pb.productname from InsiteDB.insite.container  c (nolock) 
	                         left join InsiteDB.insite.product p on c.productId = p.productId 
	                         left join InsiteDB.insite.productbase pb on pb.productbaseid = p.productbaseid 
	                         where pb.productname in <pncond> and len(c.ContainerName) = 7 group by pb.productname";
                sql = sql.Replace("<pncond>", pncond);
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var sn = Convert.ToString(line[0]);
                    var pn = Convert.ToString(line[1]);
                    if (pnsndict.ContainsKey(pn))
                    {
                        pnsndict[pn] = sn;
                    }
                }//end foreach
            }

            var newpnsndict = CableSN2RealSN(pnsndict);
            sb = new StringBuilder();
            sb.Append("('");
            foreach (var kv in newpnsndict)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    sb.Append(kv.Value + "','");
                }
            }
            var sncond = sb.ToString(0, sb.Length - 2) + ")";

            var snmpndict = new Dictionary<string, List<string>>();
            var csql = "select distinct  [ToContainer],[FromProductName] FROM [PDMS].[dbo].[ComponentIssueSummary] where [ToContainer] in <sncond>";
            csql = csql.Replace("<sncond>", sncond);
            var cdbret = DBUtility.ExeMESReportSqlWithRes(csql);
            foreach (var line in cdbret)
            {
                var sn = Convert.ToString(line[0]);
                var mpn = Convert.ToString(line[1]);
                if (snmpndict.ContainsKey(sn))
                {
                    snmpndict[sn].Add(mpn);
                }
                else
                {
                    var templist = new List<string>();
                    templist.Add(mpn);
                    snmpndict.Add(sn, templist);
                }
            }

            var pnmpndict = new Dictionary<string, List<string>>();
            foreach (var pnsnkv in newpnsndict)
            {
                if (snmpndict.ContainsKey(pnsnkv.Value))
                {
                    pnmpndict.Add(pnsnkv.Key, snmpndict[pnsnkv.Value]);
                }
            }
            return pnmpndict;
        }

        private static Dictionary<string, string> CableSN2RealSN(Dictionary<string, string> pnsndict)
        {
            var newpnsndict = new Dictionary<string, string>();

            var sndict = new Dictionary<string, string>();

            var sb = new StringBuilder();
            sb.Append("('");
            foreach (var kv in pnsndict)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    sb.Append(kv.Value + "','");
                }
            }
            if (sb.ToString().Length == 2)
            { return newpnsndict; }

            var sncond = sb.ToString(0, sb.Length - 2) + ")";

            var sql = @"SELECT  max([FromContainer]),ToContainer
                        FROM [PDMS].[dbo].[ComponentIssueSummary] where ToContainer in <sncond> and len([FromContainer]) = 7 group by ToContainer";
            sql = sql.Replace("<sncond>", sncond);
            var dbret = DBUtility.ExeMESReportSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tosn = Convert.ToString(line[1]);
                var fromsn = Convert.ToString(line[0]);
                if (!sndict.ContainsKey(tosn))
                {
                    sndict.Add(tosn, fromsn);
                }
            }

            foreach (var kv in pnsndict)
            {
                if (sndict.ContainsKey(kv.Value))
                {
                    newpnsndict.Add(kv.Key, sndict[kv.Value]);
                }
                else
                {
                    newpnsndict.Add(kv.Key, kv.Value);
                }
            }
            return newpnsndict;
        }

        public static Dictionary<string, string> RetrieveVcselPNInfo()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select PN,Rate from VcselPNData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var PN = Convert.ToString(line[0]);
                var Rate = Convert.ToString(line[1]);
                if (!ret.ContainsKey(PN))
                {
                    ret.Add(PN, Rate);
                }
            }
            return ret;
        }

        #endregion


        public string ShipID { set; get; }
        public double ShipQty { set; get; }
        public string PN { set; get; }
        public string ProdDesc { set; get; }
        public string MarketFamily { set; get; }
        public string Configuration { set; get; }
        public string VcselType { set; get; }
        public DateTime ShipDate { set; get; }
        public string ShipDateStr { get { return ShipDate.ToString("yyyy-MM-dd"); } }
        public string CustomerNum { set; get; }
        public string Customer1 { set; get; }
        public string Customer2 { set; get; }
        public DateTime OrderedDate { set; get; }
        public string DelieveNum { set; get; }
        public string SN { set; get; }
        public string Wafer { set; get; }
        public double OrderQty { set; get; }
        public DateTime OPD { set; get; }
        public string OPDStr { get { return OPD.ToString("yyyy-MM-dd"); } }
        public string OTD { set; get; }
        public string ShipTo { set; get; }
        public double Cost { set; get; }
        public double Output { set; get; }
    }
}