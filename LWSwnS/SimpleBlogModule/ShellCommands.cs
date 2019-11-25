using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell;
using LWSwnS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleBlogModule
{
    public class ShellCommands : ExtModule
    {
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "SimpleBlog-ShellCommands";
            moduleDescription.version = new Version(0, 0, 1, 0);
            CommandHandler.RegisterCommand("SimpleBlog:Add-Post", (string fileName, object content, StreamWriter writer) =>
            {
                string FilePath = Path.Combine(new DirectoryInfo("./Padth").FullName, fileName);
                if (!File.Exists(FilePath)) {
                    File.Create(FilePath).Close();
                }
                File.WriteAllText(FilePath, (string)content);
                ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                shellFeedbackData.writer = writer;
                shellFeedbackData.SendBack();
                return true;
            });
            return moduleDescription;
        }
    }
}
