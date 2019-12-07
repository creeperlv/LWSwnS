using LWSwnS.WebPage;
using System;

namespace Tests.CEPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CEP - Code Embeded Page");
            Console.WriteLine("Please specify a page:");
            string cep=Console.ReadLine();
            CodeEmbededPage codeEmbededPage = new CodeEmbededPage(cep);

            var pc = codeEmbededPage.Resolve();
            foreach (var item in pc)
            {
                Console.WriteLine((item.type==0?"[HTML]":"[C#]")+item.content);
            }
            var a=codeEmbededPage.ExecuteAndRetire(new System.Reflection.Assembly[0]);
            a.Wait();
            Console.WriteLine(a.Result);
        }
    }
}
