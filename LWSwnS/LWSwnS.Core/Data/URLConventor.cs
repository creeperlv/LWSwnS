using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace LWSwnS.Core.Data
{
    public class URLConventor
    {
        static Dictionary<string, string> Rule = new Dictionary<string, string>();
        public static void InitRules()
        {

        }
        public static string Convert(string Str)
        {
            foreach (var item in Rule)
            {
                if (Str.StartsWith(item.Key) )
                {
                    return Path.Combine(item.Value, Str.Substring(item.Key.Length));
                }
            }
            return ("."+Str);
        }
    }
}
