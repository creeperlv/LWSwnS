using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace BinaryFileTransmission
{
    public class OOBE : FirstInit
    {
        public void Init()
        {
            Console.WriteLine("Generating file type configuration...");
            UniversalConfigurationMark2 fileType = new UniversalConfigurationMark2();
            fileType.AddItem("Binary", "exe");
            fileType.AddItem("Binary", "mp4");
            fileType.SaveToFile("./BinFileTransModule.ini");
        }
    }
    public class BinFileModule : ExtModule
    {
        
        public bool EndsWith(string s,List<string> list)
        {
            foreach (var item in list)
            {
                if (s.EndsWith(item.ToUpper())) return true;
            }
            return false;
        }
        public ModuleDescription InitModule()
        {
            ModuleDescription description = new ModuleDescription();
            description.Name = "Binary-File-Transmission-Module";
            description.version = new Version(0, 0, 1, 0);
            UniversalConfigurationMark2 fileType = new UniversalConfigurationMark2();
            fileType = UniversalConfigurationMark2.LoadFromFile("./BinFileTransModule.ini");
            var list = fileType.GetValues("Binary");
            foreach (var item in list)
            {
                WebServer.AddExemptFileType(item);
            }
            EventHandler<HttpRequestData> e= (a,b) => {
                if (EndsWith(b.requestUrl.ToUpper(),list))
                {
                    
                    HttpResponseData httpResponseData = new HttpResponseData();
                    httpResponseData.Additional = "Application/Binary";
                    var RealUrl = URLConventor.Convert(b.requestUrl.Trim());
                    var fi = new FileInfo(RealUrl);
                    using(var fs = fi.OpenRead())
                    {
                        httpResponseData.SendFile(ref b.streamWriter, fs);
                    }
                }
            };
            WebServer.AddHttpRequestHandler(e);
            return description;
        }
    }
}
