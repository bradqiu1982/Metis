using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prism.Models;
using System.Web.Routing;
using System.Net;
using System.IO;

namespace Prism.Controllers
{
    public class DataAnalyzeController : Controller
    {

        public ActionResult HPUTrend()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 7, "HPUTrend"))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.defaultserial = "";
            var syscfg = CfgUtility.GetSysConfig(this);
            if (syscfg.ContainsKey("HPUTRENDSERIAL"))
            { ViewBag.defaultserial = syscfg["HPUTRENDSERIAL"]; }
            return View();
        }

        private List<double> GetHPUDataBySerial(string serial,List<string> xaxis,List<HPUMainData> srcdata)
        {
            var yieldhpulist = new List<double>();
            if (serial.Contains("-FG") || serial.Contains("- FG"))
            {
                foreach (var q in xaxis)
                {
                    var temphpu = 0.0;
                    foreach (var data in srcdata)
                    {
                        if (string.Compare(data.Serial.Replace("-SFG", "").Replace("- SFG", "").Replace("-FG", "").Replace("- FG", ""), serial.Replace("-FG", "").Replace("- FG", "")) == 0
                            && string.Compare(data.Quarter, q) == 0
                            && (data.Serial.Contains("-SFG") || data.Serial.Contains("- SFG")))
                        {
                            if (data.Serial.ToUpper().Contains("WIRE") && !data.Serial.ToUpper().Contains("WIRELESS"))
                            {
                                temphpu += 2.0 * Convert.ToDouble(data.YieldHPU);
                            }
                            else
                            {
                                temphpu +=  Convert.ToDouble(data.YieldHPU);
                            }
                        }
                        if (string.Compare(data.Serial.Replace("-SFG", "").Replace("- SFG", "").Replace("-FG", "").Replace("- FG", ""), serial.Replace("-FG", "").Replace("- FG", "")) == 0
                            && string.Compare(data.Quarter, q) == 0
                            && (data.Serial.Contains("-FG") || data.Serial.Contains("- FG")))
                        {
                            temphpu += Convert.ToDouble(data.YieldHPU);
                        }
                    }
                    yieldhpulist.Add(Math.Round(temphpu,4));
                }
            }
            else
            {
                foreach (var q in xaxis)
                {
                    foreach (var data in srcdata)
                    {
                        if (string.Compare(data.Serial, serial) == 0 && string.Compare(data.Quarter, q) == 0)
                        {
                            yieldhpulist.Add(Math.Round(Convert.ToDouble(data.YieldHPU),4));
                        }
                    }
                }
            }

            return yieldhpulist;
        }

        private List<double> GetHPUReduction(List<double> yieldhpulist)
        {
            var hpureduction = new List<double>();
            hpureduction.Add(0.0);
            for (var idx = 1; idx < yieldhpulist.Count; idx++)
            {
                hpureduction.Add(Math.Round((yieldhpulist[idx - 1] - yieldhpulist[idx]) / yieldhpulist[idx - 1] * 100.0, 2));
            }
            return hpureduction;
        }

        public JsonResult HPUTrendData()
        {
            var serial = Request.Form["serial"];
            var srcdata = HPUMainData.RetrieveHPUDataBySerial(serial);
            var syscfg = CfgUtility.GetSysConfig(this);

            var hpuarray = new List<object>();
            try {
                if (srcdata.Count > 0)
                {
                    var seriallist = new List<string>();
                    var serialdict = new Dictionary<string, bool>();
                    foreach (var item in srcdata)
                    {
                        if (!item.Serial.Contains("-SFG") && !item.Serial.Contains("- SFG"))
                        {
                            if (!serialdict.ContainsKey(item.Serial))
                            {
                                serialdict.Add(item.Serial,true);
                                seriallist.Add(item.Serial);
                            }
                        }
                    }//get serials

                    foreach (var sitem in seriallist)
                    {
                        var xaxis = new List<string>();
                        foreach (var data in srcdata)
                        {
                            if (string.Compare(data.Serial, sitem) == 0)
                            {
                                xaxis.Add(data.Quarter);
                            }
                        }//get quarter x list

                        if (xaxis.Count == 1)
                        { continue; }

                        var yieldhpulist = GetHPUDataBySerial(sitem, xaxis, srcdata);
                        var hpureduction = GetHPUReduction(yieldhpulist);
                        

                        var maxhpu = 0.0;
                        foreach (var yhp in yieldhpulist)
                        { if (yhp > maxhpu) { maxhpu = yhp; } }

                        var maxhpureduction = 5.0;
                        foreach (var hrd in hpureduction)
                        { if (hrd > maxhpureduction) { maxhpureduction = hrd; } }
                        var minhpureduction = 0.0;
                        foreach (var hrd in hpureduction)
                        { if (hrd < minhpureduction) { minhpureduction = hrd; } }

                        var defaultguideline = 5.0;
                        if (syscfg.ContainsKey(sitem + "-GUIDELINE"))
                        {
                            defaultguideline = Convert.ToDouble(syscfg[sitem + "-GUIDELINE"]);
                        }

                        var hpuguideline = new { name = "HPU Reduction Guideline", color = "#C9302C", data = defaultguideline, style = "dash" };

                        var columncolors = new List<string>();
                        foreach (var hrd in hpureduction)
                        {
                            if (hrd < defaultguideline)
                            { columncolors.Add("#cc044d"); }
                            else
                            { columncolors.Add("#12cc92"); }
                        }

                        var title = sitem.Replace("-FG", "").Replace("- FG", "") + " HPU";

                        var oneobj = new
                        {
                            id = title.Replace(" ", "_") + "_line",
                            title = title,
                            xAxis = new { data = xaxis },
                            maxhpu = maxhpu,
                            maxhpureduction = maxhpureduction,
                            minhpureduction = minhpureduction,
                            hpuguideline = hpuguideline,
                            columncolors = columncolors,
                            yieldhpu = new { name = "Yield HPU", data = yieldhpulist },
                            hpureduction = new { name = "HPU Reduction", data = hpureduction },
                            url = "/DataAnalyze/SerialHPU?defaultserial=" + sitem.Split(new string[] { "-FG", "- FG" },StringSplitOptions.RemoveEmptyEntries)[0]
                        };

                        hpuarray.Add(oneobj);

                    }//get chart data from each serial

                }
            } catch (Exception ex) { }




            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                hpuarray = hpuarray
            };
            return ret;
        }


        public ActionResult DepartmentHPU()
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 7, "DepartmentHPU"))
            {
                return RedirectToAction("Index", "Main");
            }

            var productlines = HPUMainData.GetAllProductLines();
            ViewBag.productlines = CreateSelectList(productlines, "");

            var quarterlist = HPUMainData.GetAllQuarters();
            quarterlist.Insert(0, FinanceQuarter.CURRENTQx);
            ViewBag.quarterlist = CreateSelectList(quarterlist, "");

            return View();
        }

        public JsonResult DepartmentHPUData()
        {
            var productline = Request.Form["pdline"];
            var fquarter = Request.Form["quarter"];

            if (string.Compare(fquarter, FinanceQuarter.CURRENTQx) == 0)
            {
                var now = DateTime.Now;
                var year = ExternalDataCollector.GetFYearByTime(now);
                var quarter = ExternalDataCollector.GetFQuarterByTime(now);
                fquarter = year + " " + quarter;
            }

            var data = HPUMainData.RetrieveHPUData(productline,fquarter);
            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                data = data
            };
            return ret;
        }


        private double Convert2Double(string val)
        {
            try
            {
                return Convert.ToDouble(val);
            }
            catch (Exception ex) {
                return 0.0;
            }
        }


        public ActionResult SerialHPU(string defaultserial)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 7, "SerialHPU"))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.defaultserial = "";
            if (!string.IsNullOrEmpty(defaultserial))
            {
                ViewBag.defaultserial = defaultserial;
            }
            return View();
        }

        public JsonResult GetAllSerial()
        {
            var slist = HPUMainData.RetrieveAllSerial();
            var ret = new JsonResult();
            ret.Data = new
            {
                data = slist
            };
            return ret;
        }

        public JsonResult SerialHPUData()
        {
            var serial = Request.Form["serial"];
            var data = HPUMainData.RetrieveHPUDataBySerial(serial);
            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                data = data
            };
            return ret;
        }

        public ActionResult PNHPU(string PNLink)
        {
            if (!MachineUserMap.IsLxEmployee(Request.UserHostName, null, 7, "PNHPU"))
            {
                return RedirectToAction("Index", "Main");
            }

            ViewBag.pnlink = "";
            if (!string.IsNullOrEmpty(PNLink))
            { ViewBag.pnlink = PNLink; }
            return View();
        }

        public JsonResult PNHPUPNLinkList()
        {
            var pnlist = PNHPUData.RetrievePNLinkList();
            var ret = new JsonResult();
            ret.Data = new {
                data = pnlist
            };
            return ret;
        }

        public JsonResult GetPNHPUData()
        {
            var pnlink = Request.Form["pnlink"];
            var data = PNHPUData.RetrieveHPUData(pnlink);
            var title = new List<string>();
            if (data.Count > 0)
            {
                title.AddRange(data[0]);
                data.RemoveAt(0);
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                title = title,
                data = data
            };
            return ret;
        }

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p, true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        public JsonResult RetrieveReport()
        {
            var reportid = Request.Form["reportid"];
            var reporttype = Request.Form["reporttype"];

            var wreportlist = PrismComment.RetrieveComment(reportid);
            if (wreportlist.Count == 0)
            {
                PrismComment.StoreComment(reportid, "TO BE EDIT", "System", reporttype);

                var report = new
                {
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = "System",
                    content = "TO BE EDIT"
                };

                var ret = new JsonResult();
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
            else
            {
                var report = new
                {
                    time = wreportlist[0].CommentDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = wreportlist[0].Reporter.ToUpper(),
                    content = wreportlist[0].Comment
                };
                var ret = new JsonResult();
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
        }

        public ActionResult ModifyReport(string reportid)
        {
            ViewBag.username = MachineUserMap.EmployeeName(Request.UserHostName);

            var wreportlist = PrismComment.RetrieveComment(reportid);
            if (wreportlist.Count > 0)
            {
                return View(wreportlist[0]);
            }
            return View();
        }

        [HttpPost, ActionName("ModifyWaferReport")]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyWaferReportPost()
        {
            var reportid = Request.Form["ReportId"];
            ViewBag.username = MachineUserMap.EmployeeName(Request.UserHostName);

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                PrismComment.UpdateComment(reportid, comment, ViewBag.username);
            }
            else
            {
                PrismComment.UpdateComment(reportid, "<p>To Be Edit</p>" , ViewBag.username);
            }

            return RedirectToAction("Index", "Main");
        }


        public JsonResult UploadWebmVideoData()
        {
            foreach (string fl in Request.Files)
            {
                if (fl != null && Request.Files[fl].ContentLength > 0)
                {
                    string datestring = DateTime.Now.ToString("yyyyMMdd");
                    string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                    if (!Directory.Exists(imgdir))
                    {
                        Directory.CreateDirectory(imgdir);
                    }

                    var fn = "V" + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".webm";
                    var onlyname = Path.GetFileNameWithoutExtension(fn);
                    var srcvfile = imgdir + fn;
                    Request.Files[fl].SaveAs(srcvfile);

                    //var imgname = onlyname + ".jpg";
                    //var imgpath = imgdir + imgname;
                    //var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                    //ffMpeg.GetVideoThumbnail(srcvfile, imgpath);

                    //var oggname = onlyname + ".ogg";
                    //var oggpath = imgdir + oggname;
                    //var ffMpeg1 = new NReco.VideoConverter.FFMpegConverter();
                    //ffMpeg1.ConvertMedia(srcvfile, oggpath, NReco.VideoConverter.Format.ogg);

                    var mp4name = onlyname + ".mp4";
                    var mp4path = imgdir + mp4name;
                    var ffMpeg2 = new NReco.VideoConverter.FFMpegConverter();

                    var setting = new NReco.VideoConverter.ConvertSettings();
                    setting.VideoFrameRate = 30;
                    setting.AudioSampleRate = 44100;

                    ffMpeg2.ConvertMedia(srcvfile, NReco.VideoConverter.Format.webm, mp4path, NReco.VideoConverter.Format.mp4, setting);

                    try { System.IO.File.Delete(srcvfile); } catch (Exception ex) { }

                    var url = "/userfiles/docs/" + datestring + "/" + mp4name;
                    var videohtml = "<p><video width='640' height='480' controls src='" + url + "' type='video/mp4'>"
                        + "Your browser does not support the video tag. </video></p>";

                    var ret1 = new JsonResult();
                    ret1.Data = new { data = videohtml };
                    return ret1;
                }
            }
            var ret = new JsonResult();
            ret.Data = new { data = "<p></p>" };
            return ret;
        }

    }
}