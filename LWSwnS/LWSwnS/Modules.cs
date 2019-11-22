using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace LWSwnS
{
    public class Modules : AssemblyLoadContext
    {
        private AssemblyDependencyResolver resolver;
        public Modules(string path)
        {
            resolver = new AssemblyDependencyResolver(path);
        }
        protected override Assembly Load(AssemblyName name)
        {
            string a = resolver.ResolveAssemblyToPath(name);
            return a!=null?LoadFromAssemblyPath(a):null;
        }
    }
}
