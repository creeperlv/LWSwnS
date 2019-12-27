using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using System;

namespace DirectoryModule
{
    public class WebCore : ExtModule
    {
        public static readonly Version version = new Version(0, 0, 1, 0);
        UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
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
                    Tasks.RegisterTask(() => {
                        try
                        {
                            config = UniversalConfigurationMark2.LoadFromFile("./Config/DirectoryModule.ini");
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
                    }, Tasks.TaskType.Every10Seconds);
                }
                {
                    //Handlers
                    EventHandler<HttpRequestData> eventHandler = (a, b) => {
                        foreach (var item in config.GetValues("Urls"))
                        {
                            if (b.requestUrl.StartsWith(item))
                            {
                                try
                                {
                                    string dir=URLConventor.Convert(b.requestUrl);
                                    if (LWSwnS.Api.Data.FileUtilities.DirectoryExist(dir, URLConventor.RootFolder)) {
                                        var dirInfo=LWSwnS.Api.Data.FileUtilities.GetFolderFromURL(dir, URLConventor.RootFolder);
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                    };
                    WebServer.AddHttpRequestHandler(eventHandler);
                }
            }
            return moduleDescription;
        }
    }
}
