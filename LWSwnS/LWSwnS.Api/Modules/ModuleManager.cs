using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LWSwnS.Api.Modules
{
    public class ModuleManager
    {
        public static List<ModuleDescription> ExtModules = new List<ModuleDescription>();
        public static void InitModule(string location)
        {
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(location);
            ApiManager.Functions["MODULE_INIT"](uniParamater);
        }
        public static List<ModuleDescription> LoadModule(string location)
        {
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(location);
            return ApiManager.Functions["MODULE_LOAD"](uniParamater).Data as List<ModuleDescription>;
        }
    }
}
