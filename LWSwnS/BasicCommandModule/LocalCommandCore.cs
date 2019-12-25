using LWSwnS.Api.Modules;
using LWSwnS.Core;
using System;

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
                LWSwnS.Api.Shell.Local.LocalShell.Register("cls", ClearScreen);
                LWSwnS.Api.Shell.Local.LocalShell.Register("clear", ClearScreen);
                LWSwnS.Api.Shell.Local.LocalShell.Register("version", ShowVersion);
                LWSwnS.Api.Shell.Local.LocalShell.Register("ver", ShowVersion);
            }

            return moduleDescription;
        }
        void ShowVersion(string s)
        {
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
    }
}
