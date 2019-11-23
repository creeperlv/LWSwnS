using LWSwnS.Api.Modules;
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
            Console.WriteLine("Core Server Started.");
            Console.WriteLine("Init Modules");
            foreach (var item in serverConfiguration.AllowedModules)
            {
                Modules modules = new Modules("./Modules");
                var asm=modules.LoadFromAssemblyPath("./Modules/"+item);
                var types=asm.GetTypes();
                
                foreach (var t in types)
                {
                    //t.
                }
                //ModuleManager.ExtModules.Add()
            }
            Console.WriteLine("Modules loaded.");
            while (Console.ReadLine().ToUpper() != "EXIT")
            {

            }
        }
    }
}
