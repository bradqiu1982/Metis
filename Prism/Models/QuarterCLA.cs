using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class QuarterCLA
    {
        public static List<DateTime> RetrieveDateFromQuarter(string quarter)
        {
            var ret = new List<DateTime>();

            var splitstr = quarter.Split(new string[] { " ", "-" }, StringSplitOptions.RemoveEmptyEntries);
            var year = Convert.ToInt32(splitstr[0]);
            var q = splitstr[1];

            //if (year == 2020 && q.Contains("Q2"))
            //{
            //    year = year - 1;
            //    ret.Add(DateTime.Parse(year.ToString() + "-08-01 00:00:00"));
            //    ret.Add(DateTime.Parse(year.ToString() + "-12-31 23:59:59"));
            //}
            //else if ((year < 2020) || (year == 2020 && q.Contains("Q1")))
            //{
            //    if (string.Compare(q, "Q1", true) == 0)
            //    {
            //        year = year - 1;
            //        ret.Add(DateTime.Parse(year.ToString() + "-05-01 00:00:00"));
            //        ret.Add(DateTime.Parse(year.ToString() + "-07-31 23:59:59"));
            //    }
            //    else if (string.Compare(q, "Q2", true) == 0)
            //    {
            //        year = year - 1;
            //        ret.Add(DateTime.Parse(year.ToString() + "-08-01 00:00:00"));
            //        ret.Add(DateTime.Parse(year.ToString() + "-10-31 23:59:59"));
            //    }
            //    else if (string.Compare(q, "Q4", true) == 0)
            //    {
            //        ret.Add(DateTime.Parse(year.ToString() + "-02-01 00:00:00"));
            //        ret.Add(DateTime.Parse(year.ToString() + "-04-30 23:59:59"));
            //    }
            //    else
            //    {
            //        ret.Add(DateTime.Parse((year - 1).ToString() + "-11-01 00:00:00"));
            //        ret.Add(DateTime.Parse(year.ToString() + "-01-31 23:59:59"));
            //    }
            //}
            //else
            //{
                if (string.Compare(q, "Q1", true) == 0)
                {
                    year = year - 1;
                    ret.Add(DateTime.Parse(year.ToString() + "-07-01 00:00:00"));
                    ret.Add(DateTime.Parse(year.ToString() + "-09-30 23:59:59"));
                }
                else if (string.Compare(q, "Q2", true) == 0)
                {
                    year = year - 1;
                    ret.Add(DateTime.Parse(year.ToString() + "-10-01 00:00:00"));
                    ret.Add(DateTime.Parse(year.ToString() + "-12-31 23:59:59"));
                }
                else if (string.Compare(q, "Q3", true) == 0)
                {
                    ret.Add(DateTime.Parse(year.ToString() + "-01-01 00:00:00"));
                    ret.Add(DateTime.Parse(year.ToString() + "-03-31 23:59:59"));
                }
                else
                {
                    ret.Add(DateTime.Parse(year.ToString() + "-04-01 00:00:00"));
                    ret.Add(DateTime.Parse(year.ToString() + "-06-30 23:59:59"));
                }
            //}

            return ret;
        }

        public static string RetrieveQuarterFromDate_(DateTime date)
        {
            var year = Convert.ToInt32(date.ToString("yyyy"));
            var month = Convert.ToInt32(date.ToString("MM"));
            if (month >= 5 && month <= 7)
            {
                return (year + 1).ToString() + " " + "Q1";
            }
            else if (month >= 8 && month <= 10)
            {
                return (year + 1).ToString() + " " + "Q2";
            }
            else if (month >= 2 && month <= 4)
            {
                return year.ToString() + " " + "Q4";
            }
            else if (month >= 11 && month <= 12)
            {
                return (year + 1).ToString() + " " + "Q3";
            }
            else
            {
                return year.ToString() + " " + "Q3";
            }
        }

        public static double QuarterSec
        {
            get { return 13.0 * 6.5 * 20.0 * 3600.0; }
        }

        public static double QuarterSecMax
        {
            get { return 13.0 * 7.0 * 24.0 * 3600.0; }
        }

        public static string RetrieveQuarterFromDate(DateTime date)
        {
            //if (date >= DateTime.Parse("2019-07-01 00:00:00"))
            //{
                var year = Convert.ToInt32(date.ToString("yyyy"));
                var month = Convert.ToInt32(date.ToString("MM"));
                if (month >= 7 && month <= 9)
                {
                    return (year + 1).ToString() + " " + "Q1";
                }
                else if (month >= 10 && month <= 12)
                {
                    return (year + 1).ToString() + " " + "Q2";
                }
                else if (month >= 1 && month <= 3)
                {
                    return year.ToString() + " " + "Q3";
                }
                else
                {
                    return year.ToString() + " " + "Q4";
                }
            //}
            //else
            //{ return RetrieveQuarterFromDate_(date); }

        }

        public static List<string> GetQuerterFrom19Q3()
        {
            var cq = RetrieveQuarterFromDate(DateTime.Now);
            var cqstr = cq.Substring(5, 2)+ "FY" +  cq.Substring(2, 2);

            var qlist = new List<string>();
            qlist.Add("Q1");
            qlist.Add("Q2");
            qlist.Add("Q3");
            qlist.Add("Q4");

            var ret = new List<string>();
            ret.Add("Q3FY19");
            ret.Add("Q4FY19");

            for (var idx = 20; idx < 50; idx++)
            {
                foreach (var q in qlist)
                {
                    var tmpq = q + "FY" + idx;
                    if (tmpq.Contains(cqstr))
                    {
                        ret.Add(tmpq);
                        return ret;
                    }
                    else
                    {
                        if (!ret.Contains(tmpq))
                        { ret.Add(tmpq); }
                    }
                }//end foread
            }//end for
            return ret;
        }


    }
}