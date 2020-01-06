using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Configuration;
using LWSwnS.Diagnostic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace AdvancedModuleManagement
{
    public class MainEntrance : ExtModule
    {
        public readonly static Version version = new Version(0, 0, 1, 0);
        public static Dictionary<DirectoryInfo, Package> InstalledModules = new Dictionary<DirectoryInfo, Package>();
        public static Dictionary<string, Package> ActivatedModules = new Dictionary<string, Package>();
        public static List<Source> Sources=new List<Source>();
        public static DirectoryInfo CurrentModuleDir;
        public static DirectoryInfo LWSwnSDir;
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "AdvModMan";
            moduleDescription.version = version;
            LocalShell.Register("install-module", InstallModule);
            LocalShell.Register("deploy-module", DeployModule);
            LocalShell.Register("update-module", (s, b) =>
            {
            });
            LocalShell.Register("activate-module", (s, b) =>
            {
            });
            LocalShell.Register("unload-module", (s, b) =>
            {
            });
            LocalShell.Register("deactivate-module", (s, b) =>
            {
            });
            LocalShell.Register("update-source", (s, b) =>
            {
            });
            LocalShell.Register("check-update", (s, b) =>
            {
            });
            LocalShell.Register("uninstall-module", (s, b) =>
            {
            });
            Debugger.currentDebugger.Log("Loading modules...");
            CurrentModuleDir = FileUtilities.GetFolderFromAssembly(typeof(MainEntrance));
            LWSwnSDir = FileUtilities.GetFolderFromAssembly(typeof(LWSwnS.Configuration.ConfigurationLoader));
            {
                string ModuleInstallationDir = Path.Combine(CurrentModuleDir.FullName, "AMM.Modules");
                DirectoryInfo directoryInfo = new DirectoryInfo(ModuleInstallationDir);
                foreach (var item in directoryInfo.EnumerateDirectories())
                {
                    if (item.EnumerateFiles("Package.manifest").ToArray().Length != 0)
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Package));
                        using (var a = item.EnumerateFiles("Package.manifest").First().OpenRead())
                        {
                            InstalledModules.Add(item, xmlSerializer.Deserialize(a) as Package);
                        }
                    }
                }
            }
            {
                string confF = Path.Combine(LWSwnSDir.FullName, "Configs", "AMM.ActivatedModules.ini");
                UniversalConfigurationMark2 conf = new UniversalConfigurationMark2();
                var a = conf.GetValues("PackageID", "[NULL]");
                if (a[0] != "[NULL]")
                {
                    foreach (var item in a)
                    {
                        foreach (var mod in InstalledModules)
                        {
                            if (mod.Value.ID.ToUpper() == item.Split(',')[0].ToUpper())
                            {
                                try
                                {
                                    if (item.Split(',').Length == 2)
                                    {
                                        if (mod.Value.Version.ToUpper() != item.Split(',')[1].ToUpper()) continue;
                                    }
                                    ModuleManager.InitModule(Path.Combine(mod.Key.FullName, mod.Value.MainDLL));
                                    ActivatedModules.Add(mod.Value.ID, mod.Value);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
            #region ReadSource
            {
                var SourceLstDir=new DirectoryInfo( Path.Combine(CurrentModuleDir.FullName, "Source.List.Cache"));
                foreach (var item in SourceLstDir.EnumerateFiles("*.src")) 
                {

                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Source));
                    using (var a = item.OpenRead())
                    {
                        Sources.Add(xmlSerializer.Deserialize(a) as Source);
                    }
                }
            }
            #endregion
            return moduleDescription;
        }
        void InstallModule(string s, bool b)
        {
            if (b == true)
            {
                if (LocalShell.RequireAuthCMD("Deploy-Module") == false)
                {
                    return;
                }
            }
            //foreach (var item in Source)
            //{

            //}
            DeployModule("", false);
        }
        void DeployModule(string s, bool b)
        {
            if (b == true)
            {
                if (LocalShell.RequireAuthCMD("Deploy-Module") == false)
                {
                    return;
                }
            }
            var target = Path.Combine(CurrentModuleDir.FullName, "AMM.Modules", new FileInfo(s).Name);
            var origin = new FileInfo(s);
            Debugger.currentDebugger.Log($"Extract \"{origin.FullName}\" to ");
            ZipFile.ExtractToDirectory(origin.FullName, target);
        }
    }
}
