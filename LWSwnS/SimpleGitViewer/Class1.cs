using LibGit2Sharp;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using Markdig;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace SimpleGitViewer
{
    public class GitWebCore : ExtModule
    {
        public readonly static System.Version ModuleVer = new System.Version(0, 0, 1, 0);
        public static UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "Simple-Git-Viewer";
            moduleDescription.version = ModuleVer;
            try
            {
                config = UniversalConfigurationMark2.LoadFromFile("./SimpleGit.ini");
            }
            catch (Exception)
            {
            }
            string RootDir = new FileInfo(Assembly.GetAssembly(this.GetType()).Location).Directory.FullName;
            try
            {
                Assembly.LoadFrom(Path.Combine(RootDir, "LibGit2Sharp.dll"));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Cannot load git lib, is LibGit2Sharp.dll exist?");
                Console.ForegroundColor = ConsoleColor.White;
            }
            WebServer.AddIgnoreUrlPrefix("/Git");
            string HomePage = File.ReadAllText(Path.Combine(RootDir, "GitRepoHomePage.html"));
            EventHandler<HttpRequestData> handler = (a, b) =>
            {
                try
                {
                    if (b.requestUrl.ToUpper().StartsWith("/Git".ToUpper()))
                    {
                        string repo = b.requestUrl.Trim().Substring("/Git/".Length);
                        string repoAction = "";

                        if (repo.IndexOf("/") > 0)
                        {
                            repoAction = repo.Substring(repo.IndexOf("/") + 1);
                            repo = repo.Substring(0, repo.IndexOf("/"));
                        }
                        Console.WriteLine(repo + ":" + repoAction);
                        using (var r = new Repository(config.GetValues(repo)[0]))
                        {
                            string content = "";
                            if (repoAction == "")
                            {
                                content = HomePage;
                                string branches = "";
                                foreach (Branch branch in r.Branches)
                                {
                                    branches += $"<li>{branch.FriendlyName}</li>";
                                }

                                content = content.Replace("[Branches]", branches);
                                content = content.Replace("[REPONAME]", repo);
                                try
                                {
                                    content = content.Replace("[README.MD]", Markdown.ToHtml(File.ReadAllText(Path.Combine(config.GetValues(repo)[0], "Readme.md"))));
                                }
                                catch (Exception)
                                {
                                }
                            }
                            else if (repoAction.StartsWith("Commits"))
                            {

                            }
                            HttpResponseData httpResponseData = new HttpResponseData();
                            httpResponseData.content = Encoding.UTF8.GetBytes(content);
                            httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                            httpResponseData.Send(ref b.streamWriter);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Git Error:" + e.Message);
                }

            };
            WebServer.AddHttpRequestHandler(handler);
            return moduleDescription;
        }
    }
}
