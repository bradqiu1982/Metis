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
                    list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));
                }
            }
            catch (Exception ex) { }
            return list;
        }
    }
}