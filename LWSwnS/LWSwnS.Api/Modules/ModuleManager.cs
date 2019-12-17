using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LWSwnS.Api.Modules
{
    public class ModuleManager
    {
        public static Dictionary<string, List<object>> Handlers = new Dictionary<string, List<object>>();
        public static Dictionary<string, List<string>> shellCMDS= new Dictionary<string, List<string>>();

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
        public static void UnloadModule(string ModuleFile)
        {
            FileInfo fileInfo = new FileInfo(ModuleFile);

            if (Handlers.ContainsKey(fileInfo.FullName))
            {
                var a = Handlers[fileInfo.FullName];
                foreach (var item in a)
                {

                    UniParamater uniParamater = new UniParamater();
                    uniParamater.Add(item);
                    ApiManager.Functions["RmOnReq"](uniParamater);
                }
            }
            if (shellCMDS.ContainsKey(fileInfo.FullName))
            {
                var a = Handlers[fileInfo.FullName];
                foreach (var item in a)
                {

                    UniParamater uniParamater = new UniParamater();
                    uniParamater.Add(item);
                    ApiManager.Functions["UNREGCMD"](uniParamater);
                }
            }
            {

                UniParamater uniParamater = new UniParamater();
                uniParamater.Add(ModuleFile);
                ApiManager.Functions["MODULE_UNLD"](uniParamater);

            }
        }
    }
}
