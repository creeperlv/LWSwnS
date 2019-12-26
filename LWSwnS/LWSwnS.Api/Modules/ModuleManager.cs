using LWSwnS.Api.Shell.Local;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace LWSwnS.Api.Modules
{
    public class ModuleManager
    {
        internal static Dictionary<string, List<object>> Handlers = new Dictionary<string, List<object>>();
        internal static Dictionary<string, List<string>> shellCMDS= new Dictionary<string, List<string>>();

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
            try
            {
                Tasks.Unregister(fileInfo.FullName);
            }
            catch (Exception)
            {
            }
            if (LocalShell.Commands.ContainsKey(fileInfo.Name))
            {
                LocalShell.Commands.Remove(fileInfo.Name);
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
    public class Tasks
    {
        public static Dictionary<string, List<Action>> TaskEvery5Seconds {  get; private set; } = new Dictionary<string, List<Action>>();
        public static Dictionary<string, List<Action>> TaskEvery10Seconds { get; private set; } = new Dictionary<string, List<Action>>();
        public static Dictionary<string, List<Action>> TaskEvery30Seconds { get; private set; } = new Dictionary<string, List<Action>>();
        public static void RegisterTask(Action a, TaskType taskType= TaskType.Every5Seconds)
        {
            string Identifier = "";
            StackTrace stack = new StackTrace(true);
            var f = stack.GetFrame(1);
            Identifier = new FileInfo(Assembly.GetAssembly(f.GetMethod().DeclaringType).Location).FullName;
            switch (taskType)
            {
                case TaskType.Every5Seconds:
                    {
                        if (!TaskEvery5Seconds.ContainsKey(Identifier))
                        {
                            TaskEvery5Seconds.Add(Identifier, new List<Action>());
                        }
                        TaskEvery5Seconds[Identifier].Add(a);
                    }
                    break;
                case TaskType.Every10Seconds:
                    {
                        if (!TaskEvery10Seconds.ContainsKey(Identifier))
                        {
                            TaskEvery10Seconds.Add(Identifier, new List<Action>());
                        }
                        TaskEvery10Seconds[Identifier].Add(a);
                    }
                    break;
                case TaskType.Every30Seconds:
                    {
                        if (!TaskEvery30Seconds.ContainsKey(Identifier))
                        {
                            TaskEvery30Seconds.Add(Identifier, new List<Action>());
                        }
                        TaskEvery30Seconds[Identifier].Add(a);
                    }
                    break;
                default:
                    break;
            }
        }
        public static void Unregister(string Identifier)
        {
            if (TaskEvery5Seconds.ContainsKey(Identifier))
            {
                TaskEvery5Seconds.Remove(Identifier);
            }
            if (TaskEvery10Seconds.ContainsKey(Identifier))
            {
                TaskEvery10Seconds.Remove(Identifier);
            }
            if (TaskEvery30Seconds.ContainsKey(Identifier))
            {
                TaskEvery30Seconds.Remove(Identifier);
            }
        }
        public enum TaskType
        {
            Every5Seconds,Every10Seconds,Every30Seconds
        }
    }
}
