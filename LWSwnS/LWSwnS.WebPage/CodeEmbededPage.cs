using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LWSwnS.WebPage
{
    public class CodeEmbededPage
    {
        string PageFile;
        public CodeEmbededPage(string pathToFile)
        {
            PageFile = pathToFile;
        }
        public async Task<string> ExecuteAndRetire()
        {
            //CSharpScript.
            var c = Resolve();
            string content = "";
            string[] imports = { "System" , "System.IO" , "LWSwnS.API" , "System.Collections.Generic" , "System.Text" , "LWSwnS.Diagnostic"};
            ScriptState state = null;
            foreach (var item in c)
            {
                if (item.type == 0)
                {
                    content += item.content;
                }
                else if (item.type == 1)
                {
                    if (state == null)
                        state = await CSharpScript.RunAsync(item.content,ScriptOptions.Default.WithImports(imports));
                    else await state.ContinueWithAsync(item.content);
                    
                    content += state.ReturnValue;
                }
                else if (item.type == 2)
                {
                    foreach (var varible in state.Variables)
                    {
                        if (varible.Name == item.content)
                        {
                            content += varible.Value;
                            break;
                        }
                    }
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
