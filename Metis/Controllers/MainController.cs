using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Metis.Models;

namespace Metis.Controllers
{
    public class MainController : Controller
    {
        // GET: Main
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HeartBeat()
        {

            try
            {
                ExternalDataCollector.LoadPNPlannerData(this);
            }
            catch (Exception ex) { }

            try
            {
                ExternalDataCollector.LoadScrapData(this);
            }
            catch (Exception ex) { }

            try
            {
                ExternalDataCollector.LoadIEScrapBuget(this);
            }
            catch (Exception ex) { }

            try
            {
                if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                {
                    CostCentScrapWarning.Waring(this);
                }
            }
            catch (Exception ex) { }
            
            return View();
        }
    }
}