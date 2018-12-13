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
        public static string OTDQTY = "OTD QTY";
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
            ret.Add(SEARCHFIELD.OTDQTY);
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
            ret.Add("SFP+ TUNABLE");
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
                || string.Compare(searchfield, SEARCHFIELD.OTDQTY) == 0)
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
                SearchYield(searchrange, tables, ctrl);
                SearchMachineRate(searchrange, tables, ctrl);
                SearchCapacity(searchrange, tables, ctrl);
                SearchScrap(searchrange, tables, ctrl);
                SearchInventory(searchrange, tables, ctrl);
                SearchShipQty(searchrange, tables, ctrl);
                SearchOTDQty(searchrange, tables, ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.YIELD) == 0)
            {
                SearchYield(searchrange, tables, ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.MACHINERATE) == 0)
            {
                SearchMachineRate(searchrange, tables, ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.SHIPQTY) == 0)
            {
                SearchShipQty(searchrange, tables, ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.OTDQTY) == 0)
            {
                SearchOTDQty(searchrange, tables, ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.SCRAP) == 0)
            {
                SearchScrap(searchrange, tables, ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.CAPACITY) == 0)
            {
                SearchCapacity(searchrange, tables, ctrl);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.INVENTORY) == 0)
            {
                SearchInventory(searchrange, tables, ctrl);
            }

            return tables;
        }

        private static void SearchYield(string searchrange, List<object> tables, Controller ctrl)
        {
            if (string.Compare(searchrange, "SFP+ WIRE") == 0)
            { searchrange = "PARALLEL.SFPWIRE"; }
            if (string.Compare(searchrange, "10G Tunable BIDI") == 0)
            { searchrange = "BIDI P2P"; }
            if (string.Compare(searchrange, "T-XFP") == 0)
            { searchrange = "XFP TUNABLE"; }

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["YIELDFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
                var yieldlist = new List<ProductYield>();
                yieldlist.Add(YieldVM.RetrievePDFamilyYield(searchrange,yieldcfg,ctrl));
                if (yieldlist.Count == 0 || yieldlist[0].FirstYieldList.Count == 0) { return; }
                tables.Add(YieldVM.GetYieldTable(yieldlist, "Yield", true));
            }
            else
            {
                var yieldlist = YieldVM.RetrieveProductYieldByYF(searchrange, ctrl);
                if (yieldlist.Count == 0 || yieldlist[0].FirstYieldList.Count == 0) { return; }
                tables.Add(YieldVM.GetYieldTable(yieldlist, "Yield", false));
            }
        }

        private static void SearchMachineRate(string searchrange, List<object> tables, Controller ctrl)
        {
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["YIELDFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var machinerate = MachineRate.RetrieveAllMachineByYF(searchrange, ctrl);
                if (machinerate.Count == 0 || machinerate[0][0].MachineTimeList.Count == 0) { return; }
                tables.Add(MachineRate.GetMachineTable(machinerate, "Machine Rate", true));
            }
            else
            {
                var machinerate = MachineRate.RetrieveProductMachineByYF(searchrange, ctrl);
                if (machinerate.Count == 0 || machinerate[0][0].MachineTimeList.Count == 0) { return; }
                tables.Add(MachineRate.GetMachineTable(machinerate, "Machine Rate", false));
            }
        }

        private static void SearchCapacity(string searchrange, List<object> tables, Controller ctrl)
        {
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["CAPACITYFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var capdatalist = CapacityRawData.RetrieveDataByProductType(searchrange);
                if (capdatalist.Count == 0) { return; }
                tables.Add(CapacityRawData.GetCapacityTable(capdatalist, true));
            }
            else
            {
                var pdlist = new List<string>();
                pdlist.Add(searchrange);
                var capdatalist = CapacityRawData.RetrieveDataByProd(pdlist);
                if (capdatalist.Count == 0) { return; }
                tables.Add(CapacityRawData.GetCapacityTable(capdatalist, false));
            }
        }

        private static void SearchInventory(string searchrange, List<object> tables, Controller ctrl)
        {
            if (string.Compare(searchrange, "TUNABLE") == 0)
            { searchrange = "10G Tunable"; }
            if (string.Compare(searchrange, "SFP+ WIRE") == 0)
            { searchrange = "SFP+WIRE"; }

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["INVENTORYFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var inventdata = InventoryData.RetrieveAllTrendDataByDP(searchrange);
                if (inventdata.Count == 0) { return; }
                tables.Add(InventoryData.GetInventoryDataTable(inventdata,true));
            }
            else
            {
                var pdlist = new List<string>();
                pdlist.Add(searchrange);
                var inventdata = InventoryData.RetrieveDetailDataByStandardPD(pdlist);
                if (inventdata.Count == 0) { return; }
                tables.Add(InventoryData.GetInventoryDataTable(inventdata,false));
            }
       }

        private static void SearchScrap(string searchrange, List<object> tables, Controller ctrl)
        {
            if (string.Compare(searchrange, "LINECARD") == 0)
            { searchrange = "LNCD"; }
            if (string.Compare(searchrange, "TUNABLE") == 0)
            { searchrange = "Telecom TRX"; }

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["SCRAPFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var scrapdata = ScrapData_Base.RetrieveScrapDataByPG(searchrange);
                if (scrapdata.Count == 0) { return; }
                tables.Add(ScrapData_Base.GetScrapTable(scrapdata, searchrange, true));
            }
            else
            {
                var scrapdata = ScrapData_Base.RetrieveScrapDataByStandardPD(searchrange);
                if (scrapdata.Count == 0) { return; }
                tables.Add(ScrapData_Base.GetScrapTable(scrapdata, searchrange, false));
            }
        }

        private static void SearchShipQty(string searchrange, List<object> tables, Controller ctrl)
        {
            if (string.Compare(searchrange, "SFP+ WIRE") == 0)
            { searchrange = "PARALLEL.SFPWIRE"; }

            var shipdata = FsrShipData.RetrieveOutputDataByQuarter(searchrange, ctrl);
            if (shipdata.Count == 0 || shipdata[0].Count == 0) { return; }

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["SHIPFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            { tables.Add(FsrShipData.GetShipoutTable(shipdata, searchrange, true)); }
            else
            { tables.Add(FsrShipData.GetShipoutTable(shipdata, searchrange, false)); }
        }

        private static void SearchOTDQty(string searchrange, List<object> tables, Controller ctrl)
        {
            if (string.Compare(searchrange, "SFP+ WIRE") == 0)
            { searchrange = "PARALLEL.SFPWIRE"; }

            var shipdata = FsrShipData.RetrieveOTDDataByQuarter(searchrange, ctrl);
            if (shipdata.Count == 0 || shipdata[0].Count == 0) { return; }

            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            if (searchcfg["SHIPFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            { tables.Add(FsrShipData.GetOTDTable(shipdata, searchrange, true)); }
            else
            { tables.Add(FsrShipData.GetOTDTable(shipdata, searchrange, false)); }
        }

    }
}