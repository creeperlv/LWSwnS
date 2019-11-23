using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleBlogModule
{
    public class SimpleBlog : ExtModule
    {
        public ModuleDescription InitModule()
        {
            Console.WriteLine("Blog Initialize.");
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "SimpleBlog";
            moduleDescription.version = new Version(0, 0, 1, 0);
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
            WebServer.AddIgnoreUrlPrefix("/POSTS");
            EventHandler<HttpRequestData> a = (object sender, HttpRequestData b) =>
            {
                HttpResponseData httpResponseData = new HttpResponseData();
                if (b.requestUrl.Trim().ToUpper().Equals("/POSTS") | b.requestUrl.Trim().ToUpper().Equals("/POSTS/"))
                {
                    var temp = File.ReadAllText("./Modules/netstandard2.0/PostItemTemplate.html");
                    DirectoryInfo directory = new DirectoryInfo("./Posts/");
                    var f = directory.GetFiles();
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
                            var link = "./" + item.Name;
                            PostItems.Add(temp.Replace("[POSTLINK]", link).Replace("[POSTTITLE]", Title)
                                .Replace("[POSTDATE]",item.CreationTime.ToString()).Replace("[FILESIZE]",((double)item.Length)/1024.0+" KB"));
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
                    var content = File.ReadAllText("./Modules/netstandard2.0/PostList.html").Replace("[BLOGNAME]", BlogName).Replace("[POSTLIST]", List);
                    httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                }
                else if (b.requestUrl.ToUpper().StartsWith("/POSTS"))
                {
                    try
                    {

                        var location = b.requestUrl.Substring("/POSTS/".Length);
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
                        var content = File.ReadAllText("./Modules/netstandard2.0/Template.html").Replace("[POSTNAME]", title).Replace("[BLOGNAME]",BlogName).Replace("[POSTCONTENT]", Markdig.Markdown.ToHtml(MDContent));
                        //.Replace("[MODULE_VERSION]", moduleDescription.version.ToString());

                        httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                    }
                    catch (Exception)
                    {
                        var content = File.ReadAllText("./Modules/netstandard2.0/UnderCounstruction.html").Replace("[MODULE_NAME]", moduleDescription.Name).Replace("[MODULE_VERSION]", moduleDescription.version.ToString());
                        httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                    }

                    b.Cancel = true;
                }
                httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                httpResponseData.Send(ref b.streamWriter);
            };
            WebServer.AddHttpRequestHandler(a);
            return moduleDescription;
        }
    }
}
