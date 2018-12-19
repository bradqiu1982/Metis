using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class RMASumData
    {
        public RMASumData()
        {
            SumType = string.Empty;
            RMAName = string.Empty;
            RMAQty = 0;
            RMARate = 0;
        }

        public static List<RMASumData> RetrieveRMARawDataByMonth(string sdate, string edate, string producttype, Controller ctrl)
        {
            var rmadatalist = RMADppmData.RetrieveRMARawDataByMonth(sdate, edate, producttype, ctrl);
            var pddict = new Dictionary<string, RMASumData>();
            var rtdict = new Dictionary<string, RMASumData>();
            var sumqty = 0.0;
            foreach (var item in rmadatalist)
            {
                sumqty += item.QTY;

                if (pddict.ContainsKey(item.Product))
                {
                    pddict[item.Product].RMAQty += item.QTY;
                }
                else
                {
                    var tempvm = new RMASumData();
                    tempvm.SumType = "PRODUCT";
                    tempvm.RMAName = item.Product;
                    tempvm.RMAQty = item.QTY;
                    pddict.Add(item.Product, tempvm);
                }

                if (rtdict.ContainsKey(item.RootCause))
                {
                    rtdict[item.RootCause].RMAQty += item.QTY;
                }
                else
                {
                    var tempvm = new RMASumData();
                    tempvm.SumType = "ROOTCAUSE";
                    tempvm.RMAName = item.RootCause;
                    tempvm.RMAQty = item.QTY;
                    rtdict.Add(item.RootCause, tempvm);
                }
            }//end foreach

            var pdlist = pddict.Values.ToList();
            foreach (var item in pdlist)
            { item.RMARate = Math.Round(item.RMAQty / sumqty*100.0, 2); }
            var rtlist = rtdict.Values.ToList();
            foreach (var item in rtlist)
            { item.RMARate = Math.Round(item.RMAQty / sumqty*100.0, 2); }

            pdlist.Sort(delegate (RMASumData obj1, RMASumData obj2)
            { return obj2.RMAQty.CompareTo(obj1.RMAQty); });
            rtlist.Sort(delegate (RMASumData obj1, RMASumData obj2)
            { return obj2.RMAQty.CompareTo(obj1.RMAQty); });
            pdlist.AddRange(rtlist);

            return pdlist;
        }

        public string SumType { set; get; }
        public string RMAName { set; get; }
        public double RMAQty { set; get; }
        public double RMARate { set; get; }
    }
}