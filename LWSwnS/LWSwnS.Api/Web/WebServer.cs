using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LWSwnS.Api.Web
{
    public class WebServer
    {
        public static void AddPersistentIgnorePrefix(string s)
        {

            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(s);
            ApiManager.Functions["AddPersistentIgnoreUrlPrefix"](uniParamater);
        }
        public static void AddPersistentIgnoreSuffix(string s)
        {

            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(s);
            ApiManager.Functions["AddPersistentIgnoreUrlSuffix"](uniParamater);
        }
        public static void AddIgnoreUrlPrefix(string s)
        {
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(s);
            ApiManager.Functions["IgnoreUrl"](uniParamater);
        }
        public static void AddExemptFileType(string s)
        {
            if (!s.StartsWith(".")) s = "." + s;
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(s);
            ApiManager.Functions["ExemptFT"](uniParamater);
        }
        /// <summary>
        /// Still use EventHandler<HttpResponseData>
        /// </summary>
        /// <param name="e"></param>
        public static void AddHttpRequestHandler(Object e)
        {
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(e);
            ApiManager.Functions["AddOnReq"](uniParamater);
        }
    }
}
