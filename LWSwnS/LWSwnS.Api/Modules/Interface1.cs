using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LWSwnS.Api.Modules
{
    public interface ExtModule
    {
        ModuleDescription InitModule();
    }
    public class ModuleDescription
    {
        public string Name;
        public Version version = new Version(0,0,0,0);
        public Assembly targetAssembly;
    }
}
