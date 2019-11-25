using LWSwnS.Api;
using LWSwnS.Api.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LWSwnS.Core
{
    public class ShellServer
    {
        public static Version ShellServerVersion = new Version("1.0.0.0");
        public static string ShellPassword;
        TcpListener TCPListener;
        bool ShellStop = false;
        public List<ShellClientProcessor> shellClients = new List<ShellClientProcessor>();
        public static Dictionary<string, Func<string, object, StreamWriter, bool>> Commands = new Dictionary<string, Func<string, object, StreamWriter, bool>>();
        public ShellServer(TcpListener listener)
        {
            TCPListener = listener;
        }
        public void StartListen()
        {
            Commands.Add("Get-Shell-Server-Version", (string a, object b, StreamWriter sw) =>
            {
                Console.WriteLine("Someone trying to get version.");
                ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                shellFeedbackData.StatusLine = ShellServerVersion.ToString();
                shellFeedbackData.writer = sw;
                shellFeedbackData.SendBack();
                return true;
            });
            Commands.Add("Stop-Server", (string a, object b, StreamWriter sw) =>
            {
                Environment.Exit(0);
                return false;
            });
            Task.Run(() =>
            {
                TCPListener.Start();
                while (ShellStop == false)
                {
                    var a = TCPListener.AcceptTcpClient();
                    var p = new ShellClientProcessor(a, this);
                    shellClients.Add(p);
                    //var c = new TcpClientProcessor(a);
                    //tcpClients.Add(c);
                }
            });
        }
    }
    public class ShellClientProcessor
    {
        TcpClient client;
        ShellServer FatherServer;
        NetworkStream networkStream;
        StreamReader streamReader;
        StreamWriter streamWriter;
        public ShellClientProcessor(TcpClient tcpClient, ShellServer server)
        {
            client = tcpClient;
            FatherServer = server;
            networkStream = client.GetStream();
            //networkStream.CanTimeout = true;
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);
            Task.Run(MainProcess);
        }
        bool willStop = false;
        public void StopImmediately()
        {
            willStop = true;
            try
            {

                streamReader.Close();
                streamReader.Dispose();

            }
            catch (Exception)
            {
            }
            try
            {

                streamWriter.Close();
                streamWriter.Dispose();

            }
            catch (Exception)
            {
            }
            try
            {

                networkStream.Close();
                networkStream.Dispose();

            }
            catch (Exception)
            {
            }
            GC.Collect();
            FinalizeStop();
        }
        public void SoftStop()
        {
            willStop = true;
        }
        public void FinalizeStop()
        {
            try
            {

                FatherServer.shellClients.Remove(this);
                GC.Collect();
            }
            catch (Exception)
            {

            }
        }
        void MainProcess()
        {

            while (willStop == false)
            {
                try
                {

                    var str = streamReader.ReadLine();
                    var content = NETCore.Encrypt.EncryptProvider.AESDecrypt(str, ShellServer.ShellPassword);
                    StringReader stringReader = new StringReader(content);
                    var cmd = stringReader.ReadLine();
                    var name = cmd.Substring(0, cmd.IndexOf(' '));
                    var parameter = cmd.Substring(cmd.IndexOf(' ') + 1);
                    var doc = stringReader.ReadToEnd();
                    object obj = null;
                    if (!doc.StartsWith("NULL"))
                    {
                        var data = Convert.FromBase64String(doc);

                        MemoryStream memoryStream = new MemoryStream(data);
                        BinaryFormatter binary = new BinaryFormatter();
                        obj = binary.Deserialize(memoryStream);
                    }
                    bool isReacted = false;
                    bool isShellResponsed = false;
                    foreach (var item in ShellServer.Commands)
                    {
                        if (item.Key == name)
                        {
                            try
                            {
                                bool isResponsed = item.Value(parameter, obj, streamWriter);
                                if (isResponsed == true) isShellResponsed = true;
                                isReacted = true;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    if (isReacted == false)
                    {
                        ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                        shellFeedbackData.StatusLine = "Error: No such a command!";
                        shellFeedbackData.writer = streamWriter;
                        shellFeedbackData.SendBack();
                    }
                    else if (isShellResponsed==false)
                    {

                        ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                        shellFeedbackData.writer = streamWriter;
                        shellFeedbackData.SendBack();

                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error in shell server:" + e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    StopImmediately();
                }
                //if ()
                //{

                //}
            }
        }
    }
}
