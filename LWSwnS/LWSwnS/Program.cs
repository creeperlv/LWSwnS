using LWSwnS.Api;
using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Configuration;
using LWSwnS.Core;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace LWSwnS
{
    class Program
    {
        static void FirstInitialize()
        {
            Console.WriteLine("Please specify the address that you want the server to run on:(Leave space means 0.0.0.0)");
            var ip = Console.ReadLine();
            if (ip == "") ip = "0.0.0.0";
            Console.Write("Please specify the port that you want the ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Web");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" server to listen:(Leave space means 80)");
            var webPort = Console.ReadLine();
            if (webPort == "") webPort = "80";
            int WebP = 80;
            try
            {
                WebP = int.Parse(webPort);
            }
            catch (Exception)
            {

            }
            Console.Write("Please specify the port that you want the ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Shell");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" server to listen:(Leave space means 9341)");
            var shellPort = Console.ReadLine();
            if (shellPort == "") shellPort = "9341";
            int ShellPort = 80;
            try
            {
                ShellPort = int.Parse(shellPort);
            }
            catch (Exception)
            {

            }
            ServerConfiguration.CurrentConfiguration= new ServerConfiguration();
            ServerConfiguration.CurrentConfiguration.IP = ip;
            ServerConfiguration.CurrentConfiguration.WebPort = WebP;
            ServerConfiguration.CurrentConfiguration.ShellPort = ShellPort;
            {
                Console.Write("Do you want to");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" enable ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("WebServer?(Y for yes.)");
                if (Console.ReadLine().ToUpper().Equals("Y"))
                {
                    ServerConfiguration.CurrentConfiguration.isWebEnabled = true;
                    {
                        Console.Write("Please specify will the ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" root of web contents ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("should be?(Leave space for \"./WebContents\".)");
                        var root = Console.ReadLine();
                        if (root == "") root = "./WebContents";
                        if (root.EndsWith('/') | root.EndsWith('\\'))
                        {
                            root.Remove(root.Length - 1);
                        }
                        if (!Directory.Exists(root))
                        {
                            Directory.CreateDirectory(root);
                        }
                        ServerConfiguration.CurrentConfiguration.WebContentRoot = root;
                    }
                }
                else ServerConfiguration.CurrentConfiguration.isWebEnabled = false;
            }
            {
                Console.Write("Do you want to");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" enable ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("ShellServer?(Y for yes.)");
                if (Console.ReadLine().ToUpper().Equals("Y"))
                {
                    ServerConfiguration.CurrentConfiguration.isShellEnabled = true;
                }
                else ServerConfiguration.CurrentConfiguration.isShellEnabled = false;
            }
            {
                Console.Write("Please ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" remember ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("the generated AES key:");
                Console.ForegroundColor = ConsoleColor.Green;
                string KEY = NETCore.Encrypt.EncryptProvider.CreateAesKey().Key;
                ServerConfiguration.CurrentConfiguration.ShellPassword = KEY;
                Console.WriteLine(KEY);
                Console.ForegroundColor = ConsoleColor.White;
            }
            {
                
                Console.Write("Press ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" ENTER ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("to continue.");
                Console.ReadLine();
                ConfigurationLoader.SaveToFile(ServerConfiguration.CurrentConfiguration, "./Server.ini");
                Console.Clear();
            }
        }
        static void InitApis()
        {

            ApiManager.AddFunction("REGCMD", (UniParamater p) => {
                var name = p[0] as string;
                var action = p[1] as Func<string, object, StreamWriter,bool>;
                if (ShellServer.Commands.ContainsKey(name))
                {
                    ShellServer.Commands[name] = action;
                }
                else
                {
                    ShellServer.Commands.Add(name, action);
                }
                return new UniResult();
            });
        }
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("LWSwnS - Lite Web Server with uNsafe Shell");
            if (!File.Exists("./Server.ini"))
            {
                FirstInitialize();
            }
            try
            {
                ServerConfiguration.CurrentConfiguration= ConfigurationLoader.LoadFromFile("./Server.ini");
            }
            catch (Exception)
            {
            }
            if (ServerConfiguration.CurrentConfiguration.isLogEnabled == true)
            {
                Debugger.currentDebugger = new Debugger(ServerConfiguration.CurrentConfiguration.LogLevel, ServerConfiguration.CurrentConfiguration.LogSeparateSize);
            }
            else
            {
                Debugger.currentDebugger = new EmptyDebugger();
            }
            InitApis();
            ShellDataExchange.AES_PW = ServerConfiguration.CurrentConfiguration.ShellPassword;
            URLConventor.RootFolder = ServerConfiguration.CurrentConfiguration.WebContentRoot;
             LWSwnSServerCore a = new LWSwnSServerCore(ServerConfiguration.CurrentConfiguration.IP, ServerConfiguration.CurrentConfiguration.WebPort, ServerConfiguration.CurrentConfiguration.ShellPort);
            if (ServerConfiguration.CurrentConfiguration.isWebEnabled)
                a.StartListenWeb();
            if (ServerConfiguration.CurrentConfiguration.isShellEnabled)
                a.StartListenShell();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Core Server Started.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Loading Modules");
            foreach (var item in ServerConfiguration.CurrentConfiguration.AllowedModules)
            {
                try
                {

                    Modules modules = new Modules((new FileInfo("./Modules/" + item)).DirectoryName);
                    var asm = modules.LoadFromAssemblyPath((new FileInfo("./Modules/" + item)).FullName);
                    //AssemblyLoadContext.Default.LoadFromAssemblyPath
                    //Console.WriteLine(ra);
                    var types = asm.GetTypes();
                    Console.Write("\tLoad: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(new FileInfo("./Modules/" + item).Name);
                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (var t in types)
                    {
                        //t.
                        if (typeof(ExtModule).IsAssignableFrom(t))
                        {
                            ExtModule extModule = Activator.CreateInstance(t) as ExtModule;
                            var ModDesc = extModule.InitModule();
                            ModDesc.targetAssembly = asm;
                            ModuleManager.ExtModules.Add(ModDesc);

                            Console.Write("\t\tExtModule Description: ");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(ModDesc.Name);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("/");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(ModDesc.version.ToString());
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed on loading:" + item+"\r\n"+e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                //ModuleManager.ExtModules.Add()
            }
            Console.Write("Loaded ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(ModuleManager.ExtModules.Count + "");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" ExtModule(s) in total.");
            Console.WriteLine("The server is now fully running.");
            string cmd;
            while ((cmd = Console.ReadLine()).ToUpper() != "EXIT")
            {
                if (cmd.StartsWith("Init-Module "))
                {
                    var item = cmd.Substring("Init-Module ".Length);
                    try
                    {

                        Modules modules = new Modules((new FileInfo("./Modules/" + item)).DirectoryName);
                        var asm = modules.LoadFromAssemblyPath((new FileInfo("./Modules/" + item)).FullName);
                        var types = asm.GetTypes();
                        Console.Write("\tLoad: ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(new FileInfo("./Modules/" + item).Name);
                        Console.ForegroundColor = ConsoleColor.White;
                        foreach (var t in types)
                        {
                            //t.
                            if (typeof(FirstInit).IsAssignableFrom(t))
                            {
                                FirstInit extModule = Activator.CreateInstance(t) as FirstInit;
                                extModule.Init();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\t\tInitialization Completed.");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                        ServerConfiguration.CurrentConfiguration.AllowedModules.Add(item);
                        ConfigurationLoader.SaveToFile(ServerConfiguration.CurrentConfiguration, "./Server.ini");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Module is now allowed to be executed.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Restart to take effect.");
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Failed on loading:" + item);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else if (cmd.Equals("Reconfig"))
                {
                    FirstInitialize();

                }
            }
        }
    }
}
