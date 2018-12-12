using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class SEARCHFIELD
    {
        public static string ALLFIELDS = "ALL FIELDS";
        public static string YIELD = "YIELD";
        public static string MACHINERATE = "MACHINE RATE";
        public static string SHIPQTY = "SHIP QTY";
        public static string SHIPOUTPUT = "SHIP OUTPUT";
        public static string OTDQTY = "OTD QTY";
        public static string OTDOUTPUT = "OTD OUTPUT";
        public static string SCRAP = "SCRAP";
        public static string CAPACITY = "CAPACITY";
        public static string INVENTORY = "INVENTORY";
    }

    public class SearchVM
    {
        public static List<string> SearchFields()
        {
            var ret = new List<string>();
            ret.Add(SEARCHFIELD.YIELD);
            ret.Add(SEARCHFIELD.MACHINERATE);
            ret.Add(SEARCHFIELD.SHIPQTY);
            ret.Add(SEARCHFIELD.SHIPOUTPUT);
            ret.Add(SEARCHFIELD.OTDQTY);
            ret.Add(SEARCHFIELD.OTDOUTPUT);
            ret.Add(SEARCHFIELD.SCRAP);
            ret.Add(SEARCHFIELD.CAPACITY);
            ret.Add(SEARCHFIELD.INVENTORY);
            return ret;
        }

        private static List<string> AllFieldSearchRange(Controller ctrl)
        {
            var ret = new List<string>();
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var shipfamilys = searchcfg["ALLFIELDFAMILY"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ret.AddRange(shipfamilys);
            var pfdict = PNProuctFamilyCache.PFPNDict();
            var pflist = pfdict.Keys.ToList();
            pflist.Sort();
            ret.AddRange(pflist);
            return ret;
        }

        private static List<string> YieldSearchRange(Controller ctrl)
        {
            var ret = new List<string>();
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var yieldfamilys = searchcfg["YIELDFAMILY"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ret.AddRange(yieldfamilys);

            var pdflist = YieldPreData.RetrieveYieldPDFamilyList();
            ret.AddRange(pdflist);

            return ret;
        }

        private static List<string> ShipSearchRange(Controller ctrl)
        {
            var ret = new List<string>();
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var shipfamilys = searchcfg["SHIPFAMILY"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ret.AddRange(shipfamilys);
            var pfdict = PNProuctFamilyCache.PFPNDict();
            var pflist = pfdict.Keys.ToList();
            pflist.Sort();
            ret.AddRange(pflist);
            return ret;
        }

        private static List<string> ScrapSearchRange(Controller ctrl)
        {
            var ret = new List<string>();
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var scrapfamilys = searchcfg["SCRAPFAMILY"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ret.AddRange(scrapfamilys);
            var pflist = ScrapData_Base.GetAllProductList();
            pflist.Sort();
            ret.AddRange(pflist);
            return ret;
        }

        private static List<string> CapacitySearchRange(Controller ctrl)
        {
            var ret = new List<string>();
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var capacityfamilys = searchcfg["CAPACITYFAMILY"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ret.AddRange(capacityfamilys);
            var prodlist = CapacityRawData.GetAllProductList();
            ret.AddRange(prodlist);
            return ret;
        }

        private static List<string> InventorySearchRange(Controller ctrl)
        {
            var ret = new List<string>();
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            var inventoryfamilys = searchcfg["INVENTORYFAMILY"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ret.AddRange(inventoryfamilys);
            var prodlist = InventoryData.RetrieveAllPF();
            ret.AddRange(prodlist);
            return ret;
        }


        public static List<string> SearchRange(string searchfield,Controller ctrl)
        {
            if (string.Compare(searchfield, SEARCHFIELD.ALLFIELDS) == 0)
            {
                return AllFieldSearchRange(ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.YIELD) == 0
                || string.Compare(searchfield, SEARCHFIELD.MACHINERATE) == 0)
            {
                return YieldSearchRange(ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.SHIPQTY) == 0
                || string.Compare(searchfield, SEARCHFIELD.SHIPOUTPUT) == 0
                || string.Compare(searchfield, SEARCHFIELD.OTDQTY) == 0
                || string.Compare(searchfield, SEARCHFIELD.OTDOUTPUT) == 0)
            {
                return ShipSearchRange(ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.SCRAP) == 0)
            {
                return ScrapSearchRange(ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.CAPACITY) == 0)
            {
                return CapacitySearchRange(ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.INVENTORY) == 0)
            {
                return InventorySearchRange(ctrl);
            }

            return new List<string>();
        }

        public static object SearchData(string searchfield,string searchrange, Controller ctrl)
        {
            var tables = new List<object>();
            if (string.Compare(searchfield, SEARCHFIELD.ALLFIELDS) == 0)
            {
                tables.Add(SearchYield(searchrange, ctrl));
                tables.Add(SearchMachineRate(searchrange, ctrl));
                tables.Add(SearchCapacity(searchrange, ctrl));
                tables.Add(SearchInventory(searchrange, ctrl));
            }
            else if (string.Compare(searchfield, SEARCHFIELD.YIELD) == 0)
            {
                tables.Add(SearchYield(searchrange,ctrl));
            }
            else if (string.Compare(searchfield, SEARCHFIELD.MACHINERATE) == 0)
            {
                tables.Add(SearchMachineRate(searchrange,ctrl));
            }
            else if (string.Compare(searchfield, SEARCHFIELD.SHIPQTY) == 0)
            {
                if (string.Compare(searchrange, "LINECARD") == 0)
                { searchrange = "LNCD"; }
            }
            else if (string.Compare(searchfield, SEARCHFIELD.SHIPOUTPUT) == 0)
            {
                if (string.Compare(searchrange, "LINECARD") == 0)
                { searchrange = "LNCD"; }
            }
            else if (string.Compare(searchfield, SEARCHFIELD.OTDQTY) == 0)
            {
                if (string.Compare(searchrange, "LINECARD") == 0)
                { searchrange = "LNCD"; }
            }
            else if (string.Compare(searchfield, SEARCHFIELD.OTDOUTPUT) == 0)
            {
                if (string.Compare(searchrange, "LINECARD") == 0)
                { searchrange = "LNCD"; }
            }
            else if (string.Compare(searchfield, SEARCHFIELD.SCRAP) == 0)
            {
                tables.Add(SearchScrap(searchrange, ctrl));
            }
            else if (string.Compare(searchfield, SEARCHFIELD.CAPACITY) == 0)
            {
                tables.Add(SearchCapacity(searchrange, ctrl));
            }
            else if (string.Compare(searchfield, SEARCHFIELD.INVENTORY) == 0)
            {
                tables.Add(SearchInventory(searchrange, ctrl));
            }

            return tables;
        }

        private static object SearchYield(string searchrange, Controller ctrl)
        {
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            
            if (searchcfg["YIELDFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
                var yieldlist = new List<ProductYield>();
                yieldlist.Add(YieldVM.RetrievePDFamilyYield(searchrange,yieldcfg,ctrl));
                return YieldVM.GetYieldTable(yieldlist, "Yield", true);
            }
            else
            {
                var yieldlist = YieldVM.RetrieveProductYieldByYF(searchrange, ctrl);
                return YieldVM.GetYieldTable(yieldlist, "Yield", false);
            }
        }

        private static object SearchMachineRate(string searchrange, Controller ctrl)
        {
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["YIELDFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var machinerate = MachineRate.RetrieveAllMachineByYF(searchrange, ctrl);
                return MachineRate.GetMachineTable(machinerate, "Machine Rate", true);
            }
            else
            {
                var machinerate = MachineRate.RetrieveProductMachineByYF(searchrange, ctrl);
                return MachineRate.GetMachineTable(machinerate, "Machine Rate", false);
            }
        }

        private static object SearchCapacity(string searchrange, Controller ctrl)
        {
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["CAPACITYFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var capdatalist = CapacityRawData.RetrieveDataByProductType(searchrange);
                return CapacityRawData.GetCapacityTable(capdatalist, true);
            }
            else
            {
                var pdlist = new List<string>();
                pdlist.Add(searchrange);
                var capdatalist = CapacityRawData.RetrieveDataByProd(pdlist);
                return CapacityRawData.GetCapacityTable(capdatalist, false);
            }
        }

        private static object SearchInventory(string searchrange, Controller ctrl)
        {
            if (string.Compare(searchrange, "TUNABLE") == 0)
            { searchrange = "10G Tunable"; }
            if (string.Compare(searchrange, "SFP+ WIRE") == 0)
            { searchrange = "SFP+WIRE"; }

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["INVENTORYFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var inventdata = InventoryData.RetrieveAllTrendDataByDP(searchrange);
                return InventoryData.GetInventoryDataTable(inventdata,true);
            }
            else
            {
                var pdlist = new List<string>();
                pdlist.Add(searchrange);
                var inventdata = InventoryData.RetrieveDetailDataByStandardPD(pdlist);
                return InventoryData.GetInventoryDataTable(inventdata,false);
            }
       }

        private static object SearchScrap(string searchrange, Controller ctrl)
        {
            if (string.Compare(searchrange, "LINECARD") == 0)
            { searchrange = "LNCD"; }
            if (string.Compare(searchrange, "TUNABLE") == 0)
            { searchrange = "Telecom TRX"; }

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["SCRAPFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var scrapdata = ScrapData_Base.RetrieveScrapDataByPG(searchrange);
                return ScrapData_Base.GetScrapTable(scrapdata, searchrange, true);
            }
            else
            {
                var scrapdata = ScrapData_Base.RetrieveScrapDataByStandardPD(searchrange);
                return ScrapData_Base.GetScrapTable(scrapdata, searchrange, false);
            }
        }



    }
}