using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class VcselRMAData
    {
        public static void LoadVCSELRMA(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var vcselrmafile = ExternalDataCollector.DownloadShareFile(syscfg["VCSELRMASHARE"], ctrl);
            if (string.IsNullOrEmpty(vcselrmafile))
            { return; }

            //load RMA data
            try
            {
                var existrmasn = VcselRMAData.GetAllVcselRMASN();
                var idx = 0;
                var data =  ExcelReader.RetrieveDataFromExcel(vcselrmafile, "Master list");
                var vcselrmalist = new List<VcselRMAData>();

                foreach (var line in data)
                {
                    if (idx == 0)
                    {
                        idx = idx + 1;
                        continue;
                    }

                    if (string.IsNullOrEmpty(line[10].Trim()))
                    { continue; }

                    var sn = line[10].Trim().ToUpper().Split(new string[] { ";","/"," "},StringSplitOptions.RemoveEmptyEntries)[0];
                    if (!existrmasn.ContainsKey(sn))
                    {
                        existrmasn.Add(sn, true);

                        var tempvm = new VcselRMAData();
                        tempvm.SN = sn;
                        tempvm.PN = line[8];
                        tempvm.PNDesc = line[9];
                    
                        tempvm.RMANum = line[0];
                        tempvm.Customer = line[1];
                        tempvm.ProductType = line[2];
                        tempvm.ShipDate = line[3];
                        tempvm.RMAOpenDate = line[4];

                        vcselrmalist.Add(tempvm);
                    }//not exist
                }//end foreach

                if (vcselrmalist.Count > 0)
                {
                    var snlist = new List<string>();
                    foreach (var item in vcselrmalist)
                    {
                        snlist.Add(item.SN);
                    }

                    var sncond = "('" + string.Join("','", snlist) + "')";
                    var snwaferdict = WaferData.GetWaferInfoBySN(sncond);

                    foreach (var item in vcselrmalist)
                    {
                        if (snwaferdict.ContainsKey(item.SN))
                        {
                            item.VcselType = snwaferdict[item.SN].Rate;
                            item.VcselPN = snwaferdict[item.SN].WaferPN;
                            item.VcselArray = snwaferdict[item.SN].WaferArray;
                            item.VcselTech = snwaferdict[item.SN].WaferTech;
                            item.Wafer = snwaferdict[item.SN].WaferNum;
                            item.BuildDate = DateTime.Parse(snwaferdict[item.SN].SNDate);
                            if (string.IsNullOrEmpty(item.ShipDate) || !CheckDate(item.ShipDate))
                            {
                                item.ShipDate = item.BuildDate.AddMonths(2).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            if (item.BuildDate > DateTime.Parse(item.ShipDate))
                            {
                                item.BuildDate = DateTime.Parse(item.ShipDate).AddMonths(-2);
                            }
                            item.StoreVcselRMA();
                        }
                    }//end foreach
                }
            }
            catch (Exception ex) { }

            //load milestone
            EngineeringMileStone.LoadMileStone(ctrl, vcselrmafile);
        }

        private static bool CheckDate(string d)
        {
            try
            {
                DateTime.Parse(d);
            }catch (Exception ex) { return false; }
            return true;
        }

        public static Dictionary<string, int> RetrieveRMACntByMonth(string sdate, string edate, string rate)
        {
            var ret = new Dictionary<string, int>();
            var sql = "select ShipDate from VcselRMAData where ShipDate >= @sdate and ShipDate <=@edate ";

            if (string.Compare(rate, VCSELRATE.r14G, true) == 0)
            { sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')"; }
            else if (string.Compare(rate, VCSELRATE.r25G, true) == 0)
            { sql = sql + " and VcselType = '" + rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null, dict);
            foreach (var line in dbret)
            {
                var m = Convert.ToDateTime(line[0]).ToString("yyyy-MM");
                if (ret.ContainsKey(m))
                {
                    ret[m] = ret[m] + 1;
                }
                else
                {
                    ret.Add(m, 1);
                }
            }
            return ret;
        }

        public static List<VcselRMAData> RetrieveWaferRawDataByMonth(string sdate, string edate, string rate)
        {
            var ret = new List<VcselRMAData>();
            var sql = "select SN,BuildDate,Wafer,PN,PNDesc,VcselPN,VcselType,ProductType,ShipDate,RMAOpenDate,RMANum,Customer from VcselRMAData where ShipDate >= @sdate and ShipDate <= @edate ";
            if (rate.Contains(VCSELRATE.r14G))
            { sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')"; }
            else
            { sql = sql + " and VcselType = '" + rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new VcselRMAData();
                tempvm.SN = Convert.ToString(line[0]);
                tempvm.BuildDate = Convert.ToDateTime(line[1]);
                tempvm.Wafer = Convert.ToString(line[2]);

                tempvm.PN = Convert.ToString(line[3]);
                tempvm.PNDesc = Convert.ToString(line[4]);
                tempvm.VcselPN = Convert.ToString(line[5]);
                tempvm.VcselType = Convert.ToString(line[6]);

                tempvm.ProductType = Convert.ToString(line[7]);
                tempvm.ShipDate = Convert.ToString(line[8]);
                tempvm.RMAOpenDate = Convert.ToString(line[9]);
                tempvm.RMANum = Convert.ToString(line[10]);
                tempvm.Customer = Convert.ToString(line[11]);

                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            {
                var sncond = "('";
                foreach (var item in ret)
                {
                    if (!string.IsNullOrEmpty(item.SN))
                    {
                        sncond = sncond + item.SN + "','";
                    }
                }
                sncond = sncond.Substring(0, sncond.Length - 2) + ")";
                var snkeydict = RetrieveIssueBySNs(sncond);
                foreach (var item in ret)
                {
                    if (snkeydict.ContainsKey(item.SN))
                    {
                        item.IssueKey = snkeydict[item.SN];
                    }
                }
            }

            return ret;
        }

        public static List<VcselRMAData> RetrieveWaferRawData(string wafer)
        {
            var ret = new List<VcselRMAData>();
            var sql = "select SN,BuildDate,Wafer,PN,PNDesc,VcselPN,VcselType,ProductType,ShipDate,RMAOpenDate,RMANum,Customer from VcselRMAData where Wafer = @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new VcselRMAData();
                tempvm.SN = Convert.ToString(line[0]);
                tempvm.BuildDate = Convert.ToDateTime(line[1]);
                tempvm.Wafer = Convert.ToString(line[2]);

                tempvm.PN = Convert.ToString(line[3]);
                tempvm.PNDesc = Convert.ToString(line[4]);
                tempvm.VcselPN = Convert.ToString(line[5]);
                tempvm.VcselType = Convert.ToString(line[6]);

                tempvm.ProductType = Convert.ToString(line[7]);
                tempvm.ShipDate = Convert.ToString(line[8]);
                tempvm.RMAOpenDate = Convert.ToString(line[9]);
                tempvm.RMANum = Convert.ToString(line[10]);
                tempvm.Customer = Convert.ToString(line[11]);

                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            {
                var sncond = "('";
                foreach (var item in ret)
                {
                    if (!string.IsNullOrEmpty(item.SN))
                    {
                        sncond = sncond + item.SN + "','";
                    }
                }
                sncond = sncond.Substring(0, sncond.Length - 2) + ")";
                var snkeydict = RetrieveIssueBySNs(sncond);
                foreach (var item in ret)
                {
                    if (snkeydict.ContainsKey(item.SN))
                    {
                        item.IssueKey = snkeydict[item.SN];
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, string> RetrieveIssueBySNs(string sncond)
        {
            var ret = new Dictionary<string, string>();
            var sql = "select ModuleSN,IssueKey from Issue where ModuleSN in <sncond> and IssueType='<IssueType>'";
            sql = sql.Replace("<sncond>", sncond).Replace("<IssueType>", "RMA");
            var dbret = DBUtility.ExeNPISqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                var key = Convert.ToString(line[1]);
                if (!ret.ContainsKey(sn))
                {
                    ret.Add(sn, key);
                }
            }
            return ret;
        }


        public static List<VcselRMAData> RetrieveDistinctWaferListASC(string rate)
        {
            var ret = new List<VcselRMAData>();
            var wdict = new Dictionary<string, bool>();
            var sql = "select Wafer,BuildDate,VcselType from VcselRMAData ";
            if (!string.IsNullOrEmpty(rate.Trim()))
            {
                sql = sql + "  where VcselType = '<VcselType>'  ";
                sql = sql.Replace("<VcselType>", rate);
            }
            sql = sql + " order by BuildDate ASC";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var w = Convert.ToString(line[0]);
                if (!wdict.ContainsKey(w))
                {
                    wdict.Add(w, true);
                    var tempvm = new VcselRMAData();
                    tempvm.Wafer = w;
                    tempvm.BuildDate = Convert.ToDateTime(line[1]);
                    tempvm.VcselType = Convert.ToString(line[2]);
                    ret.Add(tempvm);
                }
            }
            return ret;
        }

        public static List<VcselRMAData> RetrieveLatestWafer(string rate)
        {
            var ret = new List<VcselRMAData>();
            var wdict = new Dictionary<string, bool>();
            var sql = "select top 1 Wafer from VcselRMAData ";
            if (!string.IsNullOrEmpty(rate.Trim()))
            {
                sql = sql + "  where VcselType = '<VcselType>'  ";
                sql = sql.Replace("<VcselType>", rate);
            }
            sql = sql + " order by RMAOpenDate DESC";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new VcselRMAData();
                tempvm.Wafer = Convert.ToString(line[0]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static Dictionary<string, int> RetrieveWaferCountDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = "select count(*) as cnt,Wafer from VcselRMAData group by Wafer";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var wafer = Convert.ToString(line[1]);
                var cnt = Convert.ToInt32(line[0]);
                if (!ret.ContainsKey(wafer))
                {
                    ret.Add(wafer, cnt);
                }
            }
            return ret;
        }

        public static Dictionary<string, int> RetrieveVcselTechDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = " select count(*) as cnt,VcselTech,VcselType from [BSSupport].[dbo].[VcselRMAData] group by VcselTech,VcselType";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                
                var tech = Convert.ToString(line[1]);
                var rate = Convert.ToString(line[2]);
                if (string.Compare(tech, "MESA", true) == 0 &&
                    (rate.Contains("10G") || rate.Contains("14G")))
                { continue; }
                var cnt = Convert.ToInt32(line[0]);
                if (!ret.ContainsKey(tech))
                {
                    ret.Add(tech, cnt);
                }
                else
                {
                    ret[tech] += cnt;
                }
            }
            return ret;
        }

        public static Dictionary<string, int> RetrieveVcselArrayDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = "select count(*) as cnt,VcselArray,VcselType from [BSSupport].[dbo].[VcselRMAData] group by VcselArray,VcselType";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var rate = Convert.ToString(line[2]);
                if (!rate.Contains("25G"))
                { continue; }

                var ary = Convert.ToString(line[1]);
                var cnt = Convert.ToInt32(line[0]);
                if (!ret.ContainsKey(ary))
                {
                    ret.Add(ary, cnt);
                }
            }
            return ret;
        }

        public static List<VcselRMAData> RetrievAllDataASC()
        {
            var ret = new List<VcselRMAData>();
            var sql = "select Wafer,BuildDate,VcselType,ShipDate,RMAOpenDate,SN from VcselRMAData order by BuildDate ASC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new VcselRMAData();
                tempvm.Wafer = Convert.ToString(line[0]);
                tempvm.BuildDate = Convert.ToDateTime(line[1]);
                tempvm.VcselType = Convert.ToString(line[2]);
                tempvm.ShipDate = Convert.ToString(line[3]);
                tempvm.RMAOpenDate = Convert.ToString(line[4]);
                tempvm.SN = Convert.ToString(line[5]);
                ret.Add(tempvm);
            }
            return ret;
        }

        //return dict<month,dict<rate,count>>
        public static Dictionary<string, Dictionary<string, int>> RetrieveRMACountByBuildMonth(Dictionary<string, string> vtypedict)
        {
            var ret = new Dictionary<string, Dictionary<string, int>>();
            var vlist = RetrievAllDataASC();
            foreach (var v in vlist)
            {
                var month = v.BuildDate.ToString("yyyy-MM");
                if (!vtypedict.ContainsKey(v.VcselType))
                { vtypedict.Add(v.VcselType, ""); }

                if (ret.ContainsKey(month))
                {
                    var ratedict = ret[month];
                    if (ratedict.ContainsKey(v.VcselType))
                    {
                        ratedict[v.VcselType] = ratedict[v.VcselType] + 1;
                    }
                    else
                    {
                        ratedict.Add(v.VcselType, 1);
                    }
                }
                else
                {
                    var ratedict = new Dictionary<string, int>();
                    ratedict.Add(v.VcselType, 1);
                    ret.Add(month, ratedict);
                }
            }

            //add type to all month 
            foreach (var r in ret)
            {
                foreach (var vt in vtypedict)
                {
                    if (!r.Value.ContainsKey(vt.Key))
                    {
                        r.Value.Add(vt.Key, 0);
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, bool> GetAllVcselRMASN()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct SN from VcselRMAData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]), true);
            }
            return ret;
        }

        public static List<string> RetrieveRateList(string startdate,string enddate)
        {
            var ret = new List<string>();
            var sql = "select distinct VcselType from VcselRMAData where BuildDate > '<startdate>' and  BuildDate < '<enddate>'";
            sql = sql.Replace("<startdate>", startdate).Replace("<enddate>", enddate);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            ret.Sort();
            return ret;
        }

        public void StoreVcselRMA()
        {
            var sql = @"insert into VcselRMAData(SN,BuildDate,Wafer,PN,PNDesc,VcselPN,VcselType,VcselArray,VcselTech,ProductType,ShipDate,RMAOpenDate,RMANum,Customer)  
                        values(@SN,@BuildDate,@Wafer,@PN,@PNDesc,@VcselPN,@VcselType,@VcselArray,@VcselTech,@ProductType,@ShipDate,@RMAOpenDate,@RMANum,@Customer)";
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", SN); dict.Add("@BuildDate", BuildDate.ToString("yyyy-MM-dd HH:mm:ss")); dict.Add("@Wafer", Wafer);
            dict.Add("@PN", PN); dict.Add("@PNDesc", PNDesc); dict.Add("@VcselPN", VcselPN);
            dict.Add("@VcselType", VcselType); dict.Add("@VcselArray", VcselArray); dict.Add("@VcselTech", VcselTech);
            dict.Add("@ProductType", ProductType); dict.Add("@ShipDate", ShipDate);
            dict.Add("@RMAOpenDate", RMAOpenDate); dict.Add("@RMANum", RMANum); dict.Add("@Customer", Customer);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public VcselRMAData()
        {
            SN = "";
            BuildDate = DateTime.Parse("1982-05-06 10:00:00");
            Wafer = "";

            PN = "";
            PNDesc = "";
            VcselPN = "";
            VcselType = "";
            VcselArray = "";
            VcselTech = "";

            ProductType = "";
            ShipDate = "";
            RMAOpenDate = "";
            RMANum = "";
            Customer = "";
            IssueKey = "";
        }

        public string SN { set; get; }
        public DateTime BuildDate { set; get; }
        public string Wafer { set; get; }


        public string PN { set; get; }
        public string PNDesc { set; get; }
        public string VcselPN { set; get; }
        public string VcselType { set; get; }
        public string VcselArray { set; get; }
        public string VcselTech { set; get; }

        public string ProductType { set; get; }
        public string ShipDate { set; get; }
        public string RMAOpenDate { set; get; }
        public string RMANum { set; get; }
        public string Customer { set; get; }
        public string IssueKey { set; get; }

    }


    public class VcselRMADPPM
    {
        public VcselRMADPPM()
        {
            Wafer = "";
            DPPM = 0.0;
            ShippedQty = 0.0;
        }

        public string Wafer { set; get; }
        public double DPPM { set; get; }
        public double ShippedQty { set; get; }
    }

    public class VcselRMASum
    {
        public static List<string> FMColor()
        {
            return new List<string> {
                "#AF0A00","#009f30","#0053a2","#7030a0",
                "#105D9C","#23735D","#A55417","#821A08","#7030A0",
                "#0C779D","#34AC8B","#D85C00","#CC044D","#B925A7",
                "#4FADF3","#12CC92","#FA9604","#ED6161","#EF46FC",
                "#8CC9F7","#BEEBDF","#FDEEC3","#F6B0B0","#EC88F4"
            };
        }

        public static List<VcselRMADPPM> RetrieveVcselDPPM(string rate,string startdate,string enddate)
        {
            var ret = new List<VcselRMADPPM>();

            var wlist = WaferData.RetrieveDistinctWaferListASC(rate, startdate, enddate);
            var rmacntdict = VcselRMAData.RetrieveWaferCountDict();
            var wafercntdict = WaferData.RetriveWaferCountDict();
            foreach (var wf in wlist)
            {
                if (rmacntdict.ContainsKey(wf) && wafercntdict.ContainsKey(wf))
                {
                    var tempvm = new VcselRMADPPM();
                    tempvm.Wafer = wf;
                    tempvm.ShippedQty = wafercntdict[wf];
                    tempvm.DPPM = Math.Round((double)rmacntdict[wf] / (double)wafercntdict[wf] * 1000000, 0);
                    ret.Add(tempvm);
                }
                else if (wafercntdict.ContainsKey(wf))
                {
                    var tempvm = new VcselRMADPPM();
                    tempvm.Wafer = wf;
                    tempvm.ShippedQty = wafercntdict[wf];
                    tempvm.DPPM = 0.0;
                    ret.Add(tempvm);
                }
            }//end foreach
            return ret;
        }

        public static List<object> VcselRMAMileStoneDataByBuildDate()
        {
            var ret = new List<object>();
            var vtypedict = new Dictionary<string, string>();
            var rmacountdata = VcselRMAData.RetrieveRMACountByBuildMonth(vtypedict);
            ret.Add(rmacountdata);

            var milelist = EngineeringMileStone.RetrieveVcselMileStone();
            ret.Add(milelist);

            var colorlist = VcselRMASum.FMColor();
            var vtypekeylist = vtypedict.Keys.ToList();
            var cidx = 0;
            foreach (var vk in vtypekeylist)
            {
                vtypedict[vk] = colorlist[cidx % colorlist.Count];
                cidx = cidx + 1;
            }
            ret.Add(vtypedict);

            return ret;
        }


    }


}