using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;

namespace LWSwnS
{
    public class Modules : AssemblyLoadContext
    {
        List<JObject> Depss = new List<JObject>();
        private AssemblyDependencyResolver resolver;
        public string GenerateRID()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "win-"+RuntimeInformation.OSArchitecture.ToString().ToLower();
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux-" + RuntimeInformation.OSArchitecture.ToString().ToLower();
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "osx";
            }
            else
            {
                return "win-any";
            }
        }
        string root;
        public string GenerateRIDWinVer()
        {
            var str = RuntimeInformation.OSDescription.ToUpper();
            str = str.Substring("Microsoft Windows ".Length);
            var id = "win";
            if (str.StartsWith("10"))
            {
                id += "10";
            }else if (str.StartsWith("8.1"))
            {
                id += "81";
            }else if (str.StartsWith("8.0"))
            {
                id += "8";
            }else if (str.StartsWith("7"))
            {
                id += "7";
            }
            id += "-"+RuntimeInformation.OSArchitecture.ToString().ToLower();
            //Console.WriteLine(id);
            return id;
        }
        public Modules(string path)
        {
            root = path;
            //Console.WriteLine($"{System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var f=directoryInfo.EnumerateFiles("*.deps.json");
            foreach (var item in f)
            {
                Console.WriteLine("Loaded Dependency:"+item.Name);
                Depss.Add(JObject.Parse(File.ReadAllText(item.FullName)));
            }
            this.Resolving += Modules_Resolving;
            resolver = new AssemblyDependencyResolver(path);
        }


        private Assembly Modules_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            if (arg2.Name.EndsWith(".resources"))
            {
                //Console.WriteLine("Skipped.");
                return null;
            }
            Console.WriteLine("Try to resolve:"+arg2.Name);
            try
            {
                //DependencyContext dependencyContext=new DependencyContext()
                List<string> dlls = new List<string>();
                foreach (var dep in Depss)
                {
                    Console.WriteLine(dep.Path);
                    var t = dep["targets"] as JObject;
                    foreach (var target in t)
                    {
                        //Console.WriteLine(target.Key);
                        var singleT = target.Value as JObject;
                        foreach (var singleDep in singleT)
                        {
                            if (singleDep.Key.StartsWith(arg2.Name + "/"))
                            {
                                var runtimeTargets = singleDep.Value["runtimeTargets"] as JObject;
                                foreach (var runtimeTarget in runtimeTargets)
                                {
                                    if ((string)runtimeTarget.Value["rid"] == GenerateRID() || (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) & (string)runtimeTarget.Value["rid"] == GenerateRIDWinVer()))
                                    {
                                        if (runtimeTarget.Key.ToUpper().EndsWith("NATIVE.DLL"))
                                        {
                                            this.LoadUnmanagedDllFromPath(new FileInfo(Path.Combine(root, runtimeTarget.Key)).FullName);
                                            continue;
                                        }
                                        Console.WriteLine("Loaded:"+runtimeTarget.Key);
                                        dlls.Add(runtimeTarget.Key);
                                        var a = this.LoadFromAssemblyPath(new FileInfo(Path.Combine(root, runtimeTarget.Key)).FullName);
                                        return a;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to resolve assembly"+e.Message);
            }
            
            return null;
        }

        protected override Assembly Load(AssemblyName name)
        {
            string a = resolver.ResolveAssemblyToPath(name);
            return a!=null?LoadFromAssemblyPath(a):null;
        }
    }
}
