using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PowerShellModule
{
    public class WebCore : ExtModule
    {
        public ModuleDescription InitModule()
        {
            ModuleDescription module = new ModuleDescription();
            module.Name = "PowerShellModule-Web";
            module.version = new Version(0, 0, 1, 0);
            WebServer.AddIgnoreUrlPrefix(VariablesPool.config.Get("WebHost", "/PS"));
            string HostHTML = File.ReadAllText("./Modules/PowerShellModule/netcoreapp3.0/Template.html");
            string ItemHTML = File.ReadAllText("./Modules/PowerShellModule/netcoreapp3.0/ScriptItem.html");
            EventHandler<HttpRequestData> a = (object sender, HttpRequestData b) =>
            {
                if (b.requestUrl.Trim().ToUpper().StartsWith(VariablesPool.config.Get("WebHost", "/PS")))
                {
                    HttpResponseData httpResponseData = new HttpResponseData();
                    string items = "";
                    if (VariablesPool.PSInstances.Count == 0)
                    {
                        string itemHTML = ItemHTML.Replace("[ScriptName]", "No Script Running").Replace("[ScriptParameter]", "").Replace("[StatusText]", "").Replace("[StatusColor]", "#2288EE");
                        items += itemHTML;
                    }
                    string htmlbody = HostHTML;
                    foreach (var item in VariablesPool.PSInstances)
                    {
                        string itemHTML = ItemHTML.Replace("[ScriptName]", item.file).Replace("[ScriptParameter]", item.para).Replace("[StatusText]",
                            !item.isCompleted ? "Running" : "Stopped").Replace("[StatusColor]", !item.isCompleted ? "#00AA00" : "#2288EE");
                        items += itemHTML;
                    }
                    b.Cancel = true;
                    httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                    httpResponseData.content = Encoding.UTF8.GetBytes(htmlbody.Replace("[ScriptList]", items));
                    httpResponseData.Send(ref b.streamWriter);
                }
            };
            WebServer.AddHttpRequestHandler(a);
            return module;
        }
    }
}
