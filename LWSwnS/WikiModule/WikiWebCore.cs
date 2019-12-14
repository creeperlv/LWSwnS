using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WikiModule
{
    public class WikiWebCore : ExtModule
    {
        string PageTemplate = "";
        string PageListItemTemplate = "";
        string MobilePageTemplate = "";
        string rootDir;
        UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
        static Version ModuleVersion = new Version(0, 0, 1, 0);
        public ModuleDescription InitModule()
        {
            rootDir = new FileInfo(Assembly.GetAssembly(this.GetType()).Location).Directory.FullName;

            WebServer.AddIgnoreUrlPrefix("/WIKI");
            EventHandler<HttpRequestData> eventHandler = (a, b) =>
            {
                if (b.requestUrl.ToUpper().StartsWith("/WIKI"))
                {
                    b.Cancel = true;
                    Debugger.currentDebugger.Log("Requested wiki:" + b.requestUrl);
                    HttpResponseData httpResponseData = new HttpResponseData();
                    var urlgrp = b.requestUrl.Split('?');
                    //if(urlgrp[0].ToUpper().to)
                    string response = "";
                    string finalTitle = "";
                    FileInfo file = null;
                    try
                    {
                        if (urlgrp[0].ToUpper().EndsWith("MD"))
                        {
                            Debugger.currentDebugger.Log("Requested wiki:" + b.requestUrl + " is a file.");
                            //PAGETITLE
                            file = GetFileFromURL(urlgrp[0].Substring(1));
                            var content = File.ReadAllLines(file.FullName).ToList();
                            var realContent = "";
                            var title = "" + content[0];
                            finalTitle = title;
                            //content.RemoveAt(0);
                            foreach (var item in content)
                            {
                                if (realContent == "")
                                {
                                    realContent = item;
                                }
                                realContent += Environment.NewLine + item;
                            }

                            response = PageTemplate.Replace("[Content]", Markdown.ToHtml(realContent));
                        }else if(urlgrp[0].ToUpper().EndsWith("PNG")|| urlgrp[0].ToUpper().EndsWith("JPG")|| urlgrp[0].ToUpper().EndsWith("GIF")|| urlgrp[0].ToUpper().EndsWith("BMP")|| urlgrp[0].ToUpper().EndsWith("MP4")|| urlgrp[0].ToUpper().EndsWith("WEBP"))
                        {
                            HttpResponseData binResponse = new HttpResponseData();
                            binResponse.Additional = "Application/Binary";
                            var RealUrl = URLConventor.Convert(b.requestUrl.Trim());
                            var fi = GetFileFromURL(RealUrl);
                            using (var fs = fi.OpenRead())
                            {
                                binResponse.SendFile(ref b.streamWriter, fs);
                            }
                        }
                        else if (DirectoryExist(urlgrp[0].Substring(1)))
                        {
                            httpResponseData.StatusLine = "HTTP/1.1 307";
                            response = PageTemplate.Replace("[Content]", "Redirect...");
                            string redirectUrl = urlgrp[0].Substring(1);
                            if (redirectUrl.EndsWith("/"))
                            {

                            }
                            else
                            {
                                redirectUrl += "/";
                            }
                            httpResponseData.Additional = $"Location: {redirectUrl}Index.md";
                            file = new FileInfo($"./{urlgrp[0]}/Index.md");
                            var content = File.ReadAllLines(file.FullName).ToList();
                            var realContent = "";
                            var title = "" + content[0];
                            finalTitle = title;
                            //content.RemoveAt(0);
                            foreach (var item in content)
                            {
                                if (realContent == "")
                                {
                                    realContent = item;
                                }
                                realContent += Environment.NewLine + item;
                            }
                            response = PageTemplate.Replace("[Content]", Markdown.ToHtml(realContent));
                        }


                    }
                    catch (Exception e)
                    {
                        Debugger.currentDebugger.Log(e.Message, MessageType.Error);
                        response = PageTemplate.Replace("[Content]", e.Message);
                    }
                    Debugger.currentDebugger.Log("Resolving List...");
                    string ListContent = "";
                    finalTitle = (config.GetValues("Title").Count == 0 ? "Default Wiki" : config.GetValues("Title")[0]) + " - " + finalTitle;
                    try
                    {

                        {
                            //Generate List.
                            bool usingStaticList = false;
                            //usingStaticList = config.GetValues("useStaticList").Count == 0 ? false : bool.Parse(config.GetValues("useStaticList")[0]);
                            List<FileInfo> listFiles = new List<FileInfo>();
                            if (usingStaticList == true)
                            {
                                //listFiles =config.GetValues("StaticList");
                            }
                            else
                            {
                                if (file != null)
                                {
                                    foreach (var item in file.Directory.EnumerateFiles())
                                    {
                                        listFiles.Add(item);
                                    }
                                }
                            }
                            foreach (var item in listFiles)
                            {

                                ListContent += PageListItemTemplate.Replace("[URL]",  item.Name).Replace("[NAME]", File.ReadLines(item.FullName).First());
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        Debugger.currentDebugger.Log(e.Message, MessageType.Error);
                    }
                    response = response.Replace("[PAGETITLE]", finalTitle).Replace("[LINKS]", ListContent);
                    //response = "FUCK!";
                    httpResponseData.content = Encoding.UTF8.GetBytes(response);
                    httpResponseData.Send(ref b.streamWriter);
                    Debugger.currentDebugger.Log("Data sent.");
                }
            };
            WebServer.AddHttpRequestHandler(eventHandler);
            Task.Run(async () =>
            {
                Debugger.currentDebugger.Log("Automatic Configuration Reload task initialized.");
                while (true)
                {
                    Load();
                    await Task.Delay(5000);
                }
            });
            {
                ModuleDescription description = new ModuleDescription();
                description.Name = "Wiki-Module-Web";
                description.version = ModuleVersion;
                return description;
            }
        }
        bool DirectoryExist(string location)
        {
            var paths = location.Split('/');
            DirectoryInfo directoryInfo = new DirectoryInfo("./");
            for (int i = 0; i < paths.Length; i++)
            {
                if (i != paths.Length - 1)
                {
                    bool find = false;
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                        {
                            find = true;
                            directoryInfo = item;
                            break;
                        }
                    }
                    if(find==false)return false;
                }
                else
                {
                    if (paths[i] == "")
                    {
                        return true;
                    }
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                            return true;
                    }
                }
            }
            return false;
        }
        DirectoryInfo GetFolderFromURL(string location)
        {
            var paths = location.Split('/');
            DirectoryInfo directoryInfo = new DirectoryInfo("./");
            for (int i = 0; i < paths.Length; i++)
            {
                if (i != paths.Length - 1)
                {
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                        {
                            directoryInfo = item;
                            break;
                        }
                    }
                    
                }
                else
                {
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                            return item;
                    }
                }
            }
            throw new Exception("404,File not found!");
        }
        FileInfo GetFileFromURL(string location)
        {
            var paths = location.Split('/');
            DirectoryInfo directoryInfo = new DirectoryInfo("./");
            for (int i = 0; i < paths.Length; i++)
            {
                if (i != paths.Length - 1)
                {
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                        {
                            directoryInfo = item;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var item in directoryInfo.GetFiles())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper()) 
                            return item;
                    }
                }
            }
            throw new Exception("404,File not found!");
        }
        void Load()
        {
            try
            {
                config = UniversalConfigurationMark2.LoadFromFile("./Configs/WikiModule.ini");

            }
            catch (Exception)
            {
            }
            PageTemplate = File.ReadAllText(Path.Combine(rootDir, "WikiPage.html"));
            PageListItemTemplate = File.ReadAllText(Path.Combine(rootDir, "PageListItemTemplate.html"));
        }
    }
    public class FirstRun : FirstInit
    {
        public void Init()
        {
            try
            {

                Directory.CreateDirectory("./Wiki");

            }
            catch (Exception)
            {
            }
            try
            {
                File.Create("./Wiki/Index.md").Close();
            }
            catch (Exception)
            {
            }
            try
            {
                string content = @"Home Page
===
# Welcome to new Wiki site!
All wiki files are in './Wiki/'.
File format:
> [Title]
>
>===
>
>[Content]
[Title] is very
";
                File.WriteAllText("./Wiki/Index.md", content);
            }
            catch (Exception)
            {
            }
        }
    }
}
