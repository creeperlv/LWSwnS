using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Diagnostic;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace AdvancedModuleManagement
{
    public class MainEntrance : ExtModule
    {
        public readonly static Version version = new Version(0, 0, 1, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "AdvModMan";
            moduleDescription.version = version;
            LocalShell.Register("install-module", (s, b) => { 
            //3 Stage:
            //  1. Find from source
            //  2. Download Package (A Zip File)
            //  3. Unzip the package.
            });
            LocalShell.Register("update-module", (s, b) => { 
            });
            LocalShell.Register("check-update", (s, b) => { 
            });
            LocalShell.Register("uninstall-module", (s, b) => { 
            });
            Debugger.currentDebugger.Log("Loading modules...");
            var thisFile=new FileInfo(Assembly.GetAssembly(typeof(MainEntrance)).Location);
            var directory = thisFile.Directory;
            return moduleDescription;
        }
    }
}
