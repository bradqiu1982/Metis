using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class PNProuctFamilyCache
    {

        public static void LoadData()
        {
            var pndict = new Dictionary<string, bool>();
            var datalist = new List<PNProuctFamilyCache>();

            var pflist = new string[] {"LineCard","OSA","Parallel","Passive","WSS", "10G Tunable BIDI"
                , "COHERENT", "T-XFP"}.ToList();
            foreach (var pf in pflist)
            {
                var templist = RetrieveAllPN(pf);
                foreach (var vm in templist)
                {
                    if (!pndict.ContainsKey(vm.PN))
                    {
                        datalist.Add(vm);
                        pndict.Add(vm.PN, true);
                    }//end if
                }//end foreach
            }//end foreach

            if (datalist.Count > 0)
            {
                CleanData();
                foreach (var item in datalist)
                {
                    item.StoreData();
                }
            }

        }

        private static void CleanData()
        {
            var sql = "delete from PNProuctFamilyCache";
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreData()
        {
            var sql = "insert into PNProuctFamilyCache(PN,ProductFamily) values(@PN,@ProductFamily)";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            dict.Add("@ProductFamily", ProductFamily);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public PNProuctFamilyCache()
        {
            PN = string.Empty;
            ProductFamily = string.Empty;
        }

        public static Dictionary<string, string> PNPFDict()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select PN,ProductFamily FROM PNProuctFamilyCache";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            foreach (var line in dbret)
            {
                var PN = Convert.ToString(line[0]);
                var ProductFamily = Convert.ToString(line[1]);
                if (!ret.ContainsKey(PN))
                { ret.Add(PN, ProductFamily); }
            }
            return ret;
        }

        public static Dictionary<string, List<string>> PFPNDict()
        {
            var ret = new Dictionary<string, List<string>>();
            var sql = "select PN,ProductFamily FROM PNProuctFamilyCache";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            foreach (var line in dbret)
            {
                var PN = Convert.ToString(line[0]);
                var ProductFamily = Convert.ToString(line[1]);
                if (ret.ContainsKey(ProductFamily))
                {
                    ret[ProductFamily].Add(PN);
                }
                else
                {
                    var templist = new List<string>();
                    templist.Add(PN);
                    ret.Add(ProductFamily, templist);
                }
            }
            return ret;
        }

        private static List<PNProuctFamilyCache> RetrieveAllPN(string pg)
        {
            var datalist = new List<PNProuctFamilyCache>();
            var sql = @"select distinct pb.ProductName,pf.ProductFamilyName from [InsiteDB].[insite].[ProductFamily] pf
                          left join [InsiteDB].[insite].[Product] pd on pd.ProductFamilyId = pf.ProductFamilyId
                          left join [InsiteDB].[insite].[ProductBase] pb on pb.ProductBaseId = pd.ProductBaseId
                          where  pf.ProductFamilyName like '<productgroup>%'  and pd.[Description] is not null  and pb.ProductName is not null 
                          and ( pd.[Description] not like 'LEN%' and pd.[Description] not like 'Shell%')";
            sql = sql.Replace("<productgroup>", pg);

            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                try
                {
                    var vm = new PNProuctFamilyCache();
                    vm.PN = Convert.ToString(line[0]);
                    vm.ProductFamily = Convert.ToString(line[1]);
                    datalist.Add(vm);
                }
                catch (Exception ex) { }
            }
            return datalist;
        }
        
        public static List<string> GetPNListByPF(string pf)
        {
            var cond = "";
            if (string.Compare(pf, "PARALLEL", true) == 0)
            {
                cond = " ProductFamily like 'PARALLEL%' and ProductFamily not like 'PARALLEL.SFPWIRE%' ";
            }
            else if (string.Compare(pf, "SFP+ WIRE", true) == 0
                || string.Compare(pf, "SFP+WIRE", true) == 0)
            {
                cond = " ProductFamily like 'PARALLEL.SFPWIRE%' ";
            }
            else if (string.Compare(pf, "10G Tunable", true) == 0
                || string.Compare(pf, "TUNABLE", true) == 0
                || string.Compare(pf, "Telecom TRX", true) == 0
                || string.Compare(pf, "OPTIUM", true) == 0)
            {
                cond = " ProductFamily like '10G Tunable BIDI%' or ProductFamily like 'T-XFP%' or ProductFamily like 'COHERENT%' ";
            }
            else if (string.Compare(pf, "LINECARD", true) == 0
                || string.Compare(pf, "LNCD", true) == 0)
            {
                cond = " ProductFamily like 'Linecard%' ";
            }
            else if (string.Compare(pf, "EDFA", true) == 0
                || string.Compare(pf, "RED-C", true) == 0)
            {
                cond = " ProductFamily like 'Linecard.EDFA%' ";
            }
            else
            {
                cond = " ProductFamily like '" + pf + "%' ";
            }

            var ret = new List<string>();
            var sql = "select PN FROM PNProuctFamilyCache where (<ProductFamilyCond>)";
            sql = sql.Replace("<ProductFamilyCond>", cond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<string> GetPNListByPF4JO(string pf)
        {
            var cond = "";
            if (string.Compare(pf, "PARALLEL", true) == 0)
            {
                cond = " ProductFamily like 'PARALLEL%' and ProductFamily not like 'PARALLEL.SFPWIRE%' ";
            }
            else if (string.Compare(pf, "SFP+ WIRE", true) == 0
                || string.Compare(pf, "SFP+WIRE", true) == 0)
            {
                cond = " ProductFamily like 'PARALLEL.SFPWIRE%' ";
            }
            else if (string.Compare(pf, "10G Tunable", true) == 0
                || string.Compare(pf, "TUNABLE", true) == 0
                || string.Compare(pf, "Telecom TRX", true) == 0
                || string.Compare(pf, "OPTIUM", true) == 0)
            {
                cond = " ProductFamily like '10G Tunable BIDI%' or ProductFamily like 'T-XFP%' or ProductFamily like 'COHERENT%' ";
            }
            else if (string.Compare(pf, "LINECARD", true) == 0
                || string.Compare(pf, "LNCD", true) == 0)
            {
                cond = " ProductFamily like 'Linecard%' ";
            }
            else if (string.Compare(pf, "EDFA", true) == 0
                || string.Compare(pf, "RED-C", true) == 0)
            {
                cond = " ProductFamily like 'Linecard.EDFA%' ";
            }
            else
            {
                cond = " ProductFamily like '%" + pf + "%' ";
            }

            var ret = new List<string>();
            var sql = "select PN FROM PNProuctFamilyCache where (<ProductFamilyCond>)";
            sql = sql.Replace("<ProductFamilyCond>", cond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static Dictionary<string, bool> GetPNDictByPF(string pf)
        {
            var ret = new Dictionary<string, bool>();
            var pnlist = GetPNListByPF(pf);
            foreach (var p in pnlist)
            {
                if (!ret.ContainsKey(p))
                { ret.Add(p, true); }
            }
            return ret;
        }

        public string PN { set; get; }
        public string ProductFamily { set; get; }
    }
}