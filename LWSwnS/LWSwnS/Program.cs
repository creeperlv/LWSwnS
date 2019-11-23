using LWSwnS.Api.Modules;
using LWSwnS.Configuration;
using LWSwnS.Core;
using LWSwnS.Core.Data;
using System;
using System.IO;

namespace LWSwnS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
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
                try
                {

                    Modules modules = new Modules((new FileInfo("./Modules/" + item)).DirectoryName);
                    var asm = modules.LoadFromAssemblyPath((new FileInfo("./Modules/" + item)).FullName);
                    var types = asm.GetTypes();
                    Console.WriteLine("Load:" + (new FileInfo("./Modules/" + item)).FullName);
                    foreach (var t in types)
                    {
                        //t.
                        if (typeof(ExtModule).IsAssignableFrom(t))
                        {
                            ExtModule extModule = Activator.CreateInstance(t) as ExtModule;
                            var ModDesc = extModule.InitModule();
                            ModDesc.targetAssembly = asm;
                            ModuleManager.ExtModules.Add(ModDesc);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed on loading:" + item);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                //ModuleManager.ExtModules.Add()
            }
            Console.WriteLine("Modules loaded.");
            Console.WriteLine(ModuleManager.ExtModules.Count + " Module(s) in total.");
            while (Console.ReadLine().ToUpper() != "EXIT")
            {

            }
        }
    }
}
