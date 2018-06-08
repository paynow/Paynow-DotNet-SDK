using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Webdev.Core;
using Webdev.Payments;

namespace Webdev.Helpers
{
    public static class Extensions
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in col.AllKeys) dict.Add(k, col[k]);

            return dict;
        }

        public static string GetString(this MobileMoneyMethod method)
        {
            switch (method)
            {
                case MobileMoneyMethod.Ecocash:
                    return Constants.MobileMoneyMethodEcocash;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }
}