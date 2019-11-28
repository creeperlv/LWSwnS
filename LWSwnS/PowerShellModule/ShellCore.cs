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
            moduleDescription.version = new Version(0, 0, 1, 0);
            CommandHandler.RegisterCommand("powershell-execute", (a, b, c) =>
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
            });
            return moduleDescription;
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
                    if(parameter!="")
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
