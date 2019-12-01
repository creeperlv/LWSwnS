﻿using System;
using System.IO;
using System.Reflection;

namespace LWSwnS.Diagnostic
{
    public enum MessageType
    {
        Normal,Warning,Error
    }
    public interface IDebugger
    {
        void Log(string msg);
        void Log(string msg, MessageType msgType);
    }
    public class Debugger:IDebugger
    {
        public static Debugger currentDebugger = new Debugger();

        string fileName = "";
        public Debugger()
        {
            var Now = DateTime.Now;
            if (!Directory.Exists("./Log"))
            {
                Directory.CreateDirectory("./Log");
            }
            fileName = $"./Log/{Now.Year}-{Now.Month}-{Now.Day}.{Now.Hour}-{Now.Minute}-{Now.Second}.log";
            File.Create(fileName).Close();
            File.WriteAllText(fileName,"[LWSwnS Log]");
            File.AppendAllText(fileName, "\r\nGenerated by LWSwnS.Diagnostic");
        }
        public void Log(string msg)
        {
            System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace(true);
            var f=stack.GetFrame(1);
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write((new FileInfo(Assembly.GetAssembly(f.GetMethod().DeclaringType).Location)).Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("][");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("NORAML ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]");
            Console.WriteLine(msg);
            string CombinedMsg = $"[{(new FileInfo(Assembly.GetAssembly(f.GetMethod().DeclaringType).Location)).Name}][NORMAL ]{msg}";
            File.AppendAllText(fileName, "\r\n" + CombinedMsg);
        }

        public void Log(string msg, MessageType msgType)
        {
            System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace(true);
            var f = stack.GetFrame(1);
            string CombinedMsg = $"[{(new FileInfo(Assembly.GetAssembly(f.GetMethod().DeclaringType).Location)).Name}]";
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write((new FileInfo(Assembly.GetAssembly(f.GetMethod().DeclaringType).Location)).Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("][");
            switch (msgType)
            {
                case MessageType.Normal:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("NORAML ");
                    CombinedMsg += "[NORMAL ]";
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case MessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("WARNING");
                    CombinedMsg += "[WARNING]";
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR  ");
                    CombinedMsg += "[ERROR  ]";
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    break;
            }
            CombinedMsg += msg;
            Console.Write("]");
            Console.WriteLine(msg);
            File.AppendAllText(fileName, "\r\n"+CombinedMsg);
        }
    }
}
