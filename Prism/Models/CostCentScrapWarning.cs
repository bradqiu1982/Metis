using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prism.Models
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

            var scrapratewarningtable = new List<List<string>>();
            var scrapwarningtable = new List<List<string>>();

            var iebugetdict = IEScrapBuget.RetrieveDataCentDict(fyear, fquarter);
            var costcenterpjdict = new Dictionary<string, string>();
            var copmmap = ExternalDataCollector.GetCostCenterPMMap(costcenterpjdict);

            foreach (var dp in departmentlist)
            {
                var costcenterlist = ScrapData_Base.RetrievePJCodeByDepartment(dp, fyear, fquarter);
                
                foreach (var co in costcenterlist)
                {
                    var bugetrate = 0.0;
                    var boutput = 0.0;
                    var bscrap = 0.0;
                    
                    if (iebugetdict.ContainsKey(co))
                    {

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

                        if (costcenterpjdict.ContainsKey(co))
                        { templist.Add(costcenterpjdict[co]); }
                        else
                        { templist.Add(""); }

                        if (copmmap.ContainsKey(co))
                        { templist.Add(copmmap[co]); }
                        else
                        { templist.Add(""); }


                        scrapratewarningtable.Add(templist);

                    }

                    if (sumdata.generalscrap > bscrap)
                    {
                        var templist = new List<string>();
                        templist.Add(dp);
                        templist.Add(co);
                        templist.Add(Math.Round(bscrap,2).ToString());
                        templist.Add(Math.Round(sumdata.generalscrap,2).ToString());

                        if (costcenterpjdict.ContainsKey(co))
                        { templist.Add(costcenterpjdict[co]); }
                        else
                        { templist.Add(""); }

                        if (copmmap.ContainsKey(co))
                        { templist.Add(copmmap[co]); }
                        else
                        { templist.Add(""); }


                        scrapwarningtable.Add(templist);
                    }

                }//end foreach
            }//end foreach

            var title = new List<string>();
            title.Add("Department");
            title.Add("Cost Center");
            title.Add("Buget Scrap Rate");
            title.Add("Actual Scrap Rate");
            title.Add("Product Name");
            title.Add("PM");
            if (scrapratewarningtable.Count > 0)
            { scrapratewarningtable.Insert(0, title); }

            title = new List<string>();
            title.Add("Department");
            title.Add("Cost Center");
            title.Add("Buget Scrap USD");
            title.Add("Actual Scrap USD");
            title.Add("Product Name");
            title.Add("PM");
            if (scrapwarningtable.Count > 0)
            { scrapwarningtable.Insert(0, title); }

            if (scrapratewarningtable.Count > 0 || scrapwarningtable.Count > 0)
            {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("x", string.Join(";", departmentlist));
                routevalue.Add("defyear", fyear);
                routevalue.Add("defqrt", fquarter);

                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string url = ctrl.Url.Action("CostCenterOfOneDepart", "DataAnalyze",routevalue, scheme);
                var netcomputername = EmailUtility.RetrieveCurrentMachineName();
                url = url.Replace("//localhost", "//" + netcomputername);

                var table = EmailUtility.CreateTableHtml("Hi Guys", "Blow is the scrap warning table:",url, scrapratewarningtable, scrapwarningtable);
                var tolist = syscfg["COSTCENTERSCRAPWARNINGLIST"].Split(new string[] { }, StringSplitOptions.RemoveEmptyEntries).ToList();
                EmailUtility.SendEmail(ctrl,"WUXI Engineering Scrap Warning",tolist,table);
                new System.Threading.ManualResetEvent(false).WaitOne(1000);
            }//end if
        }


    }
}