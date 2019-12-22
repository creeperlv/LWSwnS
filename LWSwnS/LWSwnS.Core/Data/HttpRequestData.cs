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
        public HttpRange Range=new HttpRange();
        public string UA;
        public HttpRequestType RequestType;
        public bool isMobile = false;
        public bool willCancelNextHandle = false;
        public StreamWriter streamWriter;
    }
    public class HttpRange
    {
        public string type = "bytes";
        public List<KeyValuePair<long, long>> Ranges = new List<KeyValuePair<long, long>>();
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
        public void SendFileInRange(ref StreamWriter writer, FileStream reader,KeyValuePair<long,long> SingleRange)
        {
            writer.WriteLine(StatusLine);
            writer.WriteLine(Date);
            long Length = reader.Length;
            long startPosition = 0;
            long endPosition = reader.Length;
            if (SingleRange.Key == long.MinValue)
            {

            }
            else
            {
                startPosition = SingleRange.Key;
            }
            if (SingleRange.Value != long.MinValue)
            {
                endPosition = SingleRange.Value;
            }
            Length = endPosition - startPosition;
            writer.WriteLine("Content-Length: " + Length);
            writer.WriteLine(Additional);
            writer.WriteLine();
            writer.Flush();
            byte[] buffer = new byte[BufferSize];
            reader.Position = startPosition;
            int retireValue = 1;
            bool StopImmediately = false;
            while (retireValue != 0&&StopImmediately==false)
            {
                long targetLength = buffer.Length;
                if (reader.Position + buffer.Length > Length)
                {
                    targetLength=Length-reader.Position;
                    StopImmediately = true;
                }
                retireValue = reader.Read(buffer, 0, (int)targetLength);
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
