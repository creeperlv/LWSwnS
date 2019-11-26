using LWSwnS.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellModule
{
    public class VariablesPool
    {
        public static UniversalConfiguration config = new UniversalConfiguration();
        public static List<PSInstance> PSInstances = new List<PSInstance>();
    }
}
