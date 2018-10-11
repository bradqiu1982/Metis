using Prism.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Controllers
{
    public class YieldController : Controller
    {
        public ActionResult DepartmentYield()
        {
            return View();
        }

        public JsonResult DepartmentYieldData()
        {
            var pdyieldlist = YieldVM.RetrieveAllYield(this);
            var titlelist = new List<object>();
            titlelist.Add("department");
            foreach (var y in pdyieldlist[0].FinalYieldList)
            {
                titlelist.Add(y.Quarter);
            }

            var contentlist = new List<object>();
            foreach (var pdy in pdyieldlist)
            {
                var linelist = new List<object>();
                linelist.Add(pdy.ProductFamily);
                var idx = 0;
                foreach (var fy in pdy.FinalYieldList)
                {
                    linelist.Add("INPUT:"+pdy.FirstYieldList[idx].MaxInput+"<br>FIRSTYIELD:"+ pdy.FirstYieldList[idx].YieldVal + "<br>FINALYIELD:" + fy.YieldVal);
                    idx += 1;
                }
                contentlist.Add(linelist);
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                tabletitle = titlelist,
                tablecontent = contentlist
            };
            return ret;
        }

        public ActionResult ProductYield()
        {
            return View();
        }
    }
}