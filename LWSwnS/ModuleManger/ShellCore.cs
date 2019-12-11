using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using System;
using System.IO;

namespace ModuleManger
{
    public class ShellCore : ExtModule
    {
        public static Version ModuleVersion = new Version(0, 0, 1, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription description = new ModuleDescription();
            description.Name = "ModuleManager";
            description.version = ModuleVersion;
            LWSwnS.Api.Shell.CommandHandler.RegisterCommand("ModuleManager:Init-Module", InitModule);
            return description;
        }
        bool InitModule(string a, object b, StreamWriter c)
        {
            ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
            try
            {
                ModuleManager.InitModule(a);
            }
            catch (Exception e)
            {
                shellFeedbackData.StatusLine = "Error:";
                shellFeedbackData.DataBody = e.Message;
            }
            shellFeedbackData.writer = c;
            shellFeedbackData.SendBack();
            return true;
        }
        bool LoadModule(string a, object b, StreamWriter c)
        {
            ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
            try
            {
                foreach (var item in ModuleManager.LoadModule(a))
                {

                    ModuleManager.ExtModules.Add(item);
                }
            }
            catch (Exception e)
            {
                shellFeedbackData.StatusLine = "Error:";
                shellFeedbackData.DataBody = e.Message;
            }
            shellFeedbackData.writer = c;
            shellFeedbackData.SendBack();
            return true;
        }

    }
}
