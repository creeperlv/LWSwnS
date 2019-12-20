using LibGit2Sharp;
using LWSwnS.Api.Modules;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Diagnostic;
using Markdig;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGitViewer
{
    public class GitWebCore : ExtModule
    {
        public readonly static System.Version ModuleVer = new System.Version(0, 0, 2, 0);
        public static UniversalConfigurationMark2 repos = new UniversalConfigurationMark2();
        public static UniversalConfiguration Theme = new UniversalConfiguration();
        public static UniversalConfiguration Viewer = new UniversalConfiguration();
        string RootDir;
        string HomePage;
        string IndexPage;
        string CommitPage;
        string BrowserPage;
        string CommitItem;
        string RepoItem;
        string GitFileItem;
        string GitFolderItem;
        string about = "";
        public ModuleDescription InitModule()
        {
            ModuleDescription moduleDescription = new ModuleDescription();
            moduleDescription.Name = "Simple-Git-Viewer";
            moduleDescription.version = ModuleVer;

            RootDir = new FileInfo(Assembly.GetAssembly(this.GetType()).Location).Directory.FullName;
            try
            {
                Assembly.LoadFrom(Path.Combine(RootDir, "LibGit2Sharp.dll"));
            }
            catch (Exception e)
            {
                Debugger.currentDebugger.Log("Cannot load git lib, is LibGit2Sharp.dll exist?", MessageType.Warning);
            }
            WebServer.AddIgnoreUrlPrefix("/Git");
            Load();
            Tasks.RegisterTask(Load, Tasks.TaskType.Every5Seconds);
            //Task.Run(async () =>
            //{
            //    await Task.Delay(1000);
            //    Debugger.currentDebugger.Log("Auto reload task initialized.", MessageType.Normal);
            //    while (true)
            //    {
            //        await Task.Delay(5000);
            //        Load();
            //    }
            //});
            EventHandler<HttpRequestData> handler = (a, b) =>
            {
                try
                {
                    string content = "";
                    if (b.requestUrl.ToUpper().StartsWith("/Git".ToUpper()))
                    {
                        if (b.requestUrl.ToUpper() == "/GIT" || b.requestUrl.ToUpper() == "/GIT/")
                        {
                            try
                            {
                                about = Markdown.ToHtml(File.ReadAllText(Viewer.Get("AboutMD")));
                            }
                            catch (Exception)
                            {
                            }
                            string items = "";
                            if (about != "")
                            {
                                items += about;
                            }
                            foreach (var item in repos.Keys)
                            {
                                string url = "";
                                if (b.requestUrl.ToUpper() == "/GIT")
                                {
                                    url = "/GIT/";
                                }
                                url += item + "/";
                                string desc = "";
                                try
                                {
                                    desc = repos[item][1].Replace("\\n", "<br/>");
                                }
                                catch (Exception)
                                {
                                    desc = "No Description";
                                }
                                items += RepoItem.Replace("[REPOLINK]", url).Replace("[REPONAME]", item).Replace("[REPODESC]", desc);
                            }
                            content = IndexPage.Replace("[REPOLIST]", items);
                        }
                        else
                        {
                            string repo = b.requestUrl.Trim().Substring("/Git/".Length);
                            string repoAction = "";

                            if (repo.IndexOf("/") > 0)
                            {
                                repoAction = repo.Substring(repo.IndexOf("/") + 1);
                                repo = repo.Substring(0, repo.IndexOf("/"));
                            }
                            string repol = repos.GetValues(repo)[0];
                            using (var r = new Repository(repol))
                            {
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
                                    foreach (Commit c in r.Commits)
                                    {
                                        commitList += CommitItem.Replace("[ID]", c.Id.Sha).Replace("[AUTHOR]", c.Author.Name + "(" + c.Author.Email + ")").Replace("[MESSAGE]", c.Message.Replace("\n", "<br/>")).Replace("[DATE]", c.Author.When.ToString(RFC2822Format, CultureInfo.InvariantCulture));

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
                                    else location = Path.Combine(repol, repoAction.Substring("_git".Length).Replace('/', Path.DirectorySeparatorChar));
                                    location = location.Replace("%20", " ").Replace("%2B", "+");
                                    string items = "";
                                    try
                                    {
                                        if (Directory.Exists(location))
                                        {

                                            DirectoryInfo directoryInfo = new DirectoryInfo(location);
                                            //Directory.EnumerateFiles
                                            foreach (var item in directoryInfo.GetDirectories())
                                            {

                                                items += GitFolderItem.Replace("[ITEMNAME]", item.Name).Replace("[PATH]", item.Parent.FullName == new DirectoryInfo(repol).FullName ? $"{item.Name}" : $"{item.Parent.Name}/{item.Name}");
                                            }
                                            foreach (var item in directoryInfo.GetFiles())
                                            {
                                                string itemStr = GitFileItem.Replace("[ITEMNAME]", item.Name);
                                                foreach (var file in Theme.Keys)
                                                {
                                                    if (item.Name.ToUpper().EndsWith(file.ToUpper()))
                                                    {
                                                        itemStr = itemStr.Replace("[ICON]", Theme.Get(file));
                                                        break;
                                                    }
                                                }
                                                items += itemStr.Replace("[ICON]", Theme.Get("NormalFile")).Replace("[PATH]", item.Directory.FullName == new DirectoryInfo(repol).FullName ? $"{item.Name}" : $"{item.Directory.Name}/{item.Name}");
                                            }
                                        }
                                        else if(File.Exists(location))
                                        {
                                            FileInfo fileInfo = new FileInfo(location);
                                            if (fileInfo.Length / 1024 < 5 * 1024)
                                            {
                                                try
                                                {
                                                    if (fileInfo.Name.ToUpper().EndsWith(".PNG"))
                                                    {
                                                        items = $"<img width='100%' height='auto' src=\"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes(fileInfo.FullName))}\" />";
                                                    }
                                                    else
                                                    if (fileInfo.Name.ToUpper().EndsWith(".JPG"))
                                                    {
                                                        items = $"<img width='100%' height='auto' src=\"data:image/jpg;base64,{Convert.ToBase64String(File.ReadAllBytes(fileInfo.FullName))}\" />";
                                                    }
                                                    else
                                                        try
                                                        {
                                                            items = "<p>" + File.ReadAllText(fileInfo.FullName).Replace("\n", "<br/>").Replace(" ", "&nbsp;") + "</p>";
                                                        }
                                                        catch (Exception)
                                                        {
                                                        }

                                                }
                                                catch (Exception e)
                                                {
                                                    Debugger.currentDebugger.Log("Error on viewing:"+location+", "+e.Message, MessageType.Error);
                                                }
                                            }
                                            else
                                            {
                                                items = "<p>The target file it too large to view.</p>";
                                            }
                                        }
                                        else
                                        {
                                            items = "<p>404 NOT Found</p>";
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debugger.currentDebugger.Log(e.Message, MessageType.Warning);
                                    }
                                    content = BrowserPage.Replace("[REPONAME]", repo).Replace("[ITEMS]", items).Replace("[PATH]", repoAction.Substring("_git".Length));
                                }
                            }

                        }
                        try
                        {

                            HttpResponseData httpResponseData = new HttpResponseData();
                            httpResponseData.content = Encoding.UTF8.GetBytes(content);
                            httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                            httpResponseData.Send(ref b.streamWriter);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
                catch (Exception e)
                {
                    Debugger.currentDebugger.Log("Git Error:" + e.Message);
                }

            };
            WebServer.AddHttpRequestHandler(handler);
            return moduleDescription;
        }
        public void Load()
        {
            try
            {
                repos = UniversalConfigurationMark2.LoadFromFile("./Configs/SimpleGit.ini");
            }
            catch (Exception)
            {
            }
            try
            {
                Theme = UniversalConfigurationLoader.LoadFromFile("./Configs/SimpleGit.Theme.ini");
            }
            catch (Exception)
            {
            }
            try
            {
                Viewer = UniversalConfigurationLoader.LoadFromFile("./Configs/SimpleGit.Viewer.ini");
            }
            catch (Exception)
            {
            }
            HomePage = File.ReadAllText(Path.Combine(RootDir, "GitRepoHomePage.html"));
            IndexPage = File.ReadAllText(Path.Combine(RootDir, "HomePage.html"));
            CommitPage = File.ReadAllText(Path.Combine(RootDir, "CommitPage.html"));
            BrowserPage = File.ReadAllText(Path.Combine(RootDir, "BrowserPage.html"));
            CommitItem = File.ReadAllText(Path.Combine(RootDir, "CommitItem.html"));
            RepoItem = File.ReadAllText(Path.Combine(RootDir, "RepoItem.html"));
            GitFileItem = File.ReadAllText(Path.Combine(RootDir, "GitFileItem.html"));
            GitFolderItem = File.ReadAllText(Path.Combine(RootDir, "GitFolderItem.html"));
        }
    }
}
