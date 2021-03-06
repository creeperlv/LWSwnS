﻿using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace DirectoryModule
{
    public class WebCore : ExtModule
    {
        public static readonly Version version = new Version(0, 0, 1, 0);
        UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
        string TemplatePage = "";
        string TemplateItem = "";
        String RootDir = new FileInfo(Assembly.GetAssembly(typeof(WebCore)).Location).Directory.FullName;
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.version = version;
            moduleDescription.Name = "Directory-Module-Web-Core";
            {
                //Register cmds and handlers etc.
                {
                    //CMDS

                }
                {
                    //Register Settings Handler.
                    Load();
                    Tasks.RegisterTask(Load, Tasks.TaskType.Every10Seconds);
                }
                {
                    //Handlers
                    EventHandler<HttpRequestData> eventHandler = (a, b) =>
                    {
                        //Console.WriteLine(""+ config.GetValues("Urls").Count);
                        foreach (var item in config.GetValues("Urls"))
                        {
                            if (b.requestUrl.StartsWith(item))
                            {
                                try
                                {
                                    string dir = URLConventor.Convert(b.requestUrl);
                                    if (FileUtilities.DirectoryExist(dir, URLConventor.RootFolder))
                                    {
                                        Debugger.currentDebugger.Log("Browsing:" + item + ":" + dir + "," + URLConventor.RootFolder);
                                        var dirInfo = FileUtilities.GetFolderFromURL(dir, URLConventor.RootFolder);
                                        Console.WriteLine("Real Folder:" + dirInfo.FullName);
                                        string items = "";
                                        items += TemplateItem.Replace("[ItemLink]", (b.requestUrl.EndsWith("/") ? "" : "./" + dirInfo.Name + "/") + "../").Replace("[ItemName]", "" + "..")
                                            .Replace("[ItemDate]", "TimeNotApplicable").Replace("[ItemSize]", "-");
                                        foreach (var dirs in dirInfo.EnumerateDirectories())
                                        {
                                            items += TemplateItem.Replace("[ItemLink]", (b.requestUrl.EndsWith("/") ? "" : "./" + dirInfo.Name + "/") + dirs.Name + "/").Replace("[ItemName]", "" + dirs.Name + "")
                                            .Replace("[ItemDate]", "" + dirs.LastWriteTime + "").Replace("[ItemSize]", "-");
                                        }
                                        string FolderInfo = "";
                                        foreach (var file in dirInfo.EnumerateFiles())
                                        {
                                            if (file.Name == "Folder.Info")
                                            {
                                                try
                                                {

                                                    FolderInfo = File.ReadAllText(file.FullName);

                                                }
                                                catch (Exception)
                                                {
                                                    FolderInfo = "";
                                                }
                                                continue;
                                            }
                                            float fl = (float)file.Length / 1024.0f;
                                            items += TemplateItem.Replace("[ItemLink]", (b.requestUrl.EndsWith("/") ? "" : "./" + dirInfo.Name + "/") + file.Name + "").Replace("[ItemName]", file.Name + "")
                                            .Replace("[ItemDate]", "" + file.LastWriteTime + "").Replace("[ItemSize]", (fl < 1024 ? fl + " KB" : (fl < 1024 * 1024 ? fl / 1024f + " MB" : ((fl / 1024f) / 1024f) + " GB")));
                                        }
                                        string content = TemplatePage.Replace("[DirName]", dirInfo.Name).Replace("[Location]", b.requestUrl).Replace("[ItemList]", items);
                                        if (FolderInfo != "")
                                        {
                                            content=content.Replace("[FolderInfo.Display]", "inline");
                                            content=content.Replace("[FolderInfo.Content]", Markdig.Markdown.ToHtml(FolderInfo));
                                        }
                                        else
                                        {
                                            content = content.Replace("[FolderInfo.Display]", "none");
                                        }
                                        content = content.Replace("[ModuleVersion]", version.ToString());
                                        WebPagePresets.ApplyPreset(ref content);
                                        b.Cancel = true;
                                        HttpResponseData httpResponseData = new HttpResponseData();
                                        httpResponseData.content = Encoding.UTF8.GetBytes(content);
                                        httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                                        httpResponseData.Send(ref b.streamWriter);
                                    }
                                    else if (!dir.EndsWith("/"))
                                    {
                                        FileInfo f;
                                        if (dir.EndsWith("/list"))
                                        {
                                            //Send as pure text.
                                            var realLocation = b.requestUrl.Substring(0, b.requestUrl.Length - "/list".Length);
                                            {
                                                dir = URLConventor.Convert(realLocation);
                                                if (FileUtilities.DirectoryExist(dir, URLConventor.RootFolder))
                                                {
                                                    Debugger.currentDebugger.Log("Browsing:" + item + ":" + dir + "," + URLConventor.RootFolder);
                                                    var dirInfo = FileUtilities.GetFolderFromURL(dir, URLConventor.RootFolder);
                                                    Console.WriteLine("Real Folder:" + dirInfo.FullName);
                                                    string items = "";
                                                    foreach (var dirs in dirInfo.EnumerateDirectories())
                                                    {
                                                        items += dirs.Name + "/" + Environment.NewLine;
                                                    }
                                                    foreach (var file in dirInfo.EnumerateFiles())
                                                    {
                                                        if (file.Name == "Folder.Info")
                                                        {
                                                            continue;
                                                        }
                                                        items += file.Name + Environment.NewLine;
                                                    }
                                                    string content = items;
                                                    WebPagePresets.ApplyPreset(ref content);
                                                    b.Cancel = true;
                                                    HttpResponseData httpResponseData = new HttpResponseData();
                                                    httpResponseData.content = Encoding.UTF8.GetBytes(content);
                                                    httpResponseData.Additional = "Content-Type : text; charset=utf-8";
                                                    httpResponseData.Send(ref b.streamWriter);
                                                }
                                            }
                                        }else
                                        if ((f = FileUtilities.GetFileFromURL(dir, URLConventor.RootFolder)) != null)
                                        {
                                            if (f.Name.EndsWith("html") || f.Name.EndsWith("htm"))
                                            {
                                                HttpResponseData httpResponseData = new HttpResponseData();
                                                string content = File.ReadAllText(f.FullName);
                                                b.Cancel = true;
                                                WebPagePresets.ApplyPreset(ref content);
                                                httpResponseData.content = Encoding.UTF8.GetBytes(content);
                                                httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                                                httpResponseData.Send(ref b.streamWriter);
                                            }
                                        }else
                                        {
                                            HttpResponseData httpResponseData = new HttpResponseData();
                                            httpResponseData.content = Encoding.UTF8.GetBytes(SpecialPages.GetSpecialPage(KnownSpecialPages.Page404));
                                            httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                                            httpResponseData.Send(ref b.streamWriter);

                                        }
                                    }
                                    return;
                                }
                                catch (Exception e)
                                {
                                    Debugger.currentDebugger.Log(e.Message);
                                }
                            }
                        }
                    };
                    WebServer.AddHttpRequestHandler(eventHandler);
                }
            }
            return moduleDescription;
        }
        public void Load()
        {
            try
            {
                config = UniversalConfigurationMark2.LoadFromFile("./Configs/DirectoryModule.ini");
            }
            catch
            {
            }
            try
            {
                TemplatePage = File.ReadAllText(Path.Combine(RootDir, "BrowserPage.html"));
                TemplateItem = File.ReadAllText(Path.Combine(RootDir, "FileItemTemplate.html"));
            }
            catch
            {
            }
            try
            {
                foreach (var item in config.GetValues("Urls"))
                {
                    WebServer.AddIgnoreUrlPrefix(item);
                }
            }
            catch
            {
            }
        }
    }
}
