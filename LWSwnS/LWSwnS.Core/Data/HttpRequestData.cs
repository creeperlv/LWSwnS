using System;
using System.Collections.Generic;
using System.Text;

namespace LWSwnS.Core.Data
{
    public class HttpRequestData
    {
        public string requestUrl;
        public string UA;
        public HttpRequestType RequestType;
    }
    public enum HttpRequestType
    {
        GET,POST
    }
}
