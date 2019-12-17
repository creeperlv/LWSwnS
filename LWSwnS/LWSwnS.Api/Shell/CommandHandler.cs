using LWSwnS.Api.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace LWSwnS.Api.Shell
{
    public class CommandHandler
    {
        public static void RegisterCommand(string CommandName, Func<String, Object, StreamWriter, bool> cmd)
        {
            StackTrace stack = new StackTrace(true);
            var f = stack.GetFrame(1);
            var file = new FileInfo(Assembly.GetAssembly(f.GetMethod().DeclaringType).Location);
            if (!ModuleManager.shellCMDS.ContainsKey(file.FullName))
            {
                ModuleManager.shellCMDS.Add(file.FullName, new List<string>());
            }
            ModuleManager.shellCMDS[file.FullName].Add(CommandName);
            UniParamater paramater = new UniParamater();
            paramater.Add(CommandName);
            paramater.Add(cmd);
            ApiManager.Functions["REGCMD"](paramater);
        }
    }
}
