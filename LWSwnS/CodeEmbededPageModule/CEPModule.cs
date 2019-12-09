using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using LWSwnS.WebPage;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CodeEmbededPageModule
{
    public class CEPModule : ExtModule
    {
        string RootDir;
        public static Version ModuleVersion = new Version(0, 0, 1, 0);
        List<Assembly> refs = new List<Assembly>();
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            RootDir = new FileInfo(Assembly.GetAssembly(this.GetType()).Location).Directory.FullName;
            moduleDescription.Name = "CEPModule";
            
            moduleDescription.version = ModuleVersion;
            {
                UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
                try
                {
                    config = UniversalConfigurationMark2.LoadFromFile("./Configs/CEP.References.ini");
                }
                catch (Exception)
                {
                }
                foreach (var item in config.GetValues("References"))
                {
                    refs.Add(Assembly.LoadFrom((new FileInfo(item)).FullName));
                }
            }
            WebServer.AddExemptFileType("cehtml");
            WebServer.AddExemptFileType("cep");
            {
                Debugger.currentDebugger.Log("Initing Roslyn runtime...");
                try
                {
                    Assembly.LoadFrom(Path.Combine(RootDir, "Microsoft.CodeAnalysis.CSharp.dll"));
                }
                catch (Exception)
                {
                    Debugger.currentDebugger.Log("Unable to init roslyn runtime. Is all DLL files exist?", MessageType.Error);
                }
                try
                {
                    Assembly.LoadFrom(Path.Combine(RootDir, "Microsoft.CodeAnalysis.CSharp.Scripting.dll"));
                }
                catch (Exception)
                {
                    Debugger.currentDebugger.Log("Unable to init roslyn runtime. Is all DLL files exist?", MessageType.Error);
                }
                try
                {

                    Assembly.LoadFrom(Path.Combine(RootDir, "Microsoft.CodeAnalysis.dll"));
                }
                catch (Exception)
                {
                    Debugger.currentDebugger.Log("Unable to init roslyn runtime. Is all DLL files exist?", MessageType.Error);
                }
                try
                {
                    
                    Assembly.LoadFrom(Path.Combine(RootDir, "Microsoft.CodeAnalysis.Scripting.dll"));
                }
                catch (Exception)
                {
                    Debugger.currentDebugger.Log("Unable to init roslyn runtime. Is all DLL files exist?", MessageType.Error);
                }
                try
                {
                    CSharpScript.EvaluateAsync("LWSwnS.Diagnostic.Debugger.currentDebugger.Log(\"Init completed.\");",
                    ScriptOptions.Default.AddReferences(Assembly.GetAssembly(typeof(Debugger)))).Wait();

                }
                catch (Exception e)
                {
                    Debugger.currentDebugger.Log("Unable to init roslyn runtime." + e.Message, MessageType.Error);
                }
            }
            refs.Add(Assembly.GetAssembly(typeof(LWSwnS.Core.HttpServer)));
            EventHandler<HttpRequestData> eventHandler = (a, b) =>
            {
                var url = b.requestUrl.Split('?');
                if (url[0].ToUpper().EndsWith(".CEHTML") || url[0].ToUpper().EndsWith(".CEP"))
                {
                    try
                    {

                        Debugger.currentDebugger.Log("Running on CEP");
                        var p = URLConventor.Convert(url[0]);
                        CodeEmbededPage codeEmbededPage = new CodeEmbededPage(p);
                        var e = codeEmbededPage.ExecuteAndRetire(refs.ToArray());
                        e.Wait();
                        HttpResponseData httpResponseData = new HttpResponseData();
                        httpResponseData.content = Encoding.UTF8.GetBytes(e.Result);
                        httpResponseData.Send(ref b.streamWriter);
                        b.Cancel = true;
                    }
                    catch (Exception e)
                    {
                        Debugger.currentDebugger.Log("Error on CEP:"+e.Message, MessageType.Error);
                        var p = URLConventor.Convert(url[0]);
                        HttpResponseData httpResponseData = new HttpResponseData();
                        httpResponseData.content = Encoding.UTF8.GetBytes(e.Message);
                        httpResponseData.Send(ref b.streamWriter);
                        b.Cancel = true;
                    }
                }
            };
            WebServer.AddHttpRequestHandler(eventHandler);
            return moduleDescription;
        }
    }
}
