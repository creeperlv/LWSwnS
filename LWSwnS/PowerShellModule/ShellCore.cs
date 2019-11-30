using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell;
using LWSwnS.Configuration;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading.Tasks;

namespace PowerShellModule
{
    public class ShellCore : ExtModule
    {
        public static Version ShellVersion=new Version(0, 0, 2, 0);
        ModuleDescription ExtModule.InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            try
            {
                VariablesPool.config = UniversalConfigurationLoader.LoadFromFile("./psmodule.ini");
            }
            catch (Exception)
            {
            }
            moduleDescription.Name = "PowerShellModule-Shell";
            moduleDescription.version = ShellVersion;
            CommandHandler.RegisterCommand("powershell-execute", PSE);
            CommandHandler.RegisterCommand("ps-e", PSE);
            CommandHandler.RegisterCommand("powershell-add-script", AddScript);
            CommandHandler.RegisterCommand("ps-a-s", AddScript);
            CommandHandler.RegisterCommand("powershell-del-script", DelScript);
            CommandHandler.RegisterCommand("ps-d-s", DelScript);
            return moduleDescription;
        }
        bool DelScript(string a, object b, StreamWriter c)
        {
            ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
            try
            {
                File.Delete("./PSScripts/" + a);
                shellFeedbackData.StatusLine = "OK";
            }
            catch (Exception e)
            {
                shellFeedbackData.StatusLine = "ERROR:" + e.Message;
            }
            shellFeedbackData.writer = c;
            shellFeedbackData.SendBack();
            return true;
        }
        bool AddScript(string a, object b, StreamWriter c)
        {
            if (!File.Exists("./PSScripts/" + a)) File.Create("./PSScripts/" + a).Close();
            File.WriteAllText("./PSScripts/" + a, b.ToString());
            ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
            shellFeedbackData.StatusLine = "OK";
            shellFeedbackData.writer = c;
            shellFeedbackData.SendBack();
            return true;
        }
        bool PSE(string a, object b, StreamWriter c)
        {
            Console.WriteLine("Execution Accepted");
            var script = a;
            var parameter = "";
            if (script.IndexOf(" ") > 0)
            {
                script = script.Substring(0, script.IndexOf(" "));
                parameter = a.Substring(a.IndexOf(" ") + 1);
            }
            PSInstance pSInstance = new PSInstance(script, parameter);
            VariablesPool.PSInstances.Add(pSInstance);
            string result = "";
            c.BaseStream.WriteTimeout = 30000;
            Task.Run(async () =>
            {
                Console.WriteLine("Starting...");
                try
                {

                    result = pSInstance.Invoke(script, parameter);

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error:" + e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine("Invoke completed");
                await Task.Delay(3000);
                VariablesPool.PSInstances.Remove(pSInstance);
            }).Wait();
            ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
            shellFeedbackData.DataBody = result;
            shellFeedbackData.writer = c;
            shellFeedbackData.SendBack();
            return true;

        }
    }
    public class PSInstance
    {
        public string file;
        public string ResultContent;
        public string para;
        public bool isCompleted = false;
        public PSInstance(string file, string para)
        {
            this.file = file;
            this.para = para;
        }
        public string Invoke(string script, string parameter)
        {
            Console.WriteLine("Evaluating PSScript");
            string Result = "";
            try
            {
                PowerShell ps = PowerShell.Create();
                {
                    var scriptHome = VariablesPool.config.Get("ScriptHome", "./PSScripts/");
                    ps = ps.AddScript(File.ReadAllText(scriptHome + script));
                    if (parameter != "")
                        ps = ps.AddParameter(parameter);
                    var results = ps.Invoke();
                    isCompleted = true;
                    foreach (var item in results)
                    {
                        if (Result == "")
                        {
                            Result += item;
                        }
                        else
                        {
                            Result += Environment.NewLine + item;
                        }
                    }
                    ResultContent = Result;
                    Console.WriteLine(Result);
                }
                ps.Dispose();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error:" + e.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            return Result;
        }
    }
}
