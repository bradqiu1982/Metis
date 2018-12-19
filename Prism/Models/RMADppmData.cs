using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class RMADppmData
    {
        public RMADppmData()
        {
            ID = string.Empty;
            RMANum = string.Empty;
            ProductType = string.Empty;
            PN = string.Empty;
            QTY = 0;
            IssueOpenDate = DateTime.Parse("1982-05-06 10:00:00");
            PNDesc = string.Empty;
            SN = string.Empty;
            RootCause = string.Empty;
            Rate = string.Empty;
            Product = string.Empty;
        }

        public RMADppmData( string rmanum, string producttype, string pn, string pndesc, string sn, double qty, DateTime issuedate, string rootcause)
        {
            ID = "";
            RMANum = rmanum;
            ProductType = producttype;
            PN = pn;
            PNDesc = pndesc;
            SN = sn;
            QTY = qty;
            IssueOpenDate = issuedate;
            RootCause = rootcause;
            Rate = string.Empty;
            Product = string.Empty;
        }

        public static List<object> RetrieveRMACntByMonth(string sdate, string edate, string producttype)
        {
            return RMARAWData.RetrieveRMACntByMonth(sdate, edate, producttype);
        }

        public static List<RMADppmData> RetrieveRMARawDataByMonth(string sdate, string edate,string producttype,Controller ctrl)
        {
            return RMARAWData.RetrieveRMARawDataByMonth(sdate, edate, producttype,ctrl);
        }

        public static List<RMADppmData> RetrieveRMAWorkLoadDataByMonth(string sdate, string edate, string producttype)
        {
            return RMARAWData.RetrieveRMAWorkLoadDataByMonth(sdate, edate, producttype);
        }

        public static object GetShipoutTable(Dictionary<string, double> shipqtydict, List<RMADppmData> rmadata,string searchrange)
        {
            var rmadict = new Dictionary<string, double>();
            foreach (var rma in rmadata)
            {
                var q = QuarterCLA.RetrieveQuarterFromDate(rma.IssueOpenDate);
                if (rmadict.ContainsKey(q))
                {
                    rmadict[q] += rma.QTY;
                }
                else
                {
                    rmadict.Add(q, rma.QTY);
                }
            }



            var qlist = shipqtydict.Keys.ToList();
            qlist.Sort(delegate (string obj1, string obj2)
            {
                var i1 = QuarterCLA.RetrieveDateFromQuarter(obj1)[0];
                var i2 = QuarterCLA.RetrieveDateFromQuarter(obj2)[0];
                return i1.CompareTo(i2);
            });

            var titlelist = new List<object>();
            titlelist.Add("RMA");
            titlelist.Add("");
            titlelist.AddRange(qlist);

            var linelist = new List<object>();
            linelist.Add(searchrange);


            linelist.Add("<span class='YFPY'>RMA_Qty</span><br><span class='INPUT'>Ship_Qty</span><br><span class='YFY'>DPPM</span>");

            //var outputiszero = false;
            var sumscraplist = new List<SCRAPSUMData>();
            foreach (var q in qlist)
            {
                var rmaqty = 0.0;
                if (rmadict.ContainsKey(q))
                { rmaqty = rmadict[q]; }
                var dppm = Math.Round(rmaqty / shipqtydict[q] * 1000000, 0);

                linelist.Add("<span class='YFPY'>"+Math.Round(rmaqty, 0)+"</span><br><span class='INPUT'>"+ String.Format("{0:n0}", shipqtydict[q]) + "</span><br><span class='YFY'>"+ dppm + "</span>");
            }

            return new
            {
                tabletitle = titlelist,
                tablecontent = linelist
            };

        }

        //RMA RAW DATA MAP
        //RMANum AppV_B
        //ProductType AppV_F
        //PN AppV_G
        //PNDesc AppV_H
        //SN AppV_I
        //QTY AppV_J
        //IssueOpenDate(FV DATE)  AppV_W
        //RootCause AppV_Y
        //FVResult AppV_X
        //CaseType AppV_Z
        //Rate AppV_AI
        //Product AppV_AJ

        public string ID { set; get; }
        public string RMANum { set; get; }
        public string ProductType { set; get; }
        public string PN { set; get; }
        public string PNDesc { set; get; }
        public string SN { set; get; }
        public double QTY { set; get; }
        public DateTime IssueOpenDate { set; get; }
        public string IssueDateStr { get { return IssueOpenDate.ToString("yyyy-MM-dd"); } }
        public DateTime InitFAR { set; get; }
        public string InitFARStr { get { return InitFAR.ToString("yyyy-MM-dd"); } }
        public string RootCause { set; get; }
        public string Rate { set; get; }
        public string Product { set; get; }
    }
}