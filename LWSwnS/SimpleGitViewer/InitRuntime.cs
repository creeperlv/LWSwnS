using LWSwnS.Api.Modules;
using System;
using System.IO;
using System.Reflection;

namespace SimpleGitViewer
{
    class InitRuntime : FirstInit
    {
        public void Init()
        {
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
