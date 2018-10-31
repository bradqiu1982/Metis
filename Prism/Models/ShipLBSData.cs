using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
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

        }

        public string code3 { set; get; }
        public string name { set; get; }
        public double value { set; get; }
        public string code { set; get; }
    }
}