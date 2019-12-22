using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BinaryFileTransmission
{
    public class OOBE : FirstInit
    {
        public void Init()
        {
            Console.WriteLine("Generating file type configuration...");
            UniversalConfigurationMark2 fileType = new UniversalConfigurationMark2();
            BinFileModule.UpdateList(ref fileType);
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
            description.version = new Version(0, 0, 2, 0);
            UniversalConfigurationMark2 fileType = new UniversalConfigurationMark2();
            var list = new  List<string>();
            Tasks.RegisterTask(() =>
            {
                try
                {

                    fileType = UniversalConfigurationMark2.LoadFromFile("./Configs/BinFileTransModule.ini");

                    list = fileType.GetValues("Binary");
                    foreach (var item in list)
                    {
                        WebServer.AddExemptFileType(item);
                    }
                }
                catch (Exception)
                {
                }
            });
            //Task.Run(async () => {
            //    await Task.Delay(1000);
            //    Debugger.currentDebugger.Log("Auto configuration reload initialized.");
            //    while (true)
            //    {
            //        try
            //        {

            //            fileType = UniversalConfigurationMark2.LoadFromFile("./Configs/BinFileTransModule.ini");

            //        }
            //        catch (Exception)
            //        {
            //        }
            //        await Task.Delay(5000);
            //    }
            //});
            {
                LocalShell.Register("BFT-Add-File-Type", (string s) => {
                    try
                    {
                        fileType.AddItem("Binary",s.Trim());
                        fileType.SaveToFile("./Configs/BinFileTransModule.ini");
                        WebServer.AddExemptFileType(s.Trim());
                        Console.WriteLine("Target type has been added to the configuration file.");
                    }
                    catch
                    {
                    }
                });
                LocalShell.Register("BFT-Update-Basic-File-Type", (string s) => {
                    try
                    {
                        UpdateList(ref fileType);
                        Console.WriteLine("Target type has been added to the configuration file.");
                    }
                    catch
                    {
                    }
                });
            }
            EventHandler<HttpRequestData> e= (a,b) => {
                if (EndsWith(b.requestUrl.ToUpper(),list))
                {
                    
                    bool isMobile = false;
                    if (ServerConfiguration.CurrentConfiguration.SplitModile == true)
                        isMobile = b.isMobile;
                    HttpResponseData httpResponseData = new HttpResponseData();
                    httpResponseData.Additional = "Application/Binary"+Environment.NewLine+ "Accept-Ranges: bytes";
                    var RealUrl = URLConventor.Convert(b.requestUrl.Trim(),isMobile);
                    var fi = FileUtilities.GetFileFromURL(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder);
                    if (b.Range.Ranges.Count > 0)
                    {
                        //SendFileInRange
                        using (var fs = fi.OpenRead())
                        {
                            httpResponseData.StatusLine = "HTTP/1.1 206 Partial Content";
                            httpResponseData.Additional += Environment.NewLine + $"Content-Range: bytes {b.Range.Ranges[0].Key}-{b.Range.Ranges[0].Value}/{fs.Length}";
                            httpResponseData.SendFileInRange(ref b.streamWriter, fs,b.Range.Ranges[0]);
                        }
                    }
                    else
                    using(var fs = fi.OpenRead())
                    {
                        httpResponseData.SendFile(ref b.streamWriter, fs);
                    }
                }
            };
            WebServer.AddHttpRequestHandler(e);
            return description;
        }
        public static void UpdateList(ref UniversalConfigurationMark2 fileType)
        {
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
            fileType.AddItem("Binary", "deb");
            fileType.AddItem("Binary", "jar");
            fileType.AddItem("Binary", "com");
            fileType.AddItem("Binary", "so");
            fileType.AddItem("Binary", "lib");
            fileType.AddItem("Binary", "tga");
            fileType.AddItem("Binary", "webp");
            fileType.AddItem("Binary", "iso");
            fileType.SaveToFile("./Configs/BinFileTransModule.ini");

        }
    }
}
