using LWSwnS.Api.Data;
using LWSwnS.Api.Data.Streams;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell;
using LWSwnS.Configuration;
using LWSwnS.Diagnostic;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FileReceiever
{
    public class Receiever : ExtModule
    {
        public TcpListener listener;
        public static Version ModuleVersion = new Version(0, 0, 1, 0);
        public UniversalConfiguration config = new UniversalConfiguration();
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "FileReceieverModule";
            moduleDescription.version = ModuleVersion;
            config = UniversalConfigurationLoader.LoadFromFile("./Configs/FileReceiever.ini");
            Tasks.RegisterTask(() => {
                try
                {
                    config = UniversalConfigurationLoader.LoadFromFile("./Configs/FileReceiever.ini");

                }
                catch (Exception)
                {
                }
            });
            //Task.Run(async () => {
            //    await Task.Delay(500);
            //    Debugger.currentDebugger.Log("Auto Config Reload Initialized.");
            //    while (true)
            //    {
            //        await Task.Delay(5000);
            //        config = UniversalConfigurationLoader.LoadFromFile("./Configs/FileReceiever.ini");
            //    }
            //});
            CommandHandler.RegisterCommand("push-file", (a, b, c) => {
                string name = a;
                string info = b as string;
                if (info.StartsWith("Shift:"))
                {
                    int shift = int.Parse(info.Substring("Shift:".Length));
                    Debugger.currentDebugger.Log("Try to receieve as:"+shift);
                    ShiftedStream stream = new ShiftedStream(c.BaseStream, shift);
                    FileInfo fi = new FileInfo(name);
                    var sw = (fi.OpenWrite());
                    if (!fi.Directory.Exists) fi.Directory.Create();
                        byte[] buffer = new byte[int.Parse(config.Get("BufferSize", "4096"))];
                    while (stream.isEnd==false)
                    {
                        int length=stream.Read(buffer, 0, buffer.Length);
                        sw.Write(buffer, 0, length);
                    }
                    sw.Close();
                }
                else
                {
                    ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                    shellFeedbackData.StatusLine = "Wrong Message Body!";
                    shellFeedbackData.writer = c;
                    shellFeedbackData.SendBack();
                    return true;
                }
                {

                    ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                    shellFeedbackData.StatusLine = "OK";
                    shellFeedbackData.writer = c;
                    shellFeedbackData.SendBack();

                }
                return true; });
            return moduleDescription;
        }
    }
    public class FirstRun : FirstInit
    {
        public void Init()
        {
            UniversalConfiguration config = new UniversalConfiguration();
            config.Add("BufferSize", "4096");
            config.SaveToFile("./Configs/FileReceiever.ini");
        }
    }
}
