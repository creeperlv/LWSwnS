using LWSwnS.Api.Modules;
using LWSwnS.Diagnostic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdvancedModuleManagement
{
    public partial class MainEntrance
    {
        public static List<Package> PackagesToUpdate = new List<Package>();
        void CheckUpdate(string s, bool b)
        {
            FindPackageToUpdate();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(PackagesToUpdate.Count);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" package(s) in total can be updated.");
        }
        void FindPackageToUpdate()
        {
            PackagesToUpdate = new List<Package>();
            foreach (var item in InstalledModules)
            {
                var src = item.Value.OriginalSource;
                foreach (var SingleSrc in Sources)
                {
                    if (SingleSrc.Name == src)
                    {
                        for (int i = 0; i < SingleSrc.PackageVersion.Count; i++)
                        {
                            if (item.Value.Name == SingleSrc.PackageName[i])
                            {
                                if (!item.Value.Version.ToUpper().Equals(SingleSrc.PackageVersion[i].ToUpper()))
                                {
                                    PackagesToUpdate.Add(item.Value);
                                    Console.Write("Version change in ");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(item.Value.Name);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.Write(" : ");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(item.Value.Version + " -> " + SingleSrc.PackageVersion[i]);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    goto a;
                                }
                            }
                        }
                    }
                }
            a:
                ;
            }
        }
        void PackModule(string s,bool b)
        {

        }
        void GenerateList(string s,bool b)
        {

        }
        void UnloadModule(string s, bool b)
        {

            foreach (var item in ActivatedModules)
            {
                if (item.Value.Name.ToUpper() == s.ToUpper())
                {
                    ModuleManager.UnloadModule(Path.Combine(InstalledModules.ElementAt(InstalledModules.Values.ToList().IndexOf(item.Value)).Key.FullName, item.Value.MainDLL));
                    return;
                }
            }
        }
        void UpdateModule(string s, bool b)
        {
            foreach (var item in PackagesToUpdate)
            {
                if (item.Name.ToUpper() == s.ToUpper())
                {
                    Console.WriteLine("Unloading module.");
                    try
                    {
                        UnloadModule(s, b);
                        foreach (var src in Sources)
                        {
                            if (src.Name == item.OriginalSource)
                            {
                                for (int i = 0; i < src.PackageFile.Count; i++)
                                {

                                    if (src.PackageName[i].ToUpper() == s.ToUpper())
                                    {
                                        var Url = src.PackageFile[i];
                                        double p = 0.0;
                                        Console.WriteLine("Downloading package...");
                                        LiteManagedHttpDownload.Downloader.DownloadToFileWithProgressBuffered(Url, Path.Combine(CurrentModuleDir.FullName, "Caches", $"{src.PackageName[i]}-{src.PackageVersion[i]}-{src.Name}.pkg"), ref p, 10240);
                                        DeployModule(Path.Combine(CurrentModuleDir.FullName, "Caches", $"{src.PackageName[i]}-{src.PackageVersion[i]}-{src.Name}.pkg"), false);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("Complete");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        return;

                                    }
                                }
                                return;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed.");
                        Debugger.currentDebugger.Log("" + e.ToString(), MessageType.Error);
                    }
                }
            }
        }
        void UpdateAllModules(string s, bool b)
        {
            foreach (var item in PackagesToUpdate)
            {
                UpdateModule(item.Name, b);
            }
        }
    }
}
