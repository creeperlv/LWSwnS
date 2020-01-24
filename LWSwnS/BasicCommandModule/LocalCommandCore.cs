using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core;
using LWSwnS.Core.Data;
using System;
using System.IO;

namespace BasicCommandModule
{
    public static class Flags
    {
        public static bool AutoReLoad = false;
        public static bool AutoRun = false;
    }
    public class LocalCommandCore : ExtModule
    {
        public static readonly Version version = new Version(1, 0, 0, 0);
        public static UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "BasicCommandModule";
            moduleDescription.version = version;
            Load();
            if(Flags.AutoReLoad)
            Tasks.RegisterTask(Load, Tasks.TaskType.Every10Seconds);
            if (Flags.AutoRun)
            {
                Tasks.RegisterTask(() =>
                {
                    var f = config.GetValues("AutoRunFile", "./AUTORUN")[0];
                    var lines = File.ReadAllLines(f);
                    foreach (var item in lines)
                    {
                        if (!item.StartsWith("") && item != "")
                        {
                            LocalShell.Invoke(item);
                        }
                    }
                }, Tasks.TaskType.AfterAllModuleLoaded);
            }
            {
                LocalShell.Register("cls", ClearScreen);
                LocalShell.Register("bc-get-help", ShowHelp);
                LocalShell.Register("clear", ClearScreen);
                LocalShell.Register("add-rule", AddRule);
                LocalShell.Register("list-all-commands", listcmds);
                LocalShell.Register("set-preset", SetPreset);
                LocalShell.Register("change-working-directory", ChangeWorkingDirectory);
                LocalShell.Register("version", ShowVersion);
                LocalShell.Register("ver", ShowVersion);
            }

            return moduleDescription;
        }
        void ShowHelp(string s, bool b)
        {
            ShowMessage("cls\r\nclear", "Clear screen.");
            ShowMessage("Add-Rule", "Add a rule.");
            ShowMessage("Change-Working-Directory", "Set working directory to a specified path.");
            ShowMessage("version\r\nver", "Display the version of LWSwnS.");
        }
        void ShowMessage(string title,string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t"+msg);
        }
        void AddRule(string s,bool b)
        {
            URLConventor.AddRule(s.Split('|')[0], s.Split('|')[1]);
            URLConventor.SaveRule();
        }
        void Load()
        {
            try
            {
                config = UniversalConfigurationMark2.LoadFromFile("./Configs/BasicCommand.ini");
            }
            catch{}
            try
            {
                var a=bool.Parse(config.GetValues("Flags.AutoReload","False")[0]);
                Flags.AutoReLoad = a;
            }
            catch{}
            try
            {
                var a=bool.Parse(config.GetValues("Flags.AutoRun","False")[0]);
                Flags.AutoRun = a;
            }
            catch{}

        }
        void SetPreset(string s,bool b)
        {
            var key = s.Substring(0,s.IndexOf('='));
            var value = s.Substring(s.IndexOf('=')+1);
            WebPagePresets.AddPreset(key, value);
        }
        void ChangeWorkingDirectory(string s, bool b)
        {
            if (Directory.Exists(s))
            {
                ConfigurationLoader.SaveToFile(ServerConfiguration.CurrentConfiguration, "./Server.ini");
                ServerConfiguration.CurrentConfiguration.OverrideWorkingDirectory = s;
                Environment.CurrentDirectory = s;
            }
        }
        void ShowVersion(string s, bool b)
        {
            Console.Write("ServerConfiguration:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(ServerConfiguration.ConfigurationVersion.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("WebServer:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(HttpServer.WebServerVersion.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("ShellServer:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(ShellServer.ShellServerVersion.ToString());
            Console.ForegroundColor = ConsoleColor.White;
        }
        void ClearScreen(string s, bool b)
        {
            Console.Clear();
        }
        void listcmds(string s, bool b)
        {
            foreach (var item in LocalShell.Commands)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Commands in ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{item.Key}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($":");
                Console.WriteLine();
                foreach (var cmd in item.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\t{cmd.Key}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("\t\tFull name:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{item.Key}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(":");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{cmd.Key}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine();
            }
        }
    }
}
