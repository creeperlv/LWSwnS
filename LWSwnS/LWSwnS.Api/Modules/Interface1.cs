using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LWSwnS.Api.Modules
{
    public interface ExtModule
    {
        void InitModule();
    }
    public class ModuleDescription
    {
        public string Name;
        public Assembly targetAssembly;
    }
}
