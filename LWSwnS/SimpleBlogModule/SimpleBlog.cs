﻿using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SimpleBlogModule
{
    public class SimpleBlog : ExtModule
    {
        public void SortFileByTime(ref FileInfo[] FList)
        {
            Array.Sort(FList, delegate (FileInfo x, FileInfo y) { return y.CreationTime.CompareTo(x.CreationTime); });
        }
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "SimpleBlog";
            String RootDir = new FileInfo(Assembly.GetAssembly(this.GetType()).Location).Directory.FullName;
            moduleDescription.version = new Version(0, 0, 2, 0);
            UniversalConfiguration config = new UniversalConfiguration();
            try
            {
                config = UniversalConfigurationLoader.LoadFromFile("./SimpleBlog.ini");
            }
            catch (Exception)
            {
            }
            string BlogName = "Blog";
            try
            {
                BlogName = config["BlogName"];
            }
            catch (Exception)
            {
            }

            string PostList = File.ReadAllText(Path.Combine(RootDir, "PostList.html"));
            string Template = File.ReadAllText(Path.Combine(RootDir, "Template.html"));
            string PostItemTemplate = File.ReadAllText(Path.Combine(RootDir, "PostItemTemplate.html"));
            WebServer.AddIgnoreUrlPrefix("/POSTS");
            EventHandler<HttpRequestData> a = (object sender, HttpRequestData b) =>
            {

                if (b.requestUrl.ToUpper().StartsWith("/POSTS"))
                {

                    HttpResponseData httpResponseData = new HttpResponseData();
                    if (b.requestUrl.Trim().ToUpper().Equals("/POSTS") | b.requestUrl.Trim().ToUpper().Equals("/POSTS/"))
                    {
                        var temp = PostItemTemplate;
                        DirectoryInfo directory = new DirectoryInfo("./Posts/");
                        var f = directory.GetFiles();
                        SortFileByTime(ref f);
                        List<string> PostItems = new List<string>();
                        foreach (var item in f)
                        {
                            try
                            {
                                var sr = item.OpenRead();
                                var SR = new StreamReader(sr);
                                var Title = SR.ReadLine();
                                try
                                {
                                    SR.Close();
                                    SR.Dispose();
                                    sr.Close();
                                    sr.Dispose();
                                }
                                catch (Exception)
                                {
                                }
                                var link = "/posts/" + item.Name;
                                PostItems.Add(temp.Replace("[POSTLINK]", link).Replace("[POSTTITLE]", Title)
                                    .Replace("[POSTDATE]", item.CreationTime.ToString()).Replace("[FILESIZE]", ((double)item.Length) / 1024.0 + " KB"));
                            }
                            catch (Exception)
                            {
                            }
                        }
                        var List = "";
                        foreach (var item in PostItems)
                        {
                            List += item;
                        }
                        if (f.Length == 0)
                        {
                            List = "<p style=\"32\">No Posts<p>";
                        }
                        var content = PostList.Replace("[BLOGNAME]", BlogName).Replace("[POSTLIST]", List);
                        httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                        httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                        httpResponseData.Send(ref b.streamWriter);
                    }
                    //else if (b.requestUrl.ToUpper().EndsWith("PNG") | b.requestUrl.ToUpper().EndsWith("WEBP") | b.requestUrl.ToUpper().EndsWith("JPG") | b.requestUrl.ToUpper().EndsWith("MP4"))
                    //{
                    //    var location = b.requestUrl.Substring("/POSTS/".Length);
                    //    httpResponseData.content = File.ReadAllBytes(location);
                    //}
                    else if (b.requestUrl.ToUpper().StartsWith("/POSTS"))
                    {
                        try
                        {

                            var location = b.requestUrl.Substring("/POSTS/".Length);
                            if(File.Exists("./Posts/" + location))
                            {

                                var lines = File.ReadAllLines("./Posts/" + location).ToList();
                                var title = lines[0];
                                lines.RemoveAt(0);
                                var MDContent = "";
                                foreach (var item in lines)
                                {
                                    if (MDContent == "")
                                    {
                                        MDContent += item;
                                    }
                                    else
                                    {
                                        MDContent += Environment.NewLine;
                                        MDContent += item;
                                    }
                                }
                                var content = Template.Replace("[POSTNAME]", title).Replace("[BLOGNAME]", BlogName).Replace("[POSTCONTENT]", Markdig.Markdown.ToHtml(MDContent));
                                httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                            }
                            else
                            {
                                var content = Template.Replace("[POSTNAME]", "File Not Found!").Replace("[BLOGNAME]", BlogName).Replace("[POSTCONTENT]", Markdig.Markdown.ToHtml("# Unable to locate requesting file!"));
                                httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                            }
                        }
                        catch (Exception)
                        {
                            var content = File.ReadAllText(Path.Combine(RootDir,"UnderCounstruction.html")).Replace("[MODULE_NAME]", moduleDescription.Name).Replace("[MODULE_VERSION]", moduleDescription.version.ToString());
                            httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                        }
                        httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                        httpResponseData.Send(ref b.streamWriter);
                        b.Cancel = true;
                    }
                    //b.Cancel = true;
                }
            };
            WebServer.AddHttpRequestHandler(a);
            return moduleDescription;
        }
    }
}
