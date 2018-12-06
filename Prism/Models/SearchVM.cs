using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class SEARCHFIELD
    {
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
            var prodlist = InventoryData.GetAllProductList();
            ret.AddRange(prodlist);
            return ret;
        }


        public static List<string> SearchRange(string searchfield,Controller ctrl)
        {
            if (string.Compare(searchfield, SEARCHFIELD.YIELD) == 0 
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

        public static object SearchData(string searchfield,string searchrange, Controller ctrl,bool left= true)
        {
            if (string.Compare(searchfield, SEARCHFIELD.YIELD) == 0)
            {
                return SearchYield(searchrange,ctrl,left);
            }
            else if (string.Compare(searchfield, SEARCHFIELD.MACHINERATE) == 0)
            {

            }
            else if (string.Compare(searchfield, SEARCHFIELD.SHIPQTY) == 0)
            {

            }
            else if (string.Compare(searchfield, SEARCHFIELD.SHIPOUTPUT) == 0)
            {

            }
            else if (string.Compare(searchfield, SEARCHFIELD.OTDQTY) == 0)
            {

            }
            else if (string.Compare(searchfield, SEARCHFIELD.OTDOUTPUT) == 0)
            {

            }
            else if (string.Compare(searchfield, SEARCHFIELD.SCRAP) == 0)
            {

            }
            else if (string.Compare(searchfield, SEARCHFIELD.CAPACITY) == 0)
            {

            }
            else if (string.Compare(searchfield, SEARCHFIELD.INVENTORY) == 0)
            {

            }

            return new { };
        }

        private static object SearchYield(string searchrange, Controller ctrl, bool left)
        {
            var searchcfg = CfgUtility.LoadSearchConfig(ctrl);
            
            if (searchcfg["YIELDFAMILY"].ToUpper().Contains(searchrange.ToUpper()))
            {
                var yieldcfg = CfgUtility.LoadYieldConfig(ctrl);
                var yieldlist = new List<ProductYield>();
                yieldlist.Add(YieldVM.RetrievePDFamilyYield(searchrange,yieldcfg,ctrl));
                return YieldVM.GetYieldTableAndChart(yieldlist, "Department" ,searchrange + " Yield", true,left ? "left_chart_id": "right_chart_id");
            }
            else
            {
                var yieldlist = YieldVM.RetrieveProductYieldByYF(searchrange, ctrl);
                return YieldVM.GetYieldTableAndChart(yieldlist, "Product", searchrange + " Yield", false, left ? "left_chart_id" : "right_chart_id");
            }
        }


    }
}