﻿using LWSwnS.Api.Data;
using LWSwnS.Api.Data.Streams;
using LWSwnS.Configuration;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Schema;

namespace LWSwnS.ShellClient
{
    class Program
    {
        static byte[] Obj2Byte(object obj)
        {
            try
            {

                byte[] buff = new byte[Marshal.SizeOf(obj)];
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buff, 0);
                Marshal.StructureToPtr(obj, ptr, true);
                return buff;
            }
            catch (Exception)
            {

                if (obj.GetType() == typeof(string))
                {
                    return Encoding.UTF8.GetBytes(obj.ToString());
                }
                else
                {

                    byte[] buff = ShellDataExchange.ObjectToBytes(obj);
                    return buff;
                }
            }
        }
        static void OOBE()
        {
            Console.Write("Please enter the ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("address(IP or domain) ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("you want to connect to:");
            var ip = Console.ReadLine();


            Console.Write("Please enter the ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("port");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" you want to connect to:");
            var port = Console.ReadLine();

            Console.Write("Please enter the ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("AES Key");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" the server uses.");
            var key = Console.ReadLine();
            UniversalConfiguration config = new UniversalConfiguration();
            config.Add("Address", ip);
            config.Add("AESKey", key);
            config.Add("Port", port);
            config.SaveToFile("./LWSwnS.Shell.Client.ini");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Configurations Saved.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("You can reconfig anytime by typing \"!config\".");
        }
        static void Main(string[] args)
        {
            if (args.Length > 0)
                if (args[0] == "!config")
                {
                    OOBE();
                }
            if (!Directory.Exists("./ReceiveDatas/")) { Directory.CreateDirectory("./ReceiveDatas/"); }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("LWSwnS Shell Client");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loading Configurations...");
            Console.ForegroundColor = ConsoleColor.White;
            UniversalConfiguration config = new UniversalConfiguration();
            if (!File.Exists("./LWSwnS.Shell.Client.ini"))
            {
                Console.WriteLine("It seems the first time you use this Application.");
                Console.WriteLine("Let's set up something first.");
                OOBE();
            }
            config = UniversalConfigurationLoader.LoadFromFile("./LWSwnS.Shell.Client.ini");
            Console.ForegroundColor = ConsoleColor.White;
            var ip = config["Address"];
            var key = config["AESKey"];
            try
            {

                var port = int.Parse(config["Port"]);
                ShellDataExchange.AES_PW = key;
                string cmd = "";
                while (!(cmd = Console.ReadLine()).ToUpper().Equals("EXIT"))
                {
                    TcpClient tcpClient = new TcpClient();
                    bool ReceiveResult = true;
                    try
                    {
                        tcpClient.Connect(ip, port);
                        if (cmd.StartsWith("~"))
                        {
                            cmd = cmd.Substring(1);
                            ReceiveResult = false;
                        }
                        if (cmd == "!config")
                        {
                            OOBE();
                            Console.WriteLine("Please reload the application.");
                            Console.WriteLine("Exiting...");
                        }
                        else if (cmd.StartsWith("#"))
                        {
                            var line = cmd.Substring(1);
                            var CMDN = line;
                            if (line.IndexOf(' ') > 0) CMDN = line.Substring(0, line.IndexOf(' '));
                            var PARA = "NULL";
                            if (line.IndexOf(' ') > 0) PARA = line.Substring(line.IndexOf(' ') + 1);
                            var Path = Console.ReadLine();
                            var data = File.ReadAllText(Path);
                            if (ReceiveResult == true)
                            {
                                var Feedback = ShellDataExchange.SendCommandAndWaitForResult(CMDN, PARA, data, new StreamWriter(tcpClient.GetStream()), new StreamReader(tcpClient.GetStream()));
                                Console.WriteLine(Feedback.StatusLine);
                                if (Feedback.DataBody != null)
                                {
                                    Console.WriteLine("Received Object:\r\n" + Feedback.DataBody.ToString());
                                    File.Create($"./ReceiveDatas/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.object").Close();
                                    File.WriteAllBytes($"./ReceiveDatas/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.object", Obj2Byte(Feedback.DataBody));

                                }
                            }
                            else
                            {
                                ShellDataExchange.SendCommand(CMDN, PARA, data, new StreamWriter(tcpClient.GetStream()));
                            }
                        }
                        else if (cmd.StartsWith("*"))
                        {
                            var line = cmd.Substring(1);
                            var CMDN = line;
                            if (line.IndexOf(' ') > 0) CMDN = line.Substring(0, line.IndexOf(' '));
                            var PARA = "NULL";
                            if (line.IndexOf(' ') > 0) PARA = line.Substring(line.IndexOf(' ') + 1);
                            var Path = Console.ReadLine();
                            Random random = new Random();
                            int shift = random.Next(1, 255);
                            var data = "Shift:" + shift;
                            var os = tcpClient.GetStream();
                            if (ReceiveResult == true)
                            {
                                var Feedback = ShellDataExchange.SendCommandAndWaitForResult(CMDN, PARA, data, new StreamWriter(tcpClient.GetStream()), new StreamReader(os));
                                Console.WriteLine(Feedback.StatusLine);
                                if (Feedback.DataBody != null)
                                {
                                    Console.WriteLine("Received Object:\r\n" + Feedback.DataBody.ToString());
                                    File.Create($"./ReceiveDatas/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.object").Close();
                                    File.WriteAllBytes($"./ReceiveDatas/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.object", Obj2Byte(Feedback.DataBody));
                                }
                            }
                            else
                            {
                                ShellDataExchange.SendCommand(CMDN, PARA, data, new StreamWriter(os));
                            }
                            ShiftedStream stream = new ShiftedStream(os, shift);
                            using (var f = (new FileInfo(Path)).OpenRead())
                            {
                                byte[] buffer = new byte[4096];
                                int l;
                                while ((l=f.Read(buffer, 0, 4096)) != 0)
                                {
                                    stream.WriteAndFlush(buffer, 0, l);
                                }
                            }
                            stream.WriteEnd();
                            stream.Dispose();
                        }
                        else
                        {
                            var CMDN = cmd;
                            if (cmd.IndexOf(' ') > 0) CMDN = cmd.Substring(0, cmd.IndexOf(' '));
                            var PARA = "NULL";
                            if (cmd.IndexOf(' ') > 0) PARA = cmd.Substring(cmd.IndexOf(' ') + 1);
                            if (ReceiveResult == true)
                            {

                                var Feedback = ShellDataExchange.SendCommandAndWaitForResult(CMDN, PARA, null, new StreamWriter(tcpClient.GetStream()), new StreamReader(tcpClient.GetStream()));
                                Console.WriteLine(Feedback.StatusLine);
                                if (Feedback.DataBody != null)
                                {
                                    Console.WriteLine("Received Object:\r\n" + Feedback.DataBody.ToString());
                                    File.Create($"./ReceiveDatas/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.object").Close();
                                    File.WriteAllBytes($"./ReceiveDatas/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.object", Obj2Byte(Feedback.DataBody));

                                }
                            }
                            else
                            {
                                ShellDataExchange.SendCommand(CMDN, PARA, null, new StreamWriter(tcpClient.GetStream()));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error:" + e.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    finally
                    {
                        try
                        {
                            tcpClient.GetStream().Close();
                            tcpClient.Close();
                        }
                        catch (Exception e)
                        {

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Error:" + e.Message);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                return;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong Configuration");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Exiting...");
            }
        }
    }
}
