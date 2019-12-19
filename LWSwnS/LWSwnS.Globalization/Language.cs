using LWSwnS.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace LWSwnS.Globalization
{
    public class Language
    {
        static Dictionary<string, Dictionary<string, string>> LanguageDict = new Dictionary<string, Dictionary<string, string>>();
        public static string GetString(string LanguageFilePrefix,string key,string fallback="")
        {
            if (LanguageDict.ContainsKey(LanguageFilePrefix))
            {
                if (LanguageDict[LanguageFilePrefix].ContainsKey(key))
                {
                    return LanguageDict[LanguageFilePrefix][key];
                }
            }
            return fallback;
        }
        public static void LoadFile(string name)
        {
            FileInfo languagefile;
            if (File.Exists($"./Locales/{System.Globalization.CultureInfo.CurrentCulture.Name}/{name}.lang"))
            {
                languagefile = new FileInfo($"./Locales/{System.Globalization.CultureInfo.CurrentCulture.Name}/{name}.lang");
            }
            else if (File.Exists($"./Locales/en-US/{name}.lang"))
            {
                languagefile = new FileInfo($"./Locales/en-US/{name}.lang");
            }
            else
            {
                throw new Exception("Language file not found!");
            }
            UniversalConfiguration language = UniversalConfigurationLoader.LoadFromFile(languagefile.FullName);
            if (LanguageDict.ContainsKey(name))
            {
                LanguageDict[name] = language;
            }else
            LanguageDict.Add(name, language);
            //foreach (var item in language)
            //{

            //}
        }
        public static void Load()
        {
            LoadFile("General");
        }
    }
}
