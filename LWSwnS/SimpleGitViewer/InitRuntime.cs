using LWSwnS.Api.Modules;
using LWSwnS.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace SimpleGitViewer
{
    class InitRuntime : FirstInit
    {
        public void Init()
        {
            Console.WriteLine("Generating Default Theme...");
            UniversalConfiguration conf = new UniversalConfiguration();
            conf.Add("NormalFile", "https://img.icons8.com/color/64/000000/file.png");
            conf.Add("cs", "https://img.icons8.com/color/48/000000/c-plus-plus-logo.png");
            conf.Add("java", "https://img.icons8.com/color/64/000000/code-file.png");
            conf.Add("png", "https://img.icons8.com/cute-clipart/64/000000/image-file.png");
            conf.Add("jpg", "https://img.icons8.com/cute-clipart/64/000000/image-file.png");
            conf.Add("bmp", "https://img.icons8.com/cute-clipart/64/000000/image-file.png");
            conf.Add("prefab", "https://img.icons8.com/office/16/000000/sugar-cube.png");
            conf.SaveToFile("./SimpleGit.Theme.ini");
            Console.WriteLine("Copying runtime...");
            string RootDir = new FileInfo(Assembly.GetAssembly(this.GetType()).Location).Directory.FullName;
            CopyDirectory(Path.Combine(RootDir, "runtimes"), "./runtimes");
            Console.WriteLine("Completed.");
        }
        public void CopyDirectory(string SRC, string TARGET)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(SRC);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                foreach (FileSystemInfo item in fileinfo)
                {
                    if (item is DirectoryInfo)
                    {
                        if (!Directory.Exists(Path.Combine(TARGET, item.Name)))
                        {
                            Directory.CreateDirectory(Path.Combine(TARGET, item.Name));
                        }
                        CopyDirectory(item.FullName, Path.Combine(TARGET, item.Name));
                    }
                    else
                    {
                        File.Copy(item.FullName, Path.Combine(TARGET, item.Name), true);
                        Console.Write(item.Name);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(" [OK]");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error happens:"+e);
            }
        }
    }
}
