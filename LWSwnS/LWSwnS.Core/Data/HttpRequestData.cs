using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LWSwnS.Core.Data
{
    public class HttpRequestData : CancelEventArgs
    {
        public TcpClientProcessor Processor;
        public string requestUrl;
        public string UA;
        public HttpRequestType RequestType;

        public bool willCancelNextHandle = false;
        public StreamWriter streamWriter;
    }
    public class HttpResponseData
    {
        public static int BufferSize = 4096;
        public bool SkipWhole;
        public bool ContentOnly;
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

            writer.BaseStream.Write(content, 0, content.Length);
            writer.BaseStream.Flush();
            writer.Flush();
        }
        public void SendFile(ref StreamWriter writer, FileStream reader)
        {
            writer.WriteLine(StatusLine);
            writer.WriteLine(Date);
            writer.WriteLine("Content-Length: " + reader.Length);
            writer.WriteLine(Additional);
            writer.WriteLine();
            writer.Flush();
            byte[] buffer = new byte[BufferSize];
            while (reader.Read(buffer, 0, buffer.Length) != 0)
            {
                writer.BaseStream.Write(buffer, 0, buffer.Length);
                writer.BaseStream.Flush();
                writer.Flush();
            }
            writer.BaseStream.Flush();
            writer.Flush();
        }
    }
    public enum HttpRequestType
    {
        GET, POST
    }
}
