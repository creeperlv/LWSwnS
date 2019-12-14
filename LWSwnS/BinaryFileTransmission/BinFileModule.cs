using LWSwnS.Api.Data;
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
            fileType.AddItem("Binary", "7z");
            fileType.AddItem("Binary", "zip");
            fileType.AddItem("Binary", "mp3");
            fileType.AddItem("Binary", "wav");
            fileType.AddItem("Binary", "rar");
            fileType.AddItem("Binary", "out");
            fileType.AddItem("Binary", "dll");
            fileType.AddItem("Binary", "png");
            fileType.AddItem("Binary", "bmp");
            fileType.AddItem("Binary", "dmg");
            fileType.AddItem("Binary", "jpg");
            fileType.AddItem("Binary", "png");
            fileType.AddItem("Binary", "com");
            fileType.AddItem("Binary", "lib");
            fileType.AddItem("Binary", "so");
            fileType.AddItem("Binary", "nuget");
            fileType.SaveToFile("./Configs/BinFileTransModule.ini");
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
            fileType = UniversalConfigurationMark2.LoadFromFile("./Configs/BinFileTransModule.ini");
            var list = fileType.GetValues("Binary");
            foreach (var item in list)
            {
                WebServer.AddExemptFileType(item);
            }
            EventHandler<HttpRequestData> e= (a,b) => {
                if (EndsWith(b.requestUrl.ToUpper(),list))
                {

                    bool isMobile = false;
                    if (ServerConfiguration.CurrentConfiguration.SplitModile == true)
                        if (b.UA.IndexOf("Android") > 0 || b.UA.IndexOf("iPhone") > 0 || b.UA.IndexOf("Windows Phone") > 0 || b.UA.IndexOf("Lumia") > 0)
                        {
                            isMobile = true;
                        }
                    HttpResponseData httpResponseData = new HttpResponseData();
                    httpResponseData.Additional = "Application/Binary";
                    var RealUrl = URLConventor.Convert(b.requestUrl.Trim(),isMobile);
                    var fi = FileUtilities.GetFileFromURL(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder);
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
