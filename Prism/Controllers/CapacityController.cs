using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;

namespace Prism.Controllers
{
    public class CapacityController : Controller
    {
        public ActionResult DepartmentCapacity()
        {
            return View();
        }

        private List<string> RetrieveQuarterFromYield(List<CapacityRawData> cdatas)
        {
            var quarterdict = new Dictionary<string, bool>();
            foreach (var cd in cdatas)
            {
                if (!quarterdict.ContainsKey(cd.Quarter))
                {
                    quarterdict.Add(cd.Quarter, true);
                }
            }
            var qlist = quarterdict.Keys.ToList();
            qlist.Sort(delegate (string q1, string q2)
            {
                var qd1 = QuarterCLA.RetrieveDateFromQuarter(q1);
                var qd2 = QuarterCLA.RetrieveDateFromQuarter(q2);
                return qd1[0].CompareTo(qd2[0]);
            });
            return qlist;
        }

        private JsonResult GetCapacityChart(List<CapacityRawData> capdatalist, bool fordepartment = true)
        {
            var quarterdict = new Dictionary<string, bool>();
            var rawdatadict = new Dictionary<string, List<CapacityRawData>>();
            var pddict = new Dictionary<string, Dictionary<string, CapacityRawData>>();
            foreach (var cd in capdatalist)
            {
                if (!quarterdict.ContainsKey(cd.Quarter))
                { quarterdict.Add(cd.Quarter, true); }

                var pd = cd.ProductType;
                if (!fordepartment)
                { pd = cd.Product; }

                if (rawdatadict.ContainsKey(pd))
                { rawdatadict[pd].Add(cd); }
                else
                {
                    var templist = new List<CapacityRawData>();
                    templist.Add(cd);
                    rawdatadict.Add(pd, templist);
                }

                if (pddict.ContainsKey(pd))
                {
                    var qdict = pddict[pd];
                    if (qdict.ContainsKey(cd.Quarter))
                    {
                        qdict[cd.Quarter].MaxCapacity += cd.MaxCapacity;
                        qdict[cd.Quarter].ForeCast += cd.ForeCast;
                    }
                    else
                    {
                        var tempvm = new CapacityRawData();
                        tempvm.MaxCapacity = cd.MaxCapacity;
                        tempvm.ForeCast = cd.ForeCast;
                        qdict.Add(cd.Quarter, tempvm);
                    }
                }
                else
                {
                    var tempvm = new CapacityRawData();
                    tempvm.MaxCapacity = cd.MaxCapacity;
                    tempvm.ForeCast = cd.ForeCast;
                    var qdict = new Dictionary<string, CapacityRawData>();
                    qdict.Add(cd.Quarter, tempvm);
                    pddict.Add(pd, qdict);
                }
            }

            var qlist = quarterdict.Keys.ToList();
            qlist.Sort(delegate (string q1, string q2)
            {
                var qd1 = QuarterCLA.RetrieveDateFromQuarter(q1);
                var qd2 = QuarterCLA.RetrieveDateFromQuarter(q2);
                return qd1[0].CompareTo(qd2[0]);
            });

            var titlelist = new List<object>();
            titlelist.Add("Department");
            titlelist.Add("");
            titlelist.AddRange(qlist);

            var pdlist = pddict.Keys.ToList();
            var contentlist = new List<object>();
            foreach (var pd in pdlist)
            {
                var linelist = new List<object>();
                linelist.Add(pd);
                linelist.Add("<span class='YFPY'>Max Capacity</span><br><span class='YFY'>Forecast</span><br><span class='YINPUT'>Cusume Rate %</span><br><span class='YINPUT'>Buffer</span>");
                
                var qtdata = pddict[pd];
                foreach (var q in qlist)
                {
                    if (qtdata.ContainsKey(q))
                    { linelist.Add("<span class='YFPY YIELDDATA' myid='" + pd+"'>"+qtdata[q].MaxCapacity+ "</span><br><span class='YFY YIELDDATA' myid='" + pd+ "'>" + qtdata[q].ForeCast + "</span><br><span class='YINPUT'>" + qtdata[q].Usage + "</span><br><span class='YINPUT'>" + qtdata[q].GAP + "</span>"); }
                    else
                    { linelist.Add(" "); }
                }
                contentlist.Add(linelist);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                tabletitle = titlelist,
                tablecontent = contentlist,
                rawdata = rawdatadict
            };
            return ret;
        }

        public JsonResult DepartmentCapacityData()
        {
            var capdatalist = CapacityRawData.RetrieveAllData();
            return GetCapacityChart(capdatalist);
        }
        
    }
}