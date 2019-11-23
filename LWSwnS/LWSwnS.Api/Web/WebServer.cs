using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LWSwnS.Api.Web
{
    public class WebServer
    {
        public static void AddHttpRequestHandler(EventHandler e)
        {
            UniParamater uniParamater = new UniParamater();
            uniParamater.Add(e);
            ApiManager.Functions["AddOnReq"](uniParamater);
        }
    }
}
