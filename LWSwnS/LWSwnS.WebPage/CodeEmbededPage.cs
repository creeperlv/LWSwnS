using LWSwnS.Api;
using LWSwnS.Diagnostic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LWSwnS.WebPage
{
    public class Parameter {
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
    }

    public class CodeEmbededPage
    {
        string PageFile;
        static Type LWSwnSCore;
        public static void SetLWSwnSCore(Type type)
        {
            LWSwnSCore = type;
        }
        public CodeEmbededPage(string pathToFile)
        {
            PageFile = pathToFile;
        }
        public async Task<string> ExecuteAndRetire(Assembly[] References,Parameter parameter=null)
        {
            //CSharpScript.
            var c = Resolve();
            string content = "";
            string[] imports = { "System" , "System.IO"  , "System.Collections.Generic" , "System.Linq", "System.Text" };
            Assembly[] assemblies = {  Assembly.GetAssembly(typeof(ApiManager)) , Assembly.GetAssembly(typeof(IDebugger)), Assembly.GetAssembly(typeof(Console)),Assembly.GetAssembly(LWSwnSCore )};
            if (parameter == null) parameter = new Parameter();
            ScriptState state = null;
            string para = "";
            try
            {
                para = "Dictionary<string,string> Parameter =new Dictionary<string,string>();";
                
                foreach (var item in parameter.Parameters)
                {
                    para += $"Parameter.Add(\"{item.Key}\",\"{item.Value}\");";
                }
            }
            catch (Exception)
            {
            }
            //if(false)
            //try
            //{

            //    state =
            //        await CSharpScript.RunAsync("foreach(var item in Parameters){Console.WriteLine(item.Key+\":\"+item.Value);}"
            //        , options: ScriptOptions.Default.WithImports(imports).AddReferences(assemblies).AddReferences(References),
            //        globals: parameter);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            foreach (var item in c)
            {
                if (item.type == 0)
                {
                    content += item.content;
                }
                else if (item.type == 1)
                {
                    if (state == null)
                        state = await CSharpScript.RunAsync(para+item.content, ScriptOptions.Default.WithImports(imports).AddReferences(assemblies).AddReferences(References));
                    else
                        await state.ContinueWithAsync(item.content);
                    
                    //content += state.ReturnValue;
                }
                else if (item.type == 2)
                {
                    //Console.WriteLine("Matching: " + item.content+" in "+state.Variables.Length+" Variable(s).");
                    try
                    {
                        foreach (var varible in state.Variables)
                        {
                            //Console.WriteLine("Compare: " + varible.Name);
                            if (varible.Name == item.content)
                            {
                                content += varible.Value;
                                break;
                            }
                        }

                    }
                    catch (Exception)
                    {}
                }
            }
            return content;
        }
        //Untested method...
        [Serializable]
        public struct CEPItem
        {
            public string content;
            public int type;
        }
        public List<CEPItem> Resolve()
        {

            var p = File.ReadAllText(PageFile);
            List<CEPItem> pageContent = new List<CEPItem> ();
            while (p.IndexOf("[CODE:") > 0|| p.IndexOf("[VALUE:") > 0)
            {

                if (p.IndexOf("[CODE:") < p.IndexOf("[VALUE:")&&p.IndexOf("[CODE:")>0)
                {

                    pageContent.Add(new CEPItem() { type = 0, content = p.Substring(0, p.IndexOf("[CODE:")) });
                    p = p.Substring(p.IndexOf("[CODE:") + "[CODE:".Length);
                    pageContent.Add(new CEPItem() { type = 1, content = p.Substring(0, p.IndexOf(":CODE]")) });
                    p = p.Substring(p.IndexOf(":CODE]") + ":CODE]".Length);
                }
                else
                {

                    pageContent.Add(new CEPItem() { type = 0, content = p.Substring(0, p.IndexOf("[VALUE:")) });
                    p = p.Substring(p.IndexOf("[VALUE:") + "[VALUE:".Length);
                    pageContent.Add(new CEPItem() { type = 2, content = p.Substring(0, p.IndexOf(":VALUE]")) });
                    p = p.Substring(p.IndexOf(":VALUE]") + ":VALUE]".Length);
                }
            }
            if (p.Length > 0)
            {
                pageContent.Add(new CEPItem() { type = 0, content = p });
            }
            return pageContent;
        }
    }
}
