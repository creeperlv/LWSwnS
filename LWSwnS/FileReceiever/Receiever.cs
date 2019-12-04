using LWSwnS.Api.Data;
using LWSwnS.Api.Data.Streams;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Shell;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FileReceiever
{
    public class Receiever : ExtModule
    {
        public TcpListener listener;
        public static Version ModuleVersion = new Version(0, 0, 1, 0);
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "FileReceieverModule";
            moduleDescription.version = ModuleVersion;
            CommandHandler.RegisterCommand("push-file", (a, b, c) => {
                string name = a;
                string info = b as string;
                if (info.StartsWith("Shift:"))
                {
                    int shift = int.Parse(info.Substring("Shift:".Length));
                    ShiftedStream stream = new ShiftedStream(c.BaseStream, shift);

                }
                else
                {
                    ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                    shellFeedbackData.StatusLine = "Wrong Message Body!";
                    shellFeedbackData.writer = c;
                    shellFeedbackData.SendBack();
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
}
