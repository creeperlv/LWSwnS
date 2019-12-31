﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace LWSwnS.Api.Shell.Local
{
    public class LocalShell
    {
        public static Dictionary<string, Dictionary<string,Action<string>>> Commands { get; internal set; } = new Dictionary<string, Dictionary<string, Action<string>>>();
        public static void Register(string cmdName,Action<string> action)
        {

            StackTrace stack = new StackTrace(true);
            var f = stack.GetFrame(1);
            var file = new FileInfo(Assembly.GetAssembly(f.GetMethod().DeclaringType).Location);

            if (!Commands.ContainsKey(file.Name))
            {
                Commands.Add(file.Name, new Dictionary<string, Action<string>>());
            }
            Commands[file.Name].Add(cmdName, action);
        }
        public static void Invoke(string cmd)
        {
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(cmd);
            ApiManager.Functions["LOCAL-SHELL-INVOKE"](uniParamater);
        }
    }
}
