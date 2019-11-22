using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LWSwnS.Core.Data
{
    public class HttpRequestData
    {
        public string requestUrl;
        public string UA;
        public HttpRequestType RequestType;
    }
    public class HttpResponseData
    {
        public byte[] content;
        public string StatusLine = "HTTP/1.1 200 OK";
        public string Additional = "HTTP/1.1 200 OK";
        public string Date = "Date: " + DateTime.Now.ToString();
        public void Send(ref StreamWriter writer)
        {
            writer.WriteLine(StatusLine);
            writer.WriteLine(Date);
            writer.WriteLine("Content-Length: " + content.Length);
            writer.WriteLine(Additional);
            writer.WriteLine();
            writer.Flush();

            writer.BaseStream.Write(content,0,content.Length);
            writer.BaseStream.Flush();
            //    for (int i = 0; i < content.Length; i+=4096)
            //    {
            //        byte[] a = new byte[4096];
            //        content.CopyTo(a, i);
            //        writer.Write(a);
            //        writer.Flush();
            //    }
            //}
            writer.Flush();
        }
    }
    public enum HttpRequestType
    {
        GET,POST
    }
}
