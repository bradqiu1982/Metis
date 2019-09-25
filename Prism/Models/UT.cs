using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class UT
    {
        public static List<List<T>> SplitList<T>(List<T> me, int size = 5000)
        {
            var list = new List<List<T>>();
            try
            {
                for (int i = 0; i < me.Count; i += size)
                {
                    var sz = Math.Min(size, me.Count - i);
                    if (sz <= 0)
                    { break; }

                    list.Add(me.GetRange(i,sz));
                }
            }
            catch (Exception ex) { }
            return list;
        }

        public static string O2S(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToString(obj);
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public static DateTime O2T(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDateTime(obj);
                }
                catch (Exception ex) { return DateTime.Parse("1982-05-06 10:00:00"); }
            }
            return DateTime.Parse("1982-05-06 10:00:00");
        }

        public static string T2S(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public static string Db2S(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDouble(obj).ToString();
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public static int O2I(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToInt32(obj);
                }
                catch (Exception ex) { return 0; }
            }
            return 0;
        }

        public static double O2D(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToDouble(obj);
                }
                catch (Exception ex) { return 0.0; }
            }
            return 0.0;
        }

    }
}