using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Configuration
{
    public class UniversalConfigurationMark2 : Dictionary<string, List<string>>
    {
        public void SaveToFile(string path)
        {
            List<string> Content = new List<string>();
            foreach (var item in this)
            {
                foreach (var value in item.Value)
                {

                    Content.Add(item.Key + "=" + value);
                }
            }
            if (!(new FileInfo(path)).Directory.Exists) (new FileInfo(path)).Directory.Create();
            File.WriteAllLines(path, Content);
        }
        public List<string> GetKeysStartedWith(string prefix)
        {
            List<string> FetchedKeys = new List<string>();
            foreach (var item in Keys)
            {
                if (item.ToUpper().StartsWith(prefix.ToUpper()))
                {
                    FetchedKeys.Add(item);
                }
            }
            return FetchedKeys;
        }
        public List<string> GetValues(string key)
        {
            foreach (var item in Keys)
            {
                if (item.ToUpper() == key.ToUpper())
                {
                    return this[item];
                }
            }
            return new List<string>();
        }
        public List<string> GetValuesSensitive(string key)
        {
            if (this.ContainsKey(key)) return this[key]; else return new List<string>();
        }
        public void AddItem(string key, string value)
        {
            if (!this.ContainsKey(key))
            {
                this.Add(key, new List<String>());
            }
            if(!this[key].Contains(value))
            {
                this[key].Add(value);
            }
        }
        public static UniversalConfigurationMark2 LoadFromFile(string path)
        {
            UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
            var lines = File.ReadAllLines(path);
            foreach (var item in lines)
            {

                if (item.IndexOf("=") > 0)
                {
                    string key = item.Substring(0, item.IndexOf('='));
                    string value = (item.Substring(item.IndexOf('=') + 1));
                    if (config.ContainsKey(key))
                    {
                        config[key].Add(value);
                    }
                    else
                    {
                        config.Add(key, new List<string>());
                        config[key].Add(value);
                    }
                }
            }
            return config;
        }
    }
}
