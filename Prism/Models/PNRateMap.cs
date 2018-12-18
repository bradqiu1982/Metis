using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Prism.Models
{
    public class PNRateMap
    {
        public static Dictionary<string,string> RetrievePNRateMap(List<string> pnlist)
        {
            if (pnlist.Count == 0)
            { return new Dictionary<string, string>(); }

            var pnsndict = new Dictionary<string, string>();
            foreach (var p in pnlist)
            {
                if (!pnsndict.ContainsKey(p))
                {  pnsndict.Add(p, ""); }
            }
            var retobj = PN2MPn(pnsndict);
            var pn_mpn_dict = (Dictionary<string, List<string>>)retobj[0];
            var mpnratedict = (Dictionary<string, string>)retobj[1];

            var pndesdict = pnpndesmap(pnlist);

            var pn_vtype_dict = RetrieveVcselPNInfo();
            foreach (var mpnkv in mpnratedict)
            {
                if (!pn_vtype_dict.ContainsKey(mpnkv.Key))
                {
                    UpdateVcselPNInfo(mpnkv.Key, mpnkv.Value);
                    pn_vtype_dict.Add(mpnkv.Key, mpnkv.Value);
                }
            }

            var pnratedict = new Dictionary<string, string>();
            foreach (var pnmpnkv in pn_mpn_dict)
            {
                var rate = "";
                foreach (var mpn in pnmpnkv.Value)
                {
                    if (pn_vtype_dict.ContainsKey(mpn))
                    {
                        rate = pn_vtype_dict[mpn];
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(rate))
                { pnratedict.Add(pnmpnkv.Key, rate); }
                else
                {
                    if (pndesdict.ContainsKey(pnmpnkv.Key))
                    {
                        rate = RetrieveRateFromDesc(pndesdict[pnmpnkv.Key]);
                        if (!string.IsNullOrEmpty(rate))
                        { pnratedict.Add(pnmpnkv.Key, rate); }
                    }
                }
            }

            return pnratedict;
        }

        private static Dictionary<string, string> pnpndesmap(List<string> pnlist)
        {
            var pndesdict = new Dictionary<string, string>();
            var pncond = "('" + string.Join("','", pnlist) + "')";
            var sql = @" select max(p.description),pb.productname from InsiteDB.insite.product p (nolock) 
	                         left join InsiteDB.insite.productbase pb on pb.productbaseid = p.productbaseid 
	                         where pb.productname in <pncond> group by pb.productname";
            sql = sql.Replace("<pncond>", pncond);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pndes = Convert.ToString(line[0]);
                var pn = Convert.ToString(line[1]);
                if (!pndesdict.ContainsKey(pn))
                { pndesdict.Add(pn, pndes); }
            }
            return pndesdict;
        }

        private static List<object> PN2MPn(Dictionary<string, string> pnsndict)
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

            var mpnratedict = new Dictionary<string, string>();

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
            var csql = "select distinct  [ToContainer],[FromProductName],FromPNDescription FROM [PDMS].[dbo].[ComponentIssueSummary] where [ToContainer] in <sncond>";
            csql = csql.Replace("<sncond>", sncond);
            var cdbret = DBUtility.ExeMESReportSqlWithRes(csql);
            foreach (var line in cdbret)
            {
                var sn = Convert.ToString(line[0]);
                var mpn = Convert.ToString(line[1]);
                var mpndesc = Convert.ToString(line[2]);

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

                if (mpndesc.Contains(",VCSEL,") && mpndesc.Contains("850"))
                {
                    if (!mpnratedict.ContainsKey(mpn))
                    {
                        var rate = "";
                        if (mpndesc.Contains(",25G") || mpndesc.Contains(",28G"))
                        {
                            rate = "25G";

                        }
                        else if(mpndesc.Contains(",10G"))
                        {
                            rate = "10G";
                        }
                        else if (mpndesc.Contains(",14G"))
                        {
                            rate = "14G";
                        }

                        if (!string.IsNullOrEmpty(rate))
                        { mpnratedict.Add(mpn, rate); }
                    }
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

            var retobj = new List<object>();
            retobj.Add(pnmpndict);
            retobj.Add(mpnratedict);
            return retobj;
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

        private static Dictionary<string, string> RetrieveVcselPNInfo()
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

        private static void UpdateVcselPNInfo(string mpn,string rate)
        {
            var ret = new Dictionary<string, string>();
            var sql = "insert into VcselPNData(PN,Rate) values(@PN,@Rate)";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", mpn);
            dict.Add("@Rate", rate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null,dict);
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


    }
}