using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Configuration;
using LWSwnS.Core;
using System;
using System.IO;

namespace BasicCommandModule
{
    public class LocalCommandCore : ExtModule
    {
        public static readonly Version version = new Version(1, 0, 0, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "BasicCommandModule";
            moduleDescription.version = version;
            {
                LocalShell.Register("cls", ClearScreen);
                LocalShell.Register("clear", ClearScreen);
                LocalShell.Register("list-all-commands", listcmds);
                LocalShell.Register("change-working-directory", ChangeWorkingDirectory);
                LocalShell.Register("version", ShowVersion);
                LocalShell.Register("ver", ShowVersion);
            }

            return moduleDescription;
        }
        void ChangeWorkingDirectory(string s)
        {
            if (Directory.Exists(s))
            {
                ConfigurationLoader.SaveToFile(ServerConfiguration.CurrentConfiguration, "./Server.ini");
                ServerConfiguration.CurrentConfiguration.OverrideWorkingDirectory = s;
                Environment.CurrentDirectory = s;
            }
        }
        void ShowVersion(string s)
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
        void ClearScreen(string s)
        {
            Console.Clear();
        }
        void listcmds(string s)
        {
            foreach (var item in LocalShell.Commands)
            {
                foreach (var cmd in item.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{cmd.Key}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" from ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{item.Key}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
    }
}
