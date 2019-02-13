using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class JOItem
    {
        public JOItem()
        {
            JO = "";
            PN = "";
            ReleaseDate = "";
            LastActiveDate = "";
            JOStatus = "";
            SNActive = 0;
            SNClosed = 0;
            SNHold = 0;
            Qty = 0;
        }

        public string JO { set; get; }
        public string PN { set; get; }
        public int Qty { set; get; }
        public string ReleaseDate { set; get; }
        public string LastActiveDate { set; get; }
        public string JOStatus { set; get; }
        public int SNActive { set; get; }
        public int SNClosed { set; get; }
        public int SNHold { set; get; }
    }

    public class JOVM
    {
        private static object GetChartData(string title, Dictionary<string, int> wfcntdict)
        {
            var colorarray = new string[] { "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" };
            var colorlist = colorarray.ToList();

          
            var wflist = wfcntdict.Keys.ToList();
            wflist.Sort();

            var cidx = 0;
            var datalist = new List<object>();
            foreach (var key in wflist)
            {
                datalist.Add(new
                {
                    x = cidx,
                    y = wfcntdict[key],
                    color = colorlist[cidx % colorlist.Count]
                });
                cidx = cidx + 1;
            }

            var dataLabels = new
            {
                enabled = true,
                color = "#FFFFFF",
                align = "center",
                format = "{point.y}"
            };

            var allx = new { data = wflist };
            var ally = new { title = "Amount" };
            var alldata = new List<object>();
            alldata.Add(new
            {
                name = "module",
                data = datalist,
                dataLabels = dataLabels
            });

            return new
            {
                id = "jomesstatus",
                title = title + " Distribution",
                coltype = "normal",
                xAxis = allx,
                yAxis = ally,
                data = alldata
            };
        }

        public static List<object> QueryJO(string title,string pncond, string startdate, string enddate)
        {
            var ret = new List<object>();
            var sql = @"SELECT C.containername,pb.ProductName ,MO.MfgOrderName,MO.Qty,MO.ReleaseDate,w.WorkflowStepName, C.OriginalStartDate
                      , case when MO.ClosedDate is null then 'Active' else 'Closed' end JOStatus
                      , case when c.status = 1 then 'ACTIVE' when c.status = 2 then 'CLOSED' else 'HOLD' end SNStatus
                      ,case when c.HoldReasonId is not null then 'HOLD' else '' end HoldSatus 
                      FROM Insite.ProductBase pb WITH  (NOLOCK) 
                      left join Insite.Product p on pb.ProductBaseId = p.ProductBaseId 
                      left join Insite.Container C on c.ProductId = p.ProductId 
                      Left join insite.currentstatus S (nolock) on s.currentstatusid = C.CurrentStatusId 
                       Left join insite.WorkflowStep w (nolock) on w.WorkflowStepId = s.WorkflowStepId
                       LEFT JOIN Insite.MfgOrder MO WITH (NOLOCK) ON MO.MfgOrderId = C.MfgOrderId 
                       WHERE Len(C.containername) = 7 
                     and pb.ProductName in <pncond> and  MO.ReleaseDate > '<startdate>' and MO.ReleaseDate < '<enddate>' ORDER BY  c.OriginalStartDate DESC";

            sql = sql.Replace("<pncond>", pncond).Replace("<startdate>", startdate).Replace("<enddate>", enddate);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            var sndict = new Dictionary<string, bool>();
            var workflowdict = new Dictionary<string, int>();
            var jodict = new Dictionary<string, JOItem>();
            var joholddict = new Dictionary<string, List<string>>();

            foreach (var line in dbret)
            {
                try
                {
                    var sn = Convert.ToString(line[0]);
                    if (!sndict.ContainsKey(sn))
                    {
                        sndict.Add(sn, true);
                    
                        var jo = Convert.ToString(line[2]);
                        var workflow = Convert.ToString(line[5]).ToUpper();
                        var snstatus = Convert.ToString(line[8]);
                        var snholdstatus = Convert.ToString(line[9]);
                        if (!string.IsNullOrEmpty(snholdstatus))
                        { snstatus = "HOLD"; }


                        if (workflowdict.ContainsKey(workflow))
                        { workflowdict[workflow] += 1; }
                        else
                        { workflowdict.Add(workflow, 1); }

                        if (jodict.ContainsKey(jo))
                        {
                            if (snstatus.Contains("ACTIVE"))
                            { jodict[jo].SNActive += 1; }
                            else if (snstatus.Contains("CLOSED"))
                            { jodict[jo].SNClosed += 1; }
                            else
                            {
                                if (joholddict.ContainsKey(jo))
                                { joholddict[jo].Add(sn); }
                                else
                                {
                                    var templist = new List<string>();
                                    templist.Add(sn);
                                    joholddict.Add(jo, templist);
                                }
                                jodict[jo].SNHold += 1;
                            }
                        }
                        else
                        {
                            var pn = Convert.ToString(line[1]);
                            var qty = Convert.ToInt32(line[3]);
                            var releasedate = Convert.ToDateTime(line[4]).ToString("yyyy-MM-dd");
                            var joactivedate = Convert.ToDateTime(line[6]).ToString("yyyy-MM-dd");
                            var jostatus = Convert.ToString(line[7]);

                            var tempvm = new JOItem();
                            tempvm.JO = jo;
                            tempvm.PN = pn;
                            tempvm.Qty = qty;
                            tempvm.ReleaseDate = releasedate;
                            tempvm.JOStatus = jostatus;
                            tempvm.LastActiveDate = joactivedate;
                            if (snstatus.Contains("ACTIVE"))
                            { tempvm.SNActive = 1; }
                            else if (snstatus.Contains("CLOSED"))
                            { tempvm.SNClosed = 1; }
                            else
                            {
                                if (joholddict.ContainsKey(jo))
                                { joholddict[jo].Add(sn); }
                                else
                                {
                                    var templist = new List<string>();
                                    templist.Add(sn);
                                    joholddict.Add(jo, templist);
                                }
                                tempvm.SNHold = 1;
                            }

                            jodict.Add(jo,tempvm);
                        }
                    }
                }
                catch (Exception ex) { }
            }

            var jolist = jodict.Values.ToList();
            if (jolist.Count == 0)
            {
                return ret;
            }

            jolist.Sort(delegate (JOItem obj1, JOItem obj2)
            {
                var date1 = Convert.ToDateTime(obj1.ReleaseDate);
                var date2 = Convert.ToDateTime(obj2.ReleaseDate);
                return date2.CompareTo(date1);
            });

            ret.Add(jolist);
            ret.Add(joholddict);
            ret.Add(GetChartData(title,workflowdict));
            return ret;
        }
    }
}