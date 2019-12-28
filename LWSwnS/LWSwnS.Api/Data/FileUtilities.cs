using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Api.Data
{
    public class FileUtilities
    {
        public static bool DirectoryExist(string location, string baselocation)
        {
            string locationF = location;
            locationF = locationF.Substring(baselocation.Length);
            if (locationF.StartsWith("/"))
            {
                locationF = locationF.Substring(1);
            }
            var paths = locationF.Split('/');

            DirectoryInfo directoryInfo = new DirectoryInfo(baselocation);
            for (int i = 0; i < paths.Length; i++)
            {
                if (i != paths.Length - 1)
                {
                    bool find = false;
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                        {
                            find = true;
                            directoryInfo = item;
                            if (paths[i + 1] == "")
                            {
                                return true;
                            }
                            break;
                        }
                    }
                    if (find == false) return false;
                }
                else
                {
                    if (paths[i] == "")
                    {
                        return true;
                    }
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                            return true;
                    }
                }
            }
            return false;
        }
        public static string AssemblyLocation { get; private set; }
        public static string ConvertRelativeToAbsolute(string origin)
        {
            return origin.Replace("./", AssemblyLocation);
        }
        public static void InitLocation(Type t)
        {
            AssemblyLocation = new FileInfo(t.Assembly.Location).Directory.FullName;
            if (!AssemblyLocation.EndsWith("/")) AssemblyLocation += "/";
        }
        public static DirectoryInfo GetFolderFromURL(string location, string baselocation)
        {
            string locationF = location;
            locationF = locationF.Substring(baselocation.Length);
            if (locationF.StartsWith("/"))
            {
                locationF = locationF.Substring(1);
            }if (locationF.EndsWith("/"))
            {
                locationF = locationF.Remove(locationF.Length-1);
            }
            var paths = locationF.Split('/');

            DirectoryInfo directoryInfo = new DirectoryInfo(baselocation);
            if (locationF == "")
            {
                return directoryInfo;
            }
            for (int i = 0; i < paths.Length; i++)
            {
                if (i != paths.Length - 1)
                {
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                        {
                            directoryInfo = item;
                            break;
                        }
                    }

                }
                else
                {
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                            return item;
                    }
                }
            }
            return null;
        }
        public static FileInfo GetFileFromURL(string location, string baselocation)
        {
            string locationF = location;
            locationF = locationF.Substring(baselocation.Length);
            if (locationF.StartsWith("/"))
            {
                locationF = locationF.Substring(1);
            }
            var paths = locationF.Split('/');

            DirectoryInfo directoryInfo = new DirectoryInfo(baselocation);
            for (int i = 0; i < paths.Length; i++)
            {
                if (i != paths.Length - 1)
                {
                    foreach (var item in directoryInfo.GetDirectories())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                        {
                            directoryInfo = item;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var item in directoryInfo.GetFiles())
                    {
                        if (item.Name.ToUpper() == paths[i].ToUpper())
                            return item;
                    }
                }
            }
            return null;
        }
    }
}
