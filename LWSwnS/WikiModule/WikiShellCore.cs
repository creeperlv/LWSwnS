using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WikiModule
{
    class WikiShellCore : ExtModule
    {
        public static readonly Version ModuleVersion = new Version(0, 0, 1, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.version = ModuleVersion;
            moduleDescription.Name = "Wiki-Shell-Module";
            {
                LWSwnS.Api.Shell.CommandHandler.RegisterCommand("Update-Wiki", UpdateWiki);
                LWSwnS.Api.Shell.CommandHandler.RegisterCommand("Remove-Wiki", RemoveWiki);
            }
            return moduleDescription;
        }
        public bool UpdateWiki(string a,object b,StreamWriter writer)
        {
            string path=Path.Combine("./Wiki/", a);
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            else
            {
                File.WriteAllText(path, b as string);
            }
            ShellFeedbackData data = new ShellFeedbackData();
            data.writer = writer;
            data.StatusLine = "OK";
            data.SendBack();
            return true;
        }
        public bool RemoveWiki(string a,object b,StreamWriter writer)
        {
            string path = Path.Combine("./Wiki/", a);
            File.Delete(path);
            ShellFeedbackData data = new ShellFeedbackData();
            data.writer = writer;
            data.StatusLine = "OK";
            data.SendBack();
            return true;
        }
    }
}
