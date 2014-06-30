using System;
using System.Collections.Generic;
using System.Linq;

namespace Caliburn.Micro.Autofac.Internal
{
    public static class BuildUriExtensions
    {
        public static string BuildQueryString(this IDictionary<string, string> qry)
        {
            if (qry == null) return string.Empty;
            if (qry.Count < 1)
                return string.Empty;
            var str = qry.Aggregate("?", (current, pair) => current + pair.Key + "=" + Uri.EscapeDataString(pair.Value) + "&");
            return str.Remove(str.Length - 1);
        }
    }
}