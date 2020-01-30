using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell;
using System;
using System.Diagnostics;
using System.IO;

namespace RemoteShell
{
    public class RShellCore : ExtModule
    {
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "RemoteShell";
            moduleDescription.version = new Version(1, 0, 0, 0); ;

            CommandHandler.RegisterCommand("shell", ShellExe);
            return moduleDescription;
        }
        bool ShellExe(string a, object b, StreamWriter c)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(a);
            if(a.IndexOf(" ") > 0)
            {
                processStartInfo.FileName = a.Substring(0, a.IndexOf(" "));
                processStartInfo.Arguments= a.Substring(a.IndexOf(" ")+1);
            }
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.CreateNoWindow = true;
            var p=Process.Start(processStartInfo);
            string result="";
            while (p.HasExited)
            {
                try
                {
                    if (result == "")
                    {
                        result = p.StandardOutput.ReadLine();
                    }
                    else
                    {
                        result +=Environment.NewLine+ p.StandardOutput.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    c.WriteLine("Error:"+e);
                }
            }
            ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
            shellFeedbackData.StatusLine = "OK";
            shellFeedbackData.DataBody = result;
            shellFeedbackData.writer = c;
            shellFeedbackData.SendBack();
            return true;
        }
    }
}
