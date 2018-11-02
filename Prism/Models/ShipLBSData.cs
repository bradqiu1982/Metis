using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class CapitalData
    {
        public CapitalData()
        {
            cyname = "";
            ctname = "";
            lat = 0.0;
            lon = 0.0;
            code = "";
            contine = "";
        }

        public static Dictionary<string, CapitalData> LoadCapitialData(Controller ctrl)
        {
            var marks = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/highmaps/capitals.json"));
            List<CapitalData> caplist = (List<CapitalData>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<CapitalData>()).GetType());
            var ret = new Dictionary<string, CapitalData>();

            foreach (var item in caplist)
            {
                if (string.IsNullOrEmpty(item.ctname) 
                    || string.Compare(item.ctname.Trim(), "NULL", true) == 0
                    || string.Compare(item.ctname.Trim(), "N/A", true) == 0)
                { continue; }
                if (string.IsNullOrEmpty(item.code)
                    || string.Compare(item.code.Trim(), "NULL", true) == 0
                    || string.Compare(item.code.Trim(), "N/A", true) == 0)
                { continue; }
                if (item.lat == 0 && item.lon == 0)
                { continue; }

                var vm = new CapitalData();
                vm.code = item.code.Trim().ToUpper();
                vm.ctname = item.ctname.Replace(" ","").Replace("-", "").ToLower();
                vm.lat = Math.Round(item.lat,3);
                vm.lon = Math.Round(item.lon,3);
                if (!ret.ContainsKey(vm.code))
                { ret.Add(vm.code, vm); }
            }

            return ret;
        }

        public string cyname { set; get; }
        public string ctname { set; get; }
        public double lat { set; get; }
        public double lon { set; get; }
        public string code { set; get; }
        public string contine { set; get; }
    }

    public class ShipLBSData
    {
        public ShipLBSData()
        {
            code3 = "";
            name = "";
            value = 1;
            code = "";
        }

        private static List<ShipLBSData> LoadSampleData(Controller ctrl)
        {
            var marks = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/highmaps/world.json"));
            List<ShipLBSData> lbslist = (List<ShipLBSData>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<ShipLBSData>()).GetType());
            foreach (var item in lbslist)
            { item.value = 1; }
            return lbslist;
        }

        public static List<ShipLBSData> LoadShipdataLBS(string producttype,string startdate,string enddate,Controller ctrl)
        {
            var basedata = LoadSampleData(ctrl);
            var shiplist = FsrShipData.RetrieveLBSDataByMonth(producttype, startdate, enddate, ctrl);
            var shiptodict = new Dictionary<string, double>();
            foreach (var item in shiplist)
            {
                if (shiptodict.ContainsKey(item.ShipTo))
                { shiptodict[item.ShipTo] += item.ShipQty; }
                else
                { shiptodict.Add(item.ShipTo, item.ShipQty); }
            }

            var matchdict = new Dictionary<string, bool>();
            foreach (var item in basedata)
            {
                if (shiptodict.ContainsKey(item.code))
                {
                    item.value = shiptodict[item.code];
                    if (!matchdict.ContainsKey(item.code))
                    { matchdict.Add(item.code, true); }
                }
            }

            foreach (var kv in shiptodict)
            {
                if (!matchdict.ContainsKey(kv.Key))
                {
                    var vm = new ShipLBSData();
                    vm.code = kv.Key;
                    vm.code3 = kv.Key;
                    vm.name = kv.Key;
                    vm.value = kv.Value;
                    basedata.Add(vm);
                }
            }

            return basedata;
        }

        public string code3 { set; get; }
        public string name { set; get; }
        public double value { set; get; }
        public string code { set; get; }
    }
}