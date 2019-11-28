﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Configuration
{
    public class UniversalConfiguration : Dictionary<string, string>
    {
        public string Get(string key, string DefaultValue = "")
        {
            if (this.ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return DefaultValue;
            }
        }
        public void SaveToFile(string Path)
        {
            var content = "[Generated by Universal Congiguration in LWSenS.Configuration]";
            foreach (var item in this)
            {
                content += Environment.NewLine + item.Key + "=" + item.Value;
            }
            File.WriteAllText(Path, content);
        }
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