using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using System;
using System.Threading.Tasks;

namespace WikiModule
{
    public class WikiWebCore : ExtModule
    {
        string PageTemplate = "";
        UniversalConfiguration config = new UniversalConfiguration();
        static Version ModuleVersion = new Version(0,0,1,0);
        public ModuleDescription InitModule()
        {
            Task.Run(async () => {
                Debugger.currentDebugger.Log("Automatic Configuration Reload task initialized.");
                while (true)
                {
                    Load();
                    await Task.Delay(5000);
                }
            });
            WebServer.AddIgnoreUrlPrefix("/wiki");
            EventHandler<HttpRequestData> eventHandler = (a, b) => {
                if (b.requestUrl.ToUpper().StartsWith("/WIKI"))
                {

                }
            };
            WebServer.AddHttpRequestHandler(eventHandler);
            {
                ModuleDescription description = new ModuleDescription();
                description.Name = "Wiki-Module-Web";
                description.version = ModuleVersion;
                return description;
            }
        }
        void Load()
        {
            config = UniversalConfigurationLoader.LoadFromFile("./Configs/WikiModule.ini");

        }
    }
}
