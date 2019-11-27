﻿using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string FileItemHTML = File.ReadAllText("./Modules/PowerShellModule/netcoreapp3.0/ScriptFileItem.html");
            EventHandler<HttpRequestData> a = (object sender, HttpRequestData b) =>
            {
                if (b.requestUrl.Trim().ToUpper().StartsWith(VariablesPool.config.Get("WebHost", "/PS")))
                {
                    var url = b.requestUrl.Substring("/PS".Length);

                    HttpResponseData httpResponseData = new HttpResponseData();
                    if (url == "/" || url == "")
                    {

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
                        httpResponseData.content = Encoding.UTF8.GetBytes(htmlbody.Replace("[ScriptList]", items));
                    }else if (url.ToUpper().StartsWith("/ScriptList".ToUpper()))
                    {

                        string items = "";
                        DirectoryInfo pss = new DirectoryInfo("./PSScripts/");
                        var fs = pss.EnumerateFiles("*.ps1").ToList();
                        if (fs.Count== 0)
                        {
                            string itemHTML = FileItemHTML.Replace("[ScriptName]", "No Scripts").Replace("[ScriptParameter]", "").Replace("[ScriptSize]", "");
                            items += itemHTML;
                        }
                        string htmlbody = HostHTML;
                        foreach (var item in fs)
                        {
                            string itemHTML = FileItemHTML.Replace("[ScriptName]", item.Name).Replace("[ScriptSize]", (double)item.Length/1024.0+"KB");
                            items += itemHTML;
                        }
                        httpResponseData.content = Encoding.UTF8.GetBytes(htmlbody.Replace("[ScriptList]", items));
                    }else if (url.ToUpper().StartsWith("/View-Script:".ToUpper()))
                    {

                        string items = "";
                        DirectoryInfo pss = new DirectoryInfo("./PSScripts/");
                        var fs = pss.EnumerateFiles("*.ps1").ToList();
                        if (fs.Count== 0)
                        {
                            string itemHTML = FileItemHTML.Replace("[ScriptName]", "No Scripts").Replace("[ScriptParameter]", "").Replace("[ScriptSize]", "");
                            items += itemHTML;
                        }
                        string htmlbody = HostHTML;
                        foreach (var item in fs)
                        {
                            string itemHTML = FileItemHTML.Replace("[ScriptName]", item.Name).Replace("[ScriptSize]", (double)item.Length/1024.0+"KB");
                            items += itemHTML;
                        }
                        httpResponseData.content = Encoding.UTF8.GetBytes(htmlbody.Replace("[ScriptList]", items));
                    }
                    b.Cancel = true;
                    httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                    httpResponseData.Send(ref b.streamWriter);
                }
            };
            WebServer.AddHttpRequestHandler(a);
            return module;
        }
    }
}
