using System;
using System.Collections.Generic;
using System.Text;

namespace LWSwnS.Api.Web
{
    public class WebPagePresets
    {
        public static Dictionary<string, string> Presets = new Dictionary<string, string>();
        public static void AddPreset(string key,string value)
        {
            if (!Presets.ContainsKey(key)) Presets.Add(key, value);
            Presets[key] = value;
        }
        public static void RemovePreset(string key)
        {
            if (Presets.ContainsKey(key)) Presets.Remove(key);
        }
        public static void ApplyPreset(ref string content)
        {
            foreach (var item in Presets)
            {
                content = content.Replace($"{item.Key}", item.Value);
            }
        }
    }
}
