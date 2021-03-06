﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        private static object GetChartData(string title, Dictionary<string, int> wfcntdict,string defdb,Controller ctrl)
        {
            var colorarray = new string[] { "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" };
            var colorlist = colorarray.ToList();

            var wflist = new List<string>();
            if (string.Compare(defdb, "ATE") == 0)
            {
                var syscfg = CfgUtility.GetSysConfig(ctrl);
                var allwflist = syscfg["JOQUERYWORKFLOW"].Split(new string[] { ";" },StringSplitOptions.RemoveEmptyEntries).ToList();

                var tempwflist = wfcntdict.Keys.ToList();
                tempwflist.Sort();

                foreach (var item in allwflist)
                {
                    if (tempwflist.Contains(item))
                    {
                        wflist.Add(item);
                        tempwflist.Remove(item);
                    }
                }

                wflist.AddRange(tempwflist);
            }
            else
            {
                wflist = wfcntdict.Keys.ToList();
                wflist.Sort();
            }


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


        private static List<List<object>> QueryMESJO(string pncond, string startdate, string enddate)
        {
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
            return DBUtility.ExeMESSqlWithRes(sql);
        }

        private static List<List<object>> QueryATEJO(string pncond, string startdate, string enddate)
        {
            var sql = @"SELECT distinct  a.MFR_SN,a.MFR_PN,b.JOB_ID,j.oracle_qty,j.time,b.state,ds.maxtime,
                        'Active' as JOStatus,
                        case when b.status = 'END_ROUTE' then 'CLOSED' when b.state = 'TROUBLESHOOT' then 'HOLD' else 'ACTIVE' end SNStatus
                        FROM PARTS a   
                        INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX  
                        INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID 
                        INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID 
                        INNER JOIN JOBS j ON j.job_id = b.job_id
                        inner join (select ROUTE_ID,max(start_time) as maxtime from datasets group by ROUTE_ID) ds on ds.ROUTE_ID = b.route_id
                        WHERE  a.mfr_pn in <pncond> and d.start_time >= '<startdate>' and d.start_time < '<enddate>' AND b.state <> 'GOLDEN' order by a.mfr_sn,ds.maxtime desc";

            sql = sql.Replace("<pncond>", pncond).Replace("<startdate>", DateTime.Parse(startdate).ToString("yyyyMMddHHmmss")).Replace("<enddate>", DateTime.Parse(enddate).ToString("yyyyMMddHHmmss"));
            return DBUtility.ExeATESqlWithRes(sql);
        }


        public static List<object> QueryJO(string title,string pncond, string startdate, string enddate,Controller ctrl,string defdb="MES")
        {
            var ret = new List<object>();
            var dbret = new List<List<object>>();
            if (string.Compare(defdb, "ATE", true) == 0)
            {
                dbret = QueryATEJO(pncond, startdate, enddate);
            }
            else
            {
                dbret = QueryMESJO(pncond, startdate, enddate);
            }

            var sndict = new Dictionary<string, bool>();
            var workflowdict = new Dictionary<string, int>();
            var jodict = new Dictionary<string, JOItem>();
            var joholddict = new Dictionary<string, List<string>>();

            var sntimedict = new Dictionary<string, string>();
            if (string.Compare(defdb, "ATE", true) == 0)
            {
                foreach (var line in dbret)
                {
                    try
                    {
                        var sn = Convert.ToString(line[0]);
                        var time = Convert.ToString(line[6]);
                        if (!sntimedict.ContainsKey(sn))
                        { sntimedict.Add(sn, time); }
                    }
                    catch (Exception ex) { }
                }
            }

            foreach (var line in dbret)
            {
                try
                {
                    if (string.Compare(defdb, "ATE", true) == 0)
                    {
                        var asn = Convert.ToString(line[0]);
                        var time = Convert.ToString(line[6]);
                        if (string.Compare(sntimedict[asn],time) != 0)
                        { continue; }

                    }

                    var sn = Convert.ToString(line[0]);
                    if (!sndict.ContainsKey(sn))
                    {
                        sndict.Add(sn, true);
                    
                        var jo = Convert.ToString(line[2]);
                        var workflow = Convert.ToString(line[5]).ToUpper();
                        var snstatus = Convert.ToString(line[8]);

                        if (string.Compare(defdb, "ATE", true) == 0)
                        {}
                        else
                        {
                            var snholdstatus = Convert.ToString(line[9]);
                            if (!string.IsNullOrEmpty(snholdstatus))
                            { snstatus = "HOLD"; }
                        }

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

                            var releasedate = "";
                            var joactivedate = "";

                            if (string.Compare(defdb, "ATE", true) == 0)
                            {
                                var spdatetime = Convert.ToString(line[4]);
                                releasedate = spdatetime.Substring(0, 4) + "-" + spdatetime.Substring(4, 2) + "-" + spdatetime.Substring(6, 2);
                                spdatetime  = Convert.ToString(line[6]);
                                joactivedate = spdatetime.Substring(0, 4) + "-" + spdatetime.Substring(4, 2) + "-" + spdatetime.Substring(6, 2);
                            }
                            else
                            {
                                releasedate = Convert.ToDateTime(line[4]).ToString("yyyy-MM-dd");
                                joactivedate = Convert.ToDateTime(line[6]).ToString("yyyy-MM-dd");
                            }

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
            ret.Add(GetChartData(title,workflowdict,defdb,ctrl));
            return ret;
        }

        public static object QueryJOProcess(string jo, Controller ctrl)
        {
            var sql = @"SELECT distinct  a.MFR_SN,a.MFR_PN,b.JOB_ID,j.oracle_qty,j.time,b.state,ds.maxtime,
                        'Active' as JOStatus,
                        case when b.status = 'END_ROUTE' then 'CLOSED' when b.state = 'TROUBLESHOOT' then 'HOLD' else 'ACTIVE' end SNStatus
                        FROM PARTS a   
                        INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX  
                        INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID 
                        INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID   
                        INNER JOIN JOBS j ON j.job_id = b.job_id
                        inner join (select ROUTE_ID,max(start_time) as maxtime from datasets group by ROUTE_ID) ds on ds.ROUTE_ID = b.route_id
                        WHERE b.JOB_ID = '<jo>' AND b.state <> 'GOLDEN' order by a.mfr_sn,ds.maxtime desc";

            sql = sql.Replace("<jo>", jo);
            var dbret = DBUtility.ExeATESqlWithRes(sql);

            var sndict = new Dictionary<string, bool>();
            var workflowdict = new Dictionary<string, int>();

            foreach (var line in dbret)
            {
                try
                {
                    var sn = Convert.ToString(line[0]);
                    if (!sndict.ContainsKey(sn))
                    {
                        sndict.Add(sn, true);
                        var workflow = Convert.ToString(line[5]).ToUpper();
                        if (workflowdict.ContainsKey(workflow))
                        { workflowdict[workflow] += 1; }
                        else
                        { workflowdict.Add(workflow, 1); }
                    }
                }
                catch (Exception ex) { }
            }

            return GetChartData(jo, workflowdict,"ATE",ctrl);
        }


    }
}