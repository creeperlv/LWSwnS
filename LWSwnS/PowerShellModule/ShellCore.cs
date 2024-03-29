﻿using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System;
using System.Diagnostics;
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
        public static Version ShellVersion = new Version(0, 1, 3, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            Load();
            Tasks.RegisterTask(Load, Tasks.TaskType.Every30Seconds);
            moduleDescription.Name = "PowerShellModule-Shell";
            moduleDescription.version = ShellVersion;
            LocalShell.Register("SetNativePS", (s,t) =>
            {
                try
                {
                    bool b = bool.Parse(s);
                    if (VariablesPool.config.ContainsKey("UseNativePS")) VariablesPool.config["UseNativePS"] = b + ""; else VariablesPool.config.Add("UseNativePS", b + "");
                    VariablesPool.config.SaveToFile("./Configs/psmodule.ini");
                }
                catch (Exception)
                {
                }
            });
            LocalShell.Register("SetNativePreviewPS", (s, t) =>
            {
                try
                {
                    bool b = bool.Parse(s);
                    if (VariablesPool.config.ContainsKey("UseNativePreviewPS")) VariablesPool.config["UseNativePreviewPS"] = b + ""; else VariablesPool.config.Add("UseNativePreviewPS", b + "");
                    VariablesPool.config.SaveToFile("./Configs/psmodule.ini");
                }
                catch (Exception)
                {
                }
            });
            LocalShell.Register("SetDestoryDelay", (s, t) =>
            {
                try
                {
                    int d = int.Parse(s);
                    if (VariablesPool.config.ContainsKey("DestoryResultDelay")) VariablesPool.config["DestoryResultDelay"] = d + ""; else VariablesPool.config.Add("DestoryResultDelay", d + "");
                    VariablesPool.config.SaveToFile("./Configs/psmodule.ini");
                }
                catch (Exception)
                {
                }
            });
            CommandHandler.RegisterCommand("powershell-execute", PSE);
            CommandHandler.RegisterCommand("ps-e", PSE);
            CommandHandler.RegisterCommand("powershell-add-script", AddScript);
            CommandHandler.RegisterCommand("ps-a-s", AddScript);
            CommandHandler.RegisterCommand("powershell-del-script", DelScript);
            CommandHandler.RegisterCommand("ps-d-s", DelScript);
            return moduleDescription;
        }
        void Load()
        {
            try
            {
                VariablesPool.config = UniversalConfigurationLoader.LoadFromFile("./Configs/psmodule.ini");
                WebServer.AddIgnoreUrlPrefix(VariablesPool.config.Get("WebHost", "/PS"));
            }
            catch (Exception)
            {
            }
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
                LWSwnS.Diagnostic.Debugger.currentDebugger.Log("Starting Script:" + a);
                try
                {

                    result = pSInstance.Invoke(script, parameter);

                }
                catch (Exception e)
                {
                    LWSwnS.Diagnostic.Debugger.currentDebugger.Log(e.Message, LWSwnS.Diagnostic.MessageType.Error);
                }
                LWSwnS.Diagnostic.Debugger.currentDebugger.Log("Invoke completed");
                int Delay = 15000;
                try
                {
                    Delay = int.Parse(VariablesPool.config.Get("DestoryResultDelay", "15000"));
                }
                catch (Exception)
                {
                }
                await Task.Delay(Delay);
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
            LWSwnS.Diagnostic.Debugger.currentDebugger.Log("Evaluating PSScript");
            string Result = "";
            try
            {
                bool useNativeShell = false;
                try
                {
                    useNativeShell = bool.Parse(VariablesPool.config.Get("UseNativePS", "False"));
                }
                catch
                {
                }
                if (useNativeShell == false)
                {

                    PowerShell ps = PowerShell.Create(RunspaceMode.NewRunspace);
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
                        LWSwnS.Diagnostic.Debugger.currentDebugger.Log(Result);
                    }
                    ps.Dispose();
                }
                else
                {
                    bool usePreview = false;
                    try
                    {
                        usePreview = bool.Parse(VariablesPool.config.Get("UseNativePreviewPS", "False"));
                    }
                    catch
                    {
                    }
                    ProcessStartInfo startInfo;
                    if (usePreview == false)
                    {
                        startInfo = new ProcessStartInfo("pwsh");
                    }
                    else
                    {
                        startInfo = new ProcessStartInfo("pwsh-preview");
                    }

                    var scriptHome = VariablesPool.config.Get("ScriptHome", "./PSScripts/");
                    var scriptF = Path.Combine(scriptHome, script);
                    startInfo.Arguments = scriptF + " " + parameter;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    var p = Process.Start(startInfo);
                    string s;
                    while (p.HasExited)
                    {
                        if ((s = p.StandardOutput.ReadLine()) != null)
                        {

                            if (Result == "")
                            {
                                Result += s;
                            }
                            else
                            {
                                Result += Environment.NewLine + s;
                            }
                        }
                    }
                    isCompleted = true;
                    LWSwnS.Diagnostic.Debugger.currentDebugger.Log(Result);
                }
            }
            catch (Exception e)
            {
                LWSwnS.Diagnostic.Debugger.currentDebugger.Log(e.Message, LWSwnS.Diagnostic.MessageType.Error);
            }
            return Result;
        }
    }
}
