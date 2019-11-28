using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Configuration
{
    public class UniversalConfigurationMark2:Dictionary<string,List<string>>
    {
        public static UniversalConfigurationMark2 LoadFromFile(string path)
        {
            UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
            var lines = File.ReadAllLines(path);
            foreach (var item in lines)
            {

                if (item.IndexOf("=") > 0)
                {
                    string key = item.Substring(0, item.IndexOf('='));
                    string value=(item.Substring(item.IndexOf('=') + 1));
                    if (config.ContainsKey(key))
                    {
                        config[key].Add(value);
                    }
                    else{
                        config.Add(key, new List<string>());
                        config[key].Add(value);
                    }
                }
            }
            return config;
        }
    }
}
