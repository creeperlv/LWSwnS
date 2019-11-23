using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LWSwnS.Api.Web
{
    public class WebServer
    {
        public static void AddIgnoreUrlPrefix(string s)
        {
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(s);
            ApiManager.Functions["IgnoreUrl"](uniParamater);
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
