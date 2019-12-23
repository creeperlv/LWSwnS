using System;
using System.Collections.Generic;
using System.IO;

namespace LWSwnS.Configuration
{
    public class ServerConfiguration
    {
        public static ServerConfiguration CurrentConfiguration = new ServerConfiguration();
        public string WebContentRoot = ".";
        public string Page404 = "./404.html";
        public string IP = "0.0.0.0";
        public string ShellPassword = "";
        public int WebPort = 8080;
        public int ShellPort = 14134;
        public bool isWebEnabled = true;
        public bool UseHttps = false;
        public string HttpsCert = "";
        public bool isShellEnabled = true;
        public bool isLogEnabled = true;
        public bool SplitModile = false;
        public string MobileWebContentRoot = "./Mobile";
        public int LogLevel = 0;//0 - Normal, 1 - Warning, 2 - Error
        public int LogSeparateSize = 512;//KB

        public List<string> AllowedModules = new List<string>();
    }
    public class ConfigurationLoader
    {
        public static ServerConfiguration LoadFromFile(string pathToConfigFile)
        {
            ServerConfiguration serverConfiguration = new ServerConfiguration();
            var lines = File.ReadAllLines(pathToConfigFile);
            foreach (var item in lines)
            {
                if (item.StartsWith("WebRoot="))
                {
                    serverConfiguration.WebContentRoot = item.Substring("WebRoot=".Length);
                }
                else
                if (item.StartsWith("MobileWebRoot="))
                {
                    serverConfiguration.MobileWebContentRoot = item.Substring("MobileWebRoot=".Length);
                }
                else
                if (item.StartsWith("IP="))
                {
                    serverConfiguration.IP = item.Substring("IP=".Length);
                }
                else
                if (item.StartsWith("HttpsCert="))
                {
                    serverConfiguration.IP = item.Substring("HttpsCert=".Length);
                }
                else
                if (item.StartsWith("Page404="))
                {
                    serverConfiguration.Page404 = item.Substring("Page404=".Length);
                }
                else
                if (item.StartsWith("ShellPassword="))
                {
                    serverConfiguration.ShellPassword = item.Substring("ShellPassword=".Length);
                }
                else
                if (item.StartsWith("WebPort="))
                {
                    serverConfiguration.WebPort = int.Parse(item.Substring("WebPort=".Length));
                }
                else
                if (item.StartsWith("LogLevel="))
                {
                    serverConfiguration.LogLevel = int.Parse(item.Substring("LogLevel=".Length));
                }
                else
                if (item.StartsWith("LogSeparateSize="))
                {
                    serverConfiguration.LogSeparateSize = int.Parse(item.Substring("LogSeparateSize=".Length));
                }
                else
                if (item.StartsWith("ShellPort="))
                {
                    serverConfiguration.ShellPort = int.Parse(item.Substring("ShellPort=".Length));
                }
                else
                if (item.StartsWith("isWebEnabled="))
                {
                    serverConfiguration.isWebEnabled = bool.Parse(item.Substring("isWebEnabled=".Length));
                }
                else
                if (item.StartsWith("SplitModile="))
                {
                    serverConfiguration.SplitModile = bool.Parse(item.Substring("SplitModile=".Length));
                }
                else
                if (item.StartsWith("UseHttps="))
                {
                    serverConfiguration.UseHttps = bool.Parse(item.Substring("UseHttps=".Length));
                }
                else
                if (item.StartsWith("isLogEnabled="))
                {
                    serverConfiguration.isLogEnabled = bool.Parse(item.Substring("isLogEnabled=".Length));
                }
                else
                if (item.StartsWith("isShellEnabled="))
                {
                    serverConfiguration.isShellEnabled = bool.Parse(item.Substring("isShellEnabled=".Length));
                }
                else
                if (item.StartsWith("AllowedModule="))
                {
                    serverConfiguration.AllowedModules.Add(item.Substring("AllowedModule=".Length));
                }
            }
            return serverConfiguration;
        }
        public static void SaveToFile(ServerConfiguration serverConfiguration, string path)
        {
            string content = "[Auto-Generated LWSwnS Configuration]";
            content += "\r\nIP=" + serverConfiguration.IP;
            content += "\r\nWebPort=" + serverConfiguration.WebPort;
            content += "\r\nShellPort=" + serverConfiguration.ShellPort;
            content += "\r\nisWebEnabled=" + serverConfiguration.isWebEnabled;
            content += "\r\nisShellEnabled=" + serverConfiguration.isShellEnabled;
            content += "\r\nWebRoot=" + serverConfiguration.WebContentRoot;
            content += "\r\nMobileWebRoot=" + serverConfiguration.MobileWebContentRoot;
            content += "\r\nSplitModile=" + serverConfiguration.SplitModile;
            content += "\r\nShellPassword=" + serverConfiguration.ShellPassword;
            content += "\r\nisLogEnabled=" + serverConfiguration.isLogEnabled;
            content += "\r\nLogLevel=" + serverConfiguration.LogLevel;
            content += "\r\nLogSeparateSize=" + serverConfiguration.LogSeparateSize;
            foreach (var item in serverConfiguration.AllowedModules)
            {
                content += "\r\nAllowedModule=" + item;
            }
            File.WriteAllText(path, content);
        }
    }
}
