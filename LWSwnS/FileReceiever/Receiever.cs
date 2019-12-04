using LWSwnS.Api.Modules;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FileReceiever
{
    public class Receiever : ExtModule
    {
        public TcpListener listener;
        public static Version ModuleVersion = new Version(0, 0, 1, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "FileReceieverModule";
            moduleDescription.version = ModuleVersion;
            Task.Run(MainListener);
            return moduleDescription;
        }
        public void MainListener()
        {
        }
    }
}
