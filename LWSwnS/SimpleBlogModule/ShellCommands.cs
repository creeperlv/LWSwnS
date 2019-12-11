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
            moduleDescription.version = new Version(0, 0, 2, 0);
            CommandHandler.RegisterCommand("SimpleBlog:Add-Post", (string fileName, object content, StreamWriter writer) =>
            {
                string FilePath = Path.Combine(new DirectoryInfo("./Posts").FullName, fileName);
                if (!File.Exists(FilePath)) {
                    File.Create(FilePath).Close();
                }
                File.WriteAllText(FilePath, (string)content);
                ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                shellFeedbackData.writer = writer;
                shellFeedbackData.SendBack();
                return true;
            });
            CommandHandler.RegisterCommand("SimpleBlog:Del-Post", (string fileName, object content, StreamWriter writer) =>
            {
                string FilePath = Path.Combine(new DirectoryInfo("./Posts").FullName, fileName);
                if (File.Exists(FilePath)) {
                    File.Delete(FilePath);
                }

                ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                shellFeedbackData.writer = writer;
                shellFeedbackData.SendBack();
                return true;
            });
            CommandHandler.RegisterCommand("SimpleBlog:Get-Post", (string fileName, object content, StreamWriter writer) =>
            {
                string FilePath = Path.Combine(new DirectoryInfo("./Posts").FullName, fileName);
                

                ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                if (File.Exists(FilePath))
                {
                    shellFeedbackData.DataBody = File.ReadAllText(FilePath);
                }
                else
                {
                    shellFeedbackData.StatusLine="FILE_NO_FOUND";
                }
                shellFeedbackData.writer = writer;
                shellFeedbackData.SendBack();
                return true;
            });
            return moduleDescription;
        }
    }
}
