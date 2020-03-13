using LWSwnS.Api.Data;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell.Local;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using LWSwnS.Globalization;
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

            Console.WriteLine("Copying Language Files...");

            DirectoryInfo LanguageDir = new DirectoryInfo(Path.Combine((new FileInfo(Assembly.GetAssembly(typeof(BinFileModule)).Location)).Directory.FullName, "Locales"));
            foreach (var lang in LanguageDir.EnumerateDirectories())
            {
                if (!Directory.Exists("./Locales/" + lang.Name))
                    Directory.CreateDirectory("./Locales/" + lang.Name);
                foreach (var langF in lang.EnumerateFiles())
                {
                    langF.CopyTo("./Locales/" + lang.Name + "/" + langF.Name, true);
                }
            }
        }
    }
    public class BinFileModule : ExtModule
    {

        public bool EndsWith(string s, List<string> list)
        {
            foreach (var item in list)
            {
                if (s.EndsWith(item.ToUpper()))
                    return true;
            }
            return false;
        }
        public ModuleDescription InitModule()
        {
            ModuleDescription description = new ModuleDescription();
            description.Name = "Binary-File-Transmission-Module";
            description.version = new Version(0, 0, 3, 0);
            UniversalConfigurationMark2 fileType = new UniversalConfigurationMark2();
            var list = new List<string>();
            var TextList = new List<string>();
            try
            {
                Language.LoadFile("BFT");
            }
            catch
            {
            }
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
                    TextList = fileType.GetValues("Text-based");
                    foreach (var item in TextList)
                    {
                        WebServer.AddExemptFileType(item);
                    }
                }
                catch (Exception)
                {
                }
            });
            {
                LocalShell.Register("BFT-Add-File-Type", (string s, bool b) =>
                {
                    try
                    {
                        fileType.AddItem("Binary", s.Trim());
                        fileType.SaveToFile("./Configs/BinFileTransModule.ini");
                        WebServer.AddExemptFileType(s.Trim());
                        Console.WriteLine(Language.GetString("BFT", "BFT.AddedTo", "Target type has been added to the configuration file."));
                    }
                    catch
                    {
                    }
                });
                LocalShell.Register("Add-Bin-File-Type", (string s, bool b) =>
                {
                    try
                    {
                        fileType.AddItem("Binary", s.Trim());
                        fileType.SaveToFile("./Configs/BinFileTransModule.ini");
                        WebServer.AddExemptFileType(s.Trim());
                        Console.WriteLine(Language.GetString("BFT", "BFT.AddedTo", "Target type has been added to the configuration file."));
                    }
                    catch
                    {
                    }
                });
                LocalShell.Register("Add-Text-File-Type", (string s, bool b) =>
                {
                    try
                    {
                        fileType.AddItem("Text", s.Trim());
                        fileType.SaveToFile("./Configs/BinFileTransModule.ini");
                        WebServer.AddExemptFileType(s.Trim());
                        Console.WriteLine(Language.GetString("BFT", "BFT.AddedTo", "Target type has been added to the configuration file."));
                    }
                    catch
                    {
                    }
                });
                LocalShell.Register("BFT-Update-Basic-File-Type", (string s, bool b) =>
                {
                    try
                    {
                        UpdateList(ref fileType);
                        Console.WriteLine(Language.GetString("BFT", "BFT.UpdateType", "Configuration file has been updated with the newest pre-defined types."));
                    }
                    catch
                    {
                    }
                });
                LocalShell.Register("bc-get-help", (string s, bool b) =>
                {
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("BFT-Add-File-Type <FileType>");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\tAdd given file type into configuration file.");
                    }
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Add-Bin-File-Type <FileType>");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\tAdd given file type into configuration file as binary file type.");
                    }
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Add-Text-File-Type <FileType>");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\tAdd given file type into configuration file as pure-text file type.");
                    }
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("BFT-Update-Basic-File-Type");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\tUpdate the configuration file with embedded file type list.");
                    }
                });
            }
            EventHandler<HttpRequestData> e = (a, b) =>
            {
                if (EndsWith(b.requestUrl.ToUpper(), list))
                {
                    try
                    {

                        bool isMobile = false;
                        if (ServerConfiguration.CurrentConfiguration.SplitModile == true)
                            isMobile = b.isMobile;
                        b.Cancel = true;
                        HttpResponseData httpResponseData = new HttpResponseData();
                        httpResponseData.Additional = "Content-Type: Application/Binary" + Environment.NewLine + "Accept-Ranges: bytes";
                        var RealUrl = URLConventor.Convert(b.requestUrl.Trim(), isMobile);
                        var fi = FileUtilities.GetFileFromURL(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder);
                        if (fi != null)
                            httpResponseData.Additional += Environment.NewLine + $"Content-Disposition: attachment; filename=\"{fi.Name}\"";
                        if (b.Range.Ranges.Count > 0)
                        {
                            //SendFileInRange
                            using (var fs = fi.OpenRead())
                            {
                                httpResponseData.StatusLine = "HTTP/1.1 206 Partial Content";
                                httpResponseData.Additional += Environment.NewLine + $"Content-Range: bytes {b.Range.Ranges[0].Key}-{b.Range.Ranges[0].Value}/{fs.Length}";
                                httpResponseData.SendFileInRange(ref b.streamWriter, fs, b.Range.Ranges[0]);
                            }
                        }
                        else
                        {
                            if (fi != null)
                                using (var fs = fi.OpenRead())
                                {
                                    httpResponseData.SendFile(ref b.streamWriter, fs);
                                }
                        }
                    }
                    catch (Exception err)
                    {
                        Debugger.currentDebugger.Log(Language.GetString("BFT", "BFT.Error", "Something error happened in BFT:") + err, MessageType.Error);
                    }
                }
                else
                if (EndsWith(b.requestUrl.ToUpper(), TextList))
                {
                    try
                    {

                        bool isMobile = false;
                        if (ServerConfiguration.CurrentConfiguration.SplitModile == true)
                            isMobile = b.isMobile;
                        b.Cancel = true;
                        HttpResponseData httpResponseData = new HttpResponseData();
                        httpResponseData.Additional = "Content-Type: text/plain" + Environment.NewLine + "Accept-Ranges: bytes";
                        var RealUrl = URLConventor.Convert(b.requestUrl.Trim(), isMobile);
                        var fi = FileUtilities.GetFileFromURL(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder);
                        httpResponseData.Additional += Environment.NewLine + $"Content-Disposition: inline; filename=\"{fi.Name}\"";
                        if (b.Range.Ranges.Count > 0)
                        {
                            //SendFileInRange
                            using (var fs = fi.OpenRead())
                            {
                                httpResponseData.StatusLine = "HTTP/1.1 206 Partial Content";
                                httpResponseData.Additional += Environment.NewLine + $"Content-Range: bytes {b.Range.Ranges[0].Key}-{b.Range.Ranges[0].Value}/{fs.Length}";
                                httpResponseData.SendFileInRange(ref b.streamWriter, fs, b.Range.Ranges[0]);
                            }
                        }
                        else
                            using (var fs = fi.OpenRead())
                            {
                                httpResponseData.SendFile(ref b.streamWriter, fs);
                            }
                    }
                    catch (Exception err)
                    {
                        Debugger.currentDebugger.Log(Language.GetString("BFT", "BFT.Error", "Something error happened in BFT:") + err, MessageType.Error);
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
            fileType.AddItem("Binary", "apk");
            fileType.AddItem("Binary", "ipa");
            fileType.AddItem("Binary", "pkg");
            fileType.AddItem("Binary", "fbx");
            fileType.AddItem("Binary", "pdb");
            fileType.AddItem("Binary", "ogg");
            fileType.AddItem("Binary", "avi");
            fileType.AddItem("Binary", "wav");
            fileType.AddItem("Binary", "flac");
            fileType.AddItem("Binary", "ncm");
            fileType.AddItem("Binary", "wim");
            fileType.AddItem("Binary", "flv");
            fileType.AddItem("Binary", "pdf");
            fileType.AddItem("Binary", "doc");
            fileType.AddItem("Binary", "docx");
            fileType.AddItem("Binary", "xls");
            fileType.AddItem("Binary", "xlsx");
            fileType.AddItem("Binary", "ppt");
            fileType.AddItem("Binary", "pptx");
            fileType.AddItem("Binary", "db");
            fileType.AddItem("Binary", "accdb");
            fileType.AddItem("Binary", "stl");
            fileType.AddItem("Binary", "dat");
            fileType.AddItem("Binary", "appx");
            fileType.AddItem("Binary", "msix");
            fileType.AddItem("Binary", "msi");
            fileType.AddItem("Binary", "pdb");
            fileType.AddItem("Binary", "gz");
            fileType.AddItem("Binary", "tar");
            fileType.AddItem("Binary", "xz");
            fileType.AddItem("Binary", "rpm");
            fileType.AddItem("Binary", "mkv");
            fileType.AddItem("Binary", "src");
            fileType.AddItem("Binary", "glsl");
            fileType.AddItem("Binary", "fsh");
            fileType.AddItem("Binary", "vsh");
            fileType.AddItem("Binary", "meta");
            fileType.AddItem("Binary", "unity");
            fileType.AddItem("Binary", "mat");
            fileType.AddItem("Binary", "shader");
            fileType.AddItem("Binary", "asset");
            fileType.AddItem("Binary", "hlsl");
            fileType.AddItem("Binary", "resx");
            fileType.AddItem("Text", "txt");
            fileType.AddItem("Text", "ini");
            fileType.AddItem("Text", "inf");
            fileType.AddItem("Text", "cs");
            fileType.AddItem("Text", "java");
            fileType.AddItem("Text", "cpp");
            fileType.AddItem("Text", "h");
            fileType.AddItem("Text", "sh");
            fileType.AddItem("Text", "ps1");
            fileType.AddItem("Text", "bat");
            fileType.SaveToFile("./Configs/BinFileTransModule.ini");

        }
    }
}
