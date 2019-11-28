using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Core.Data;
using System;
using System.IO;
using System.Reflection;

namespace MarkdownFileModule
{
    public class WebMD : ExtModule
    {
        public ModuleDescription InitModule()
        {
            ModuleDescription description = new ModuleDescription();
            description.Name = "MarkdownFile-Module";
            description.version = new Version(0, 0, 1, 0);
            WebServer.AddExemptFileType("md");
            var modDirectory =new FileInfo( Assembly.GetAssembly(this.GetType()).Location).Directory;
            EventHandler<HttpRequestData> eventHandler = (a, b) => {
                if (b.requestUrl.ToUpper().EndsWith("MD"))
                {
                    try
                    {

                        HttpResponseData httpResponseData = new HttpResponseData();
                        var RealUrl = URLConventor.Convert(b.requestUrl.Trim());
                        var MDContent = File.ReadAllText(RealUrl);
                        var content = File.ReadAllText(Path.Combine(modDirectory.FullName, "ContentTemplate.html")).Replace("[FileName]", (new FileInfo(RealUrl)).Name).Replace("[MDContent]", Markdig.Markdown.ToHtml(MDContent));
                        httpResponseData.content = System.Text.Encoding.UTF8.GetBytes(content);
                        httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                        httpResponseData.Send(ref b.streamWriter);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            };
            WebServer.AddHttpRequestHandler(eventHandler);
            return description;
        }
    }
}