using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Api.Shell
{
    public class CommandHandler
    {
        public static void RegisterCommand(string CommandName, Action<String, Object, StreamWriter> cmd)
        {
            UniParamater paramater = new UniParamater();
            paramater.Add(CommandName);
            paramater.Add(cmd);
            ApiManager.Functions["REGCMD"](paramater);
        }
    }
}
