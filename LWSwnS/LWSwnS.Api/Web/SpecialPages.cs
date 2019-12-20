using LWSwnS.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Api.Web
{
    public class SpecialPages
    {
        public static string GetSpecialPage(KnownSpecialPages knownSpecialPages)
        {
            switch (knownSpecialPages)
            {
                case KnownSpecialPages.Page404:
                    {
                        try
                        {
                            return File.ReadAllText(ServerConfiguration.CurrentConfiguration.Page404);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    break;
                default:
                    break;
            }
            return "";
        }
    }
    public enum KnownSpecialPages
    {
        Page404
    }
}
