﻿using LWSwnS.Api.Modules;
using LWSwnS.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace LWSwnS.Core.Data
{
    public class URLConventor
    {
        public static string RootFolder = ".";
        public static string MobileRootFolder = ".";
        //static Dictionary<string, string> Rule = new Dictionary<string, string>();
        static UniversalConfiguration Rule = new UniversalConfiguration();
        public static void AddRule(string url,string route)
        {
            Rule.Add(url, route);
        }
        public static void SaveRule()
        {
            Rule.SaveToFile("./Configs/URLRules.ini");
        }
        public static void InitRules()
        {
            try
            {

                Rule = UniversalConfigurationLoader.LoadFromFile("./Configs/URLRules.ini");
            }
            catch
            {
            }
            Tasks.RegisterTask(() =>
            {
                try
                {
                    Rule = UniversalConfigurationLoader.LoadFromFile("./Configs/URLRules.ini");
                }
                catch
                {
                }
            }, Tasks.TaskType.Every5Seconds);

        }
        public static string Convert(string Str, bool isMobile = false)
        {

            foreach (var item in Rule)
            {
                if (Str.StartsWith(item.Key))
                {
                    return Path.Combine(item.Value, Str.Substring(item.Key.Length));
                }
            }
            return ((isMobile == false ? RootFolder : MobileRootFolder) + Str);
        }
    }
}
