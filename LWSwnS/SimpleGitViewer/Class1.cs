using LibGit2Sharp;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using Markdig;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleGitViewer
{
    public class GitWebCore : ExtModule
    {
        public readonly static System.Version ModuleVer = new System.Version(0, 0, 1, 0);
        public static UniversalConfigurationMark2 config = new UniversalConfigurationMark2();
        public static UniversalConfiguration Theme= new UniversalConfiguration();
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
            try
            {
                Theme = UniversalConfigurationLoader.LoadFromFile("./SimpleGit.Theme.ini");
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
            string CommitPage = File.ReadAllText(Path.Combine(RootDir, "CommitPage.html"));
            string BrowserPage = File.ReadAllText(Path.Combine(RootDir, "BrowserPage.html"));
            string CommitItem = File.ReadAllText(Path.Combine(RootDir, "CommitItem.html"));
            string GitFileItem = File.ReadAllText(Path.Combine(RootDir, "GitFileItem.html"));
            string GitFolderItem = File.ReadAllText(Path.Combine(RootDir, "GitFolderItem.html"));
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
                        Console.WriteLine(repo + ":" + repoAction+" in "+ config.GetValues(repo)[0]);
                        string repol = config.GetValues(repo)[0];
                        using (var r = new Repository(repol))
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
                                    content = content.Replace("[README.MD]", Markdown.ToHtml(File.ReadAllText(Path.Combine(repol, "Readme.md"))));
                                }
                                catch (Exception)
                                {
                                }
                            }
                            else if (repoAction.ToUpper().StartsWith("Commits".ToUpper()))
                            {
                                var RFC2822Format = "ddd dd MMM HH:mm:ss yyyy K";
                                string commitList = "";
                                foreach (Commit c in r.Commits.Take(15))
                                {
                                    commitList += CommitItem.Replace("[ID]",c.Id.Sha).Replace("[AUTHOR]",c.Author.Name+"("+c.Author.Email+")").Replace("[MESSAGE]",c.Message.Replace("\n","<br/>")).Replace("[DATE]", c.Author.When.ToString(RFC2822Format, CultureInfo.InvariantCulture));

                                }
                                content = CommitPage.Replace("[REPONAME]", repo).Replace("[Commits]", commitList);
                            }
                            else if (repoAction.ToUpper().StartsWith("_git".ToUpper()))
                            {
                                string location;
                                if (repoAction.IndexOf('/') > 0)
                                {
                                    location = Path.Combine(repol, repoAction.Substring("_git/".Length).Replace('/', Path.DirectorySeparatorChar));
                                }
                                else location=Path.Combine(repol, repoAction.Substring("_git".Length).Replace('/',Path.DirectorySeparatorChar));
                                string items = "";
                                try
                                {
                                    DirectoryInfo directoryInfo = new DirectoryInfo(location);
                                    //Directory.EnumerateFiles
                                    foreach (var item in directoryInfo.GetDirectories())
                                    {
                                        
                                        items += GitFolderItem.Replace("[ITEMNAME]", item.Name).Replace("[PATH]", item.Parent.FullName==new DirectoryInfo(repol).FullName? $"_git/{item.Name}" : $"{item.Parent.Name}/{item.Name}");
                                    }
                                    foreach (var item in directoryInfo.GetFiles())
                                    {
                                        string itemStr= GitFileItem.Replace("[ITEMNAME]", item.Name);
                                        foreach (var file in Theme.Keys)
                                        {
                                            if (item.Name.ToUpper().EndsWith(file.ToUpper()))
                                            {
                                                itemStr = itemStr.Replace("[ICON]", Theme.Get(file));
                                                break;
                                            }
                                        }
                                        items += itemStr.Replace("[ICON]",Theme.Get("NormalFile"));
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                                content = BrowserPage.Replace("[REPONAME]", repo).Replace("[ITEMS]", items).Replace("[PATH]", repoAction.Substring("_git".Length));
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
