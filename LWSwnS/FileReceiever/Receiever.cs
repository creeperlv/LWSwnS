using LWSwnS.Api.Modules;
using System;

namespace FileReceiever
{
    public class Receiever : ExtModule
    {
        public static Version ModuleVersion = new Version(0, 0, 1, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "FileReceieverModule";
            moduleDescription.version = ModuleVersion;
            return moduleDescription;
        }
    }
}
