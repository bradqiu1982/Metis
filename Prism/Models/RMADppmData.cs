using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public static List<RMADppmData> RetrieveRMARawDataByMonth(string sdate, string edate,string producttype)
        {
            return RMARAWData.RetrieveRMARawDataByMonth(sdate, edate, producttype);
        }

        public static List<RMADppmData> RetrieveRMAWorkLoadDataByMonth(string sdate, string edate, string producttype)
        {
            return RMARAWData.RetrieveRMAWorkLoadDataByMonth(sdate, edate, producttype);
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