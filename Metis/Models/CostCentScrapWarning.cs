using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Metis.Models
{
    public class CostCentScrapWarning
    {
        private static double ConvertToDouble(string val)
        {
            try
            {
                return Math.Round(Convert.ToDouble(val), 2);
            }
            catch (Exception ex) { return 0.0; }
        }

        public static void Waring(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var now = DateTime.Now;
            var fyear = ExternalDataCollector.GetFYearByTime(now);
            var fquarter = ExternalDataCollector.GetFQuarterByTime(now);
            var departmentlist = syscfg["COSTCENTERSCRAPWARNING"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var warningtable = new List<List<string>>();

            var iebugetdict = IEScrapBuget.RetrieveDataCentDict(fyear, fquarter);
            var copmmap = ExternalDataCollector.GetCostCenterPMMap();

            foreach (var dp in departmentlist)
            {
                var costcenterlist = ScrapData_Base.RetrievePJCodeByDepartment(dp, fyear, fquarter);
                
                foreach (var co in costcenterlist)
                {
                    var bugetrate = 0.0;
                    if (iebugetdict.ContainsKey(co))
                    {
                        var boutput = 0.0;
                        var bscrap = 0.0;
                        foreach (var item in iebugetdict[co])
                        {
                            boutput += ConvertToDouble(item.OutPut);
                            bscrap += ConvertToDouble(item.Scrap);
                        }
                        bugetrate = Math.Round(bscrap / boutput * 100.0, 2);
                    }
                    else
                    { continue; }

                    var onepjdata = ScrapData_Base.RetrieveCostCenterQuarterData(co, fyear, fquarter);
                    var sumdata = new SCRAPSUMData();
                    foreach (var item in onepjdata)
                    {
                        if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.OUTPUT, true) == 0)
                        {
                            sumdata.output += ConvertToDouble(item.Transaction_Value_Usd_1);
                        }
                        else if (string.Compare(item.Scrap_Or_Output, SCRAPOUTPUTSCRAP.SCRAP, true) == 0)
                        {
                            if (string.Compare(item.REASON_NAME, SCRAPTYPE.GENERALSCRAP) == 0)
                            {
                                sumdata.generalscrap += ConvertToDouble(item.Transaction_Value_Usd_1);
                            }
                        }
                    }

                    if (sumdata.output == 0.0)
                    { continue; }

                    var grate = Math.Round(sumdata.generalscrap / sumdata.output * 100.0, 2);
                    if (grate > bugetrate)
                    {
                        var templist = new List<string>();
                        templist.Add(dp);
                        templist.Add(co);
                        templist.Add(bugetrate.ToString());
                        templist.Add(grate.ToString());
                        if (copmmap.ContainsKey(co))
                        { templist.Add(copmmap[co]); }
                        else
                        { templist.Add(""); }
                        warningtable.Add(templist);
                    }
                }//end foreach
            }//end foreach

            var title = new List<string>();
            title.Add("Department");
            title.Add("Cost Center");
            title.Add("Buget Scrap Rate");
            title.Add("Actual Scrap Rate");
            title.Add("PM");
            if (warningtable.Count > 0)
            { warningtable.Insert(0, title); }

            if (warningtable.Count > 0)
            {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("x", string.Join(";", departmentlist));
                routevalue.Add("defyear", fyear);
                routevalue.Add("defqrt", fquarter);

                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string url = ctrl.Url.Action("CostCenterOfOneDepart", "DataAnalyze",routevalue, scheme);
                var netcomputername = EmailUtility.RetrieveCurrentMachineName();
                url = url.Replace("//localhost", "//" + netcomputername);

                var table = EmailUtility.CreateTableHtml("Hi Guys", "Blow is the scrap warning table:",url, warningtable);
                var tolist = syscfg["COSTCENTERWARNINGLIST"].Split(new string[] { }, StringSplitOptions.RemoveEmptyEntries).ToList();
                EmailUtility.SendEmail(ctrl,"WUXI Engineering Scrap Warning",tolist,table);
                new System.Threading.ManualResetEvent(false).WaitOne(1000);
            }//end if
        }


    }
}