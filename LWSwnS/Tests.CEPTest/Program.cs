using LWSwnS.WebPage;
using System;
using System.Collections.Generic;

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
            //foreach (var item in pc)
            //{
            //    Console.WriteLine((item.type==0?"[HTML]":"[C#]")+item.content);
            //}
            Parameter parameter = new Parameter();
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("KeyWord", "Site-13");
            parameter.Parameters = para;
            var a=codeEmbededPage.ExecuteAndRetire(new System.Reflection.Assembly[0],parameter);
            a.Wait();
            Console.WriteLine(a.Result);
        }
    }
}
