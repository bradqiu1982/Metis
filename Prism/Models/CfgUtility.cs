using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class CfgUtility
    {
        public static Dictionary<string, string> GetSysConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/PrismCfg.txt"));
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!ret.ContainsKey(kvpair[0].Trim()) && kvpair.Length > 1)
                    {
                        ret.Add(kvpair[0].Trim(), kvpair[1].Trim());
                    }
                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> LoadYieldConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/YieldData.cfg"));
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!ret.ContainsKey(kvpair[0].Trim()) && kvpair.Length > 1)
                    {
                        ret.Add(kvpair[0].Trim(), kvpair[1].ToUpper().Trim());
                    }
                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> GetStandardPJList(Controller ctrl)
        {
            var ret = new Dictionary<string, string>();
            var sql = "select distinct ProjectName from [NebulaTrace].[dbo].[ProjectVM]";
            var dbret = DBUtility.ExeNebulaSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var pjname = Convert.ToString(line[0]);
                    if (pjname.Contains("/"))
                    {
                        var idx = pjname.IndexOf("/") + 1;
                        pjname = pjname.Substring(idx);
                    }

                    if (!ret.ContainsKey(pjname))
                    {
                        ret.Add(pjname, pjname);
                    }

                }
                catch (Exception ex) { }
            }

            return ret;
        }

        //public static Dictionary<string, string> GetNPIMachine(Controller ctrl)
        //{
        //    var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/npidepartmentmachine.cfg"));
        //    var ret = new Dictionary<string, string>();
        //    foreach (var line in lines)
        //    {
        //        if (line.Contains("##"))
        //        {
        //            continue;
        //        }

        //        if (line.Contains(":::"))
        //        {
        //            var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
        //            if (!ret.ContainsKey(kvpair[0].Trim()))
        //            {
        //                ret.Add(kvpair[0].Trim().ToUpper(), kvpair[1].Trim());
        //            }
        //        }//end if
        //    }//end foreach
        //    return ret;
        //}

    }
}