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

            var sql = "select distinct PN,ProductFamily FROM ModuleTestData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var datalist = new List<PNProuctFamilyCache>();
            foreach (var line in dbret)
            {
                var vm = new PNProuctFamilyCache();
                vm.PN = Convert.ToString(line[0]);
                vm.ProductFamily = Convert.ToString(line[1]);
                datalist.Add(vm);

                if (!pndict.ContainsKey(vm.PN))
                { pndict.Add(vm.PN, true); }
            }

            var pflist = new string[] {"LineCard","OSA","Parallel","Passive","WSS" }.ToList();
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
            var datalist = new List<PNProuctFamilyCache>();
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
            var datalist = new List<PNProuctFamilyCache>();
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

        public string PN { set; get; }
        public string ProductFamily { set; get; }
    }
}