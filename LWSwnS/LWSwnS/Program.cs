using LWSwnS.Configuration;
using LWSwnS.Core;
using LWSwnS.Core.Data;
using System;

namespace LWSwnS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("LWSwnS - Lite Web Server with uNsafe Shell");
            ServerConfiguration serverConfiguration = new ServerConfiguration();
            try
            {
                serverConfiguration = ConfigurationLoader.LoadFromFile("./Server.ini");
            }
            catch (Exception)
            {
            }
            URLConventor.RootFolder = serverConfiguration.WebContentRoot;
            //LWSwnSServerCore a = new LWSwnSServerCore(serverConfiguration.IP, serverConfiguration.WebPort, serverConfiguration.ShellPort);
            LWSwnSServerCore a = new LWSwnSServerCore(serverConfiguration.IP, serverConfiguration.WebPort, serverConfiguration.ShellPort);
            if (serverConfiguration.isWebEnabled)
                a.StartListenWeb();
            if (serverConfiguration.isShellEnabled)
                a.StartListenShell();
            foreach (var item in serverConfiguration.AllowedModules)
            {
                Modules modules = new Modules("./Modules");
                modules.LoadFromAssemblyPath("./Modules/"+item);
            }
            while (Console.ReadLine().ToUpper() != "EXIT")
            {

            }
        }
    }
}
