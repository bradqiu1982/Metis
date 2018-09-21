using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class ATETestData
    {

        public ATETestData()
        {
            Init();
        }

        public ATETestData(string did, string sn, string starttime, string dsname, string station, string mdtype, string pn, string status, string endtime, string routeid, string modelid, string speedrate)
        {
            Init();

            DataID = did;
            ModuleSerialNum = sn;
            TestTimeStamp = DateTime.Parse(starttime);
            WhichTest = dsname;
            TestStation = station;
            ModuleType = mdtype;
            PN = pn;
            ErrAbbr = status;
            EndTime = DateTime.Parse(endtime);
            RouteId = routeid;
            PNDesc = modelid;
            SpeedRate = speedrate;
        }

        public void Init()
        {
            DataID = "";
            ModuleSerialNum = "";
            WhichTest = "";
            TestStation = "";
            ModuleType = "";
            PN = "";
            ErrAbbr = "";
            RouteId = "";
            PNDesc = "";
            SpeedRate = "";
            SpendTime = "";
            ProductFamily = "";
        }

        private static List<ModuleTestData> RetrieveValidATETestData(List<List<object>> dbret,Dictionary<string,string> yddict)
        {
            var retdata = new List<ModuleTestData>();

            var validatedata = new List<ATETestData>();
            var temppjdatalist = new List<ATETestData>();
            var currentroutid = "";
            var currentstation = "";
            var currentsn = "";
            
            foreach (var item in dbret)
            {
                try
                {
                    var did = Convert.ToString(item[0]); 
                    var sn = Convert.ToString(item[1]);

                    var spdatetime = Convert.ToString(item[2]);
                    var starttime = spdatetime.Substring(0, 4) + "-" + spdatetime.Substring(4, 2) + "-" + spdatetime.Substring(6, 2) + " "
                                          + spdatetime.Substring(8, 2) + ":" + spdatetime.Substring(10, 2) + ":" + spdatetime.Substring(12, 2);
                    var dsname = Convert.ToString(item[3]);
                    var station = Convert.ToString(item[4]);
                    var family = Convert.ToString(item[5]);
                    var pn = Convert.ToString(item[6]);
                    var status = Convert.ToString(item[7]);
                    spdatetime = Convert.ToString(item[8]);
                    var endtime = spdatetime.Substring(0, 4) + "-" + spdatetime.Substring(4, 2) + "-" + spdatetime.Substring(6, 2) + " "
                                          + spdatetime.Substring(8, 2) + ":" + spdatetime.Substring(10, 2) + ":" + spdatetime.Substring(12, 2);
                    var routeid = Convert.ToString(item[9]);
                    var modelid = Convert.ToString(item[10]);
                    var speedrate = Convert.ToString(item[11]);

                    var setupflag = false;
                    if (dsname.ToUpper().Contains("_SETUP"))
                    { setupflag = true; }

                    if (string.Compare(currentroutid, routeid) != 0
                        || string.Compare(currentstation, station) != 0
                        || string.Compare(currentsn, sn) != 0
                        || setupflag)
                    {
                        currentroutid = routeid;
                        currentstation = station;
                        currentsn = sn;

                        if (temppjdatalist.Count > 0)
                        {
                            var err = "";
                            var errid = "";
                            foreach (var line in temppjdatalist)
                            {
                                if (string.Compare(line.ErrAbbr, "PASS", true) != 0
                                    && string.Compare(line.ErrAbbr, "INFO", true) != 0)
                                {
                                    err = line.WhichTest.ToUpper();
                                    errid = line.DataID;
                                    break;
                                }
                            }//end foreach

                            var sec = (double)(temppjdatalist[temppjdatalist.Count - 1].EndTime - temppjdatalist[0].EndTime).TotalSeconds;
                            if (sec > 0 && sec < 20 * 3600)
                            {
                                var wt = temppjdatalist[0].WhichTest.ToUpper().Replace("_SETUP", "");
                                var testdata = new ATETestData();
                                if (string.IsNullOrEmpty(errid))
                                {
                                    testdata = new ATETestData(temppjdatalist[0].DataID, temppjdatalist[0].ModuleSerialNum , temppjdatalist[0].TestTimeStamp.ToString(), wt
                                        , temppjdatalist[0].TestStation, temppjdatalist[0].ModuleType, temppjdatalist[0].PN, "PASS", temppjdatalist[0].TestTimeStamp.ToString()
                                        , "", temppjdatalist[0].PNDesc, temppjdatalist[0].SpeedRate);
                                    testdata.SpendTime = sec.ToString();
                                }
                                else
                                {
                                    testdata  = new ATETestData(errid, temppjdatalist[0].ModuleSerialNum, temppjdatalist[0].TestTimeStamp.ToString(), wt
                                        , temppjdatalist[0].TestStation, temppjdatalist[0].ModuleType, temppjdatalist[0].PN, err, temppjdatalist[0].TestTimeStamp.ToString()
                                        , "", temppjdatalist[0].PNDesc, temppjdatalist[0].SpeedRate);
                                    testdata.SpendTime = sec.ToString();
                                }

                                if (yddict.ContainsKey(testdata.ModuleType.ToUpper() + "_GEN"))
                                {
                                    var pndeslist = yddict[testdata.ModuleType.ToUpper() + "_GEN"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                    foreach (var pnd in pndeslist)
                                    {
                                        if (testdata.PNDesc.ToUpper().Contains(pnd.ToUpper()))
                                        {
                                            testdata.ProductFamily = yddict[pnd];
                                            break;
                                        }
                                    }

                                    if (string.IsNullOrEmpty(testdata.ProductFamily))
                                    {
                                        testdata.ProductFamily = testdata.ModuleType;
                                    }
                                }
                                else
                                {
                                    testdata.ProductFamily = testdata.ModuleType;
                                }

                                validatedata.Add(testdata);
                            }
                        }//end if pjdatalist.Count > 0

                        temppjdatalist.Clear();

                        var tempdata = new ATETestData(did, sn, starttime, dsname, station, family, pn, status, endtime, routeid,  modelid, speedrate);
                        temppjdatalist.Add(tempdata);
                    }
                    else
                    {
                        var tempdata = new ATETestData(did, sn, starttime, dsname, station, family, pn, status, endtime, routeid, modelid, speedrate);
                        temppjdatalist.Add(tempdata);
                    }

                }
                catch (Exception ex)
                { }
            }

            foreach (var item in validatedata)
            {
                var tempvm = new ModuleTestData(item.DataID, item.ModuleSerialNum, item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss")
                                                , item.WhichTest, item.ErrAbbr, item.TestStation, item.ProductFamily, item.PN, item.PNDesc, item.ModuleType, item.SpeedRate,item.SpendTime, "ROUTE_DATA");
                retdata.Add(tempvm);
            }

            return retdata;

        }

        public static List<ModuleTestData> LoadATETestData(string familycond, DateTime startdate, DateTime enddate, Controller ctrl)
        {
            var ydcfg = CfgUtility.LoadYieldConfig(ctrl);
            var sql = @"SELECT d.dataset_id,a.MFR_SN,d.start_time,d.DATASET_NAME,d.STATION,c.FAMILY,a.MFR_PN,d.STATUS,d.END_TIME,d.ROUTE_ID,c.model_id,c.product_group FROM PARTS a   
                    INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX  
                    INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID 
                    INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID   
                    WHERE c.FAMILY in <FAMILYCOND> and d.start_time >= '<STARTDATE>' and d.start_time < '<ENDDATE>' AND b.state <> 'GOLDEN' ORDER BY a.MFR_SN,d.start_time ASC";
            sql = sql.Replace("<FAMILYCOND>",familycond).Replace("<STARTDATE>", startdate.ToString("yyyyMMddHHmmss")).Replace("<ENDDATE>", enddate.ToString("yyyyMMddHHmmss"));
            var dbret = DBUtility.ExeATESqlWithRes(sql);
            return RetrieveValidATETestData(dbret,ydcfg);
        }



        public string DataID { set; get; }
        public string ModuleSerialNum { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string WhichTest { set; get; }
        public string TestStation { set; get; }
        public string ModuleType { set; get; }
        public string PN { set; get; }
        public string ErrAbbr { set; get; }
        public DateTime EndTime { set; get; }
        public string RouteId { set; get; }
        public string PNDesc { set; get; }
        public string SpeedRate { set; get; }

        public string ProductFamily { set; get; }
        public string SpendTime { set; get; }

    }
}