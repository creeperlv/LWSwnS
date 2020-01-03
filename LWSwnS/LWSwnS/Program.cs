using LWSwnS.Api;
using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using LWSwnS.Globalization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

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
            ServerConfiguration.CurrentConfiguration = new ServerConfiguration();
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
                    {
                        Console.Write("Do you want to");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" split ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("contents for mobile?(Y for yes.)");

                        if (Console.ReadLine().ToUpper().Equals("Y"))
                        {
                            ServerConfiguration.CurrentConfiguration.SplitModile = true;
                            {
                                Console.Write("Please specify will the ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(" root of web contents for mobile ");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("should be?(Leave space for \"./MobileWebContents\".)");
                                var root = Console.ReadLine();
                                if (root == "") root = "./MobileWebContents";
                                if (root.EndsWith('/') | root.EndsWith('\\'))
                                {
                                    root.Remove(root.Length - 1);
                                }
                                if (!Directory.Exists(root))
                                {
                                    Directory.CreateDirectory(root);
                                }
                                ServerConfiguration.CurrentConfiguration.MobileWebContentRoot = root;
                            }
                        }
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
            FileUtilities.InitLocation(typeof(Program));
            ApiManager.AddFunction("REGCMD", (UniParamater p) =>
            {
                var name = p[0] as string;
                var action = p[1] as Func<string, object, StreamWriter, bool>;
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
            ApiManager.AddFunction("LOCAL-SHELL-INVOKE", (UniParamater p) =>
            {
                var cmd = p[0] as string;

                var subbed = cmd.ToUpper();
                try
                {

                    if (subbed.IndexOf(' ') > 0)
                    {
                        subbed = subbed.Substring(0, subbed.IndexOf(' '));
                    }
                    bool Find = false;
                    if (subbed.IndexOf('/') > 0)
                    {

                        // fully qualified command.
                        string Origin = subbed.Split('/')[0];
                        string SpecifiedCMD = subbed.Split('/')[1];
                        foreach (var moduleCMD in LocalShell.Commands)
                        {
                            if (moduleCMD.Key.ToUpper() == Origin.ToUpper())
                                foreach (var singleCMD in moduleCMD.Value)
                                {
                                    if (SpecifiedCMD == (singleCMD.Key.ToUpper()))
                                    {
                                        singleCMD.Value(cmd.Substring(subbed.Length).Trim(),true);
                                        Find = true;
                                    }
                                    if (Find == true) break;
                                }
                            if (Find == true) break;
                        }
                        //LocalShell.Commands[Origin][SpecifiedCMD](cmd.Substring(subbed.Length).Trim());
                    }
                    if (Find == false)
                        foreach (var moduleCMD in LocalShell.Commands)
                        {
                            foreach (var singleCMD in moduleCMD.Value)
                            {
                                if (subbed == (singleCMD.Key.ToUpper()))
                                {
                                    singleCMD.Value(cmd.Substring(subbed.Length).Trim(),true);
                                    Find = true;
                                }
                                if (Find == true) break;
                            }
                            if (Find == true) break;
                        }
                    if (Find == false)
                    {
                        Console.WriteLine(Language.GetString("General", "Host.Cmd.NotFound", "\"{cmd}\" is neither an internal command nor external command.").Replace("{cmd}", cmd));
                    }
                }
                catch (Exception e)
                {
                    Debugger.currentDebugger.Log("Error in executing command:" + subbed.Trim() + "\r\n\t" + e.Message, MessageType.Error);
                }
                return new UniResult();
            });
            ApiManager.AddFunction("UNREGCMD", (UniParamater p) =>
            {
                var name = p[0] as string;
                if (ShellServer.Commands.ContainsKey(name))
                {
                    ShellServer.Commands.Remove(name);
                }
                return new UniResult();
            });
            ApiManager.AddFunction("MODULE_INIT", (UniParamater p) =>
            {
                Modules modules = new Modules((new FileInfo("./Modules/" + p[0])).DirectoryName);
                var asm = modules.LoadFromAssemblyPath((new FileInfo("./Modules/" + p[0])).FullName);
                var types = asm.GetTypes();
                Console.Write("\tLoad: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(new FileInfo("./Modules/" + p[0]).Name);
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
                ServerConfiguration.CurrentConfiguration.AllowedModules.Add(p[0] as string);
                ConfigurationLoader.SaveToFile(ServerConfiguration.CurrentConfiguration, "./Server.ini");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Module is now allowed to be executed.");
                Console.ForegroundColor = ConsoleColor.White;
                return new UniResult();
            });
            ApiManager.AddFunction("MODULE_UNLD", (UniParamater p) =>
            {
                try
                {
                    List<int> ids = new List<int>();
                    for (int i = 0; i < ModuleManager.ExtModules.Count; i++)
                    {
                        try
                        {
                            ids.Add(i);
                            var dllfile = new FileInfo(ModuleManager.ExtModules[i].targetAssembly.Location).FullName;
                            var TargetFile = new FileInfo(p[0] as String).FullName;
                            if (TargetFile == dllfile)
                            {
                                (ModuleManager.ExtModules[i].Environment as Modules).Unload();
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    for (int i = ids.Count - 1; i >= 0; i--)
                    {
                        ModuleManager.ExtModules.RemoveAt(ids[i]);
                    }
                    //ModuleManager.ExtModules.RemoveAt();
                }
                catch (Exception)
                {
                }
                return new UniResult();
            });
            ApiManager.AddFunction("MODULE_LOAD", (UniParamater p) =>
            {
                List<ModuleDescription> result = new List<ModuleDescription>();
                Modules modules = new Modules((new FileInfo("./Modules/" + p[0])).DirectoryName);
                var asm = modules.LoadFromAssemblyPath((new FileInfo("./Modules/" + p[0])).FullName);
                var types = asm.GetTypes();
                Console.Write("\tLoad: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(new FileInfo("./Modules/" + p[0]).Name);
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var t in types)
                {
                    //t.
                    if (typeof(ExtModule).IsAssignableFrom(t))
                    {
                        ExtModule extModule = Activator.CreateInstance(t) as ExtModule;
                        var ModDesc = extModule.InitModule();
                        ModDesc.Environment = modules;
                        ModDesc.targetAssembly = asm;
                        result.Add(ModDesc);
                    }
                }
                ServerConfiguration.CurrentConfiguration.AllowedModules.Add(p[0] as string);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Module is now allowed to be executed.");
                Console.ForegroundColor = ConsoleColor.White;
                return new UniResult() { Data = result };
            });

            {
                //Determine System Infos.
                WebPagePresets.AddPreset("Sys.Description", RuntimeInformation.OSDescription);
                WebPagePresets.AddPreset("Sys.Architecture", RuntimeInformation.OSArchitecture.ToString());
                WebPagePresets.AddPreset("LWSwnS.Web.Version", HttpServer.WebServerVersion.ToString());
                Console.WriteLine(Language.GetString("General", "Host.AddPreset", "Added Preset:") + "Sys.Description=" + RuntimeInformation.OSDescription);
                Console.WriteLine(Language.GetString("General", "Host.AddPreset", "Added Preset:") + "Sys.Architecture=" + RuntimeInformation.OSArchitecture.ToString());
                Console.WriteLine(Language.GetString("General", "Host.AddPreset", "Added Preset:") + "LWSwnS.Web.Version=" + HttpServer.WebServerVersion.ToString());
            }
        }
        static void StartTasks()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    {

                        await Task.Delay(5000);
                        try
                        {
                            ServerConfiguration.CurrentConfiguration = ConfigurationLoader.LoadFromFile("./Server.ini");
                        }
                        catch { }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery5Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        await Task.Delay(5000);
                        try
                        {
                        }
                        catch { }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery5Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery10Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    {

                        await Task.Delay(5000);
                        try
                        {
                            ServerConfiguration.CurrentConfiguration = ConfigurationLoader.LoadFromFile("./Server.ini");
                        }
                        catch { }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery5Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        await Task.Delay(5000);
                        try
                        {
                        }
                        catch { }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery5Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery10Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    {

                        await Task.Delay(5000);
                        try
                        {
                            ServerConfiguration.CurrentConfiguration = ConfigurationLoader.LoadFromFile("./Server.ini");
                        }
                        catch { }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery5Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        await Task.Delay(5000);
                        try
                        {
                        }
                        catch { }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery5Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery10Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        try
                        {

                            foreach (var item in Tasks.TaskEvery30Seconds)
                            {
                                foreach (var action in item.Value)
                                {
                                    try
                                    {

                                        action();

                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            });
        }
        static void InitModuleFromList(string lst,bool b)
        {
            if (b == true)
            {
                if (LocalShell.RequireAuthCMD("Init-Module") == true) { return; }
            }
            else
            {

            }
            if (File.Exists(lst))
            {
                var list = File.ReadAllLines(lst);
                foreach (var item in list)
                {
                    if (File.Exists(item))
                    {
                        try
                        {
                            ModuleManager.InitModule(item);
                            foreach (var desc in ModuleManager.LoadModule(item))
                            {
                                ModuleManager.ExtModules.Add(desc);
                            }
                        }
                        catch (Exception)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed on loading:" + item);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
            else
            {
                Debugger.currentDebugger.Log("List file not found.", MessageType.Warning);
            }
        }
        static void Main(string[] args)
        {
            Console.Title = "LWSwnS";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("LWSwnS - Lite Web Server with uNsafe Shell");
            Console.WriteLine("Initialize Localization Flavor");
            Console.Write("Region:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(System.Globalization.CultureInfo.CurrentCulture.Name);
            Console.ForegroundColor = ConsoleColor.White;
            if (!Directory.Exists("./Configs/"))
            {
                Directory.CreateDirectory("./Configs");
            }
            try
            {
                Language.Load();
            }
            catch (Exception e)
            {

                Debugger.currentDebugger.Log("Cannot initialize localization flavor!:" + e.Message, MessageType.Error);
                Debugger.currentDebugger.Log("Current Directory:" + (new DirectoryInfo("./").FullName), MessageType.Error);
            }
            if (!File.Exists("./Server.ini"))
            {
                FirstInitialize();
            }
            try
            {
                ServerConfiguration.CurrentConfiguration = ConfigurationLoader.LoadFromFile("./Server.ini");

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
            URLConventor.MobileRootFolder = ServerConfiguration.CurrentConfiguration.MobileWebContentRoot;
            LWSwnSServerCore a = new LWSwnSServerCore(ServerConfiguration.CurrentConfiguration.IP, ServerConfiguration.CurrentConfiguration.WebPort, ServerConfiguration.CurrentConfiguration.ShellPort);
            if (ServerConfiguration.CurrentConfiguration.isWebEnabled)
                a.StartListenWeb();
            if (ServerConfiguration.CurrentConfiguration.isShellEnabled)
                a.StartListenShell();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Language.GetString("General", "Host.CoreStarted", "Core Server Started."));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Language.GetString("General", "Host.LoadModules", "Loading Modules"));
            foreach (var item in ServerConfiguration.CurrentConfiguration.AllowedModules)
            {
                try
                {

                    Modules modules = new Modules((new FileInfo("./Modules/" + item)).DirectoryName);
                    var asm = modules.LoadFromAssemblyPath((new FileInfo("./Modules/" + item)).FullName);
                    //AssemblyLoadContext.Default.LoadFromAssemblyPath
                    //Console.WriteLine(ra);
                    var types = asm.GetTypes();
                    Console.Write(Language.GetString("General", "Host.Module.Load", "\tLoad: "));
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
                            ModDesc.Environment = modules;
                            ModuleManager.ExtModules.Add(ModDesc);

                            Console.Write(Language.GetString("General", "Host.Module.ExtMod", "\t\tExtModule Description: "));
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
                    Console.WriteLine("Failed on loading:" + item + "\r\n" + e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                //ModuleManager.ExtModules.Add()
            }
            Console.Write(Language.GetString("General", "Host.LoadModule.FirstHalf", "Loaded "));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(ModuleManager.ExtModules.Count + "");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Language.GetString("General", "Host.LoadModule.SecondHalf", " ExtModule(s) in total."));
            Console.WriteLine(Language.GetString("General", "Host.FullRun", "The server is now fully running."));
            StartTasks();
            string cmd;
            LocalShell.Register("Init-Module", (s,b) =>
            {
                if (b == true)
                {
                    if (LocalShell.RequireAuthCMD("Init-Module")==true) { return; }
                }
                else
                {

                }
                var item = s.Trim();
                try
                {
                    ModuleManager.InitModule(item);
                    foreach (var desc in ModuleManager.LoadModule(item))
                    {
                        ModuleManager.ExtModules.Add(desc);
                    }
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed on loading:" + item);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            });
            LocalShell.Register("Disable-Module", (s, b) =>
            {
                if (b == true)
                {
                    if (LocalShell.RequireAuthCMD("Init-Module") == true) { return; }
                }
                else
                {

                }
                var item = s.Trim();
                ServerConfiguration.CurrentConfiguration.AllowedModules.Remove(item);
                ConfigurationLoader.SaveToFile(ServerConfiguration.CurrentConfiguration, "./Server.ini");
            });
            LocalShell.Register("Init-Module-From-List", InitModuleFromList);
            LocalShell.Register("initmods", InitModuleFromList);
            LocalShell.Register("Move-All-Configs", (s, b) =>
            {
                if (b == true)
                {
                    if (LocalShell.RequireAuthCMD("Init-Module") == true) { return; }
                }
                else
                {

                }
                Debugger.currentDebugger.Log("Finding ini files...");
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(".");
                    var configs = directoryInfo.EnumerateFiles("*.ini");
                    if (!Directory.Exists("./Configs/")) Directory.CreateDirectory("./Configs/");
                    foreach (var item in configs)
                    {
                        if (item.Name != "Server.ini")
                            item.MoveTo("./Configs/" + item.Name);
                    }
                }
                Debugger.currentDebugger.Log("OK. Some modules may need restart to take effect.");
            });
            LocalShell.Register("Reconfig", (s, b) =>
            {
                if (b == true)
                {
                    if (LocalShell.RequireAuthCMD("Init-Module") == true) { return; }
                }
                else
                {

                }
                FirstInitialize();
            });
            {
                //FINALIZE LASR STEP
                foreach (var item in Tasks.AfterAllModulesLoaded)
                {
                    foreach (var action in item.Value)
                    {
                        action();
                    }
                }
                Tasks.ClearTask_AfterAllModulesLoaded();
            }
            while ((cmd = Console.ReadLine()).ToUpper() != "EXIT")
            {
                if (cmd.ToUpper().Equals("Help".ToUpper()) || cmd.Equals("?"))
                {
                    Console.WriteLine("");
                    Console.WriteLine("Help of LWSwnS server console");
                    {
                        Console.Write("Init-Module ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("<Path-To-Module-Dll-File>");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\tInits the specified module and add it to allow list.");
                        Console.WriteLine("");
                    }
                    {
                        Console.Write("Disable-Module ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("<Path-To-Module-Dll-File>");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\tRemove the given module from the allow list.");
                        Console.WriteLine("");
                    }
                    {
                        Console.WriteLine("Reconfig");
                        Console.Write("\tReconfigurate the server.");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" All allowed modules will be removed in the same time.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("");
                    }
                    {
                        Console.WriteLine("Move-All-Configs");
                        Console.Write("\tDue to the previous changes in modules to prevent the home directory from being too messy. All configuration files are moved to ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" ./Configs/ ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("except"); Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Server.ini");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(".");
                        Console.WriteLine("");
                    }
                }
                else
                {
                    UniParamater uniParamater = new UniParamater();
                    uniParamater.Add(cmd);
                    uniParamater.Add(false);
                    ApiManager.Functions["LOCAL-SHELL-INVOKE"](uniParamater);
                }
            }
        }
    }
}
