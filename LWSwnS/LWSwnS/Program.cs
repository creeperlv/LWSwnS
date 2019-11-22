using LWSwnS.Core;
using System;

namespace LWSwnS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test Field");
            LWSwnSServerCore a = new LWSwnSServerCore("0.0.0.0", 80, 9341);
            a.StartListenWeb();
            while (Console.ReadLine()!="Exit")
            {

            }
        }
    }
}
