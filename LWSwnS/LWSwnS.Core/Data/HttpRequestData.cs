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
        public string ContentType;
        public string ContentLength;
        public string ContentLocation;
        public HttpRange Range = new HttpRange();
        public string UA;
        public HttpRequestType RequestType;
        public bool isMobile = false;
        public bool willCancelNextHandle = false;
        public StreamWriter streamWriter;
        public List<Cookie> Cookies = new List<Cookie>();
    }
    public class Cookie {
        public string name;
        public string value;
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
        public string Additional = "";
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
        public void SendFileInRange(ref StreamWriter writer, FileStream reader, KeyValuePair<long, long> SingleRange, int speedLimit = int.MaxValue)
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
            int count = 0;
            bool wait = false;
            DateTime LastCompare = DateTime.Now;
            while (retireValue != 0 && StopImmediately == false)
            {
                if (wait == false)
                {

                    long targetLength = buffer.Length;
                    if (reader.Position + buffer.Length > Length)
                    {
                        targetLength = Length - reader.Position;
                        StopImmediately = true;
                    }
                    retireValue = reader.Read(buffer, 0, (int)targetLength);

                    if (speedLimit != int.MaxValue)
                    {
                        count += retireValue;
                        if (count >= speedLimit)
                        {
                            var t = DateTime.Now - LastCompare;
                            if (t > TimeSpan.FromSeconds(1))
                            {
                                count = 0;
                            }
                            else
                            {
                                wait = true;
                            }
                        }
                    }
                    writer.BaseStream.Write(buffer, 0, buffer.Length);
                    writer.BaseStream.Flush();
                    writer.Flush();
                }
                else
                {
                    if (count >= speedLimit)
                    {
                        var t = DateTime.Now - LastCompare;
                        if (t > TimeSpan.FromSeconds(1))
                        {
                            count = 0;
                            wait = false;
                        }
                    }
                }
            }
            writer.BaseStream.Flush();
            writer.Flush();
        }
    }
    public enum HttpRequestType
    {
        GET, POST, PUT
    }
}
