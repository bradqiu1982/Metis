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

        public static Dictionary<string, string> LoadLineCardIgnoreConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/LincardIgnoreTest.cfg"));
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
                        var key = kvpair[0].Trim();
                        key = System.Text.RegularExpressions.Regex.Replace(key, @"\d", "").Replace("_", "").ToUpper();
                        var val = kvpair[1].ToUpper().Trim();
                        if (!ret.ContainsKey(key))
                        {
                            ret.Add(key,val);
                        }
                    }
                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> LoadTunableIgnoreConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/TunableIgnoreTest.cfg"));
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
                        var key = kvpair[0].Trim();
                        key = System.Text.RegularExpressions.Regex.Replace(key, @"\d", "").Replace("_", "")
                            .Replace("-", "").Replace("[", "").Replace("]", "").Replace("%", "").ToUpper();
                        var val = kvpair[1].ToUpper().Trim();
                        if (!ret.ContainsKey(key))
                        {
                            ret.Add(key, val);
                        }
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

        //public static Dictionary<string, string> GetShipCustConfig(Controller ctrl, string producttype)
        //{
        //    var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/ShipCustomer.cfg"));
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
        //            if (kvpair[0].Contains(producttype + "_"))
        //            {
        //                var key = kvpair[0].Replace(producttype + "_", "").ToUpper();
        //                var val = kvpair[1].Trim().ToUpper();
        //                if (!ret.ContainsKey(key))
        //                { ret.Add(key, val); }
        //            }
        //        }//end if
        //    }//end foreach
        //    return ret;
        //}

        public static Dictionary<string, string> GetAllCustConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/ShipCustomer.cfg"));
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
                    var producttype = kvpair[0].Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    var key = kvpair[0].Replace(producttype + "_", "").ToUpper();
                    var val = kvpair[1].Trim().ToUpper();
                    if (!ret.ContainsKey(key))
                    { ret.Add(key, val); }

                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, double> GetUSDRate(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/USDRate.cfg"));
            var ret = new Dictionary<string, double>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    try
                    {
                        var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                        var key = kvpair[0];
                        var val = Convert.ToDouble(kvpair[1]);
                        if (!ret.ContainsKey(key))
                        { ret.Add(key, val); }
                    }
                    catch (Exception ex) { }


                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> LoadSearchConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/SearchData.cfg"));
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

        public static Dictionary<string, string> LoadNamePNConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/NamePNMap.cfg"));
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

        public static Dictionary<string, string> LoadNamePFConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/NamePFMap.cfg"));
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