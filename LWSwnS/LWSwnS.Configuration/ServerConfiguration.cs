﻿using System;
using System.Collections.Generic;
using System.IO;

namespace LWSwnS.Configuration
{
    public class ServerConfiguration
    {
        public static ServerConfiguration CurrentConfiguration=new ServerConfiguration();
        public string WebContentRoot = ".";
        public string IP = "0.0.0.0";
        public string ShellPassword = "";
        public int WebPort = 8080;
        public int ShellPort = 14134;
        public bool isWebEnabled = true;
        public bool UseHttps = false;
        public string HttpsCert = "";
        public bool isShellEnabled = true;
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
                if (item.StartsWith("IP="))
                {
                    serverConfiguration.IP = item.Substring("IP=".Length);
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
        public static void SaveToFile(ServerConfiguration serverConfiguration,string path)
        {
            string content="[Auto-Generated LWSwnS Configuration]";
            content += "\r\nIP="+serverConfiguration.IP;
            content += "\r\nWebPort=" + serverConfiguration.WebPort;
            content += "\r\nShellPort=" + serverConfiguration.ShellPort;
            content += "\r\nisWebEnabled=" + serverConfiguration.isWebEnabled;
            content += "\r\nisShellEnabled=" + serverConfiguration.isShellEnabled;
            content += "\r\nWebRoot=" + serverConfiguration.WebContentRoot;
            content += "\r\nShellPassword=" + serverConfiguration.ShellPassword;
            foreach (var item in serverConfiguration.AllowedModules)
            {
                content += "\r\nAllowedModule="+item;
            }
            File.WriteAllText(path,content);
        }
    }
}
