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
                ExternalDataCollector.LoadScrapData(this);
            }
            catch (Exception ex) { }

            return View();
        }
    }
}