using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Metis
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private bool IsDebug()
        {
            bool debugging = false;
#if DEBUG
            debugging = true;
#else
            debugging = false;
#endif
            return debugging;
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            try
            {
                if (!IsDebug())
                {
                    using (Process myprocess = new Process())
                    {
                        myprocess.StartInfo.FileName = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"Scripts\MetisHeartBeat.exe").Replace("\\", "/");
                        //System.Windows.MessageBox.Show(myprocess.StartInfo.FileName);
                        //myprocess.StartInfo.CreateNoWindow = true;
                        myprocess.Start();
                    }
                }
            }
            catch (Exception ex)
            { }
        }
    }
}
