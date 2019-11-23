using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Configuration
{
    public class UniversalConfiguration:Dictionary<string,string>
    {

    }
    public class UniversalConfigurationLoader
    {
        public static UniversalConfiguration LoadFromFile(string path)
        {
            UniversalConfiguration config = new UniversalConfiguration();
            if (File.Exists(path))
            {

                var lines = File.ReadAllLines(path);
                foreach (var item in lines)
                {
                    if (item.IndexOf("=") > 0)
                    {
                        config.Add(item.Substring(0, item.IndexOf('=')), item.Substring(item.IndexOf('=') + 1));
                    }
                }
            }
            return config;
        }
    }
}
