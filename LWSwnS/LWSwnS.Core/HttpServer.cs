using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LWSwnS.Core
{
    public class HttpServer
    {
        List<TcpClientProcessor> tcpClients = new List<TcpClientProcessor>();
        TcpListener TCPListener;
        public HttpServer(TcpListener listener)
        {
            TCPListener = listener;
        }
        bool WebStop = false;
        public void StartListen()
        {
            Task.Run(() =>
            {
                TCPListener.Start();
                while (WebStop == false)
                {
                    var a = TCPListener.AcceptTcpClient();
                    var c = new TcpClientProcessor(a);
                    tcpClients.Add(c);
                }
            });
        }
    }
    public class TcpClientProcessor
    {
        NetworkStream networkStream;
        StreamReader streamReader;
        StreamWriter streamWriter;
        TcpClient currentClient;
        public TcpClientProcessor(TcpClient client)
        {
            currentClient = client;
            networkStream = currentClient.GetStream();
            //networkStream.CanTimeout = true;
            networkStream.ReadTimeout=3000;
            networkStream.WriteTimeout= 3000;
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);
            Task.Run(MainThread);
        }
        bool willStop = false;
        public void StopImmediately()
        {
            willStop = true;
            streamReader.Close();
            streamReader.Dispose();
            streamWriter.Close();
            streamWriter.Dispose();
            networkStream.Close();
            networkStream.Dispose();
            System.GC.Collect();
        }
        public void SoftStop()
        {
            willStop = true;
        }
        HttpRequestData ReceiveMessage()
        {
            HttpRequestData requestData=new HttpRequestData();
            List<String> LS = new List<string>();
            string s;
            while ((s = streamReader.ReadLine()) != "")
            {
                LS.Add(s);
            }
            for (int i = 0; i < LS.Count; i++)
            {
                //Console.WriteLine(LS[i]);
                if (i == 0)
                {
                    if (LS[i].StartsWith("GET "))
                    {
                        requestData.requestUrl = LS[i].Substring("GET ".Length, LS[i].Length - "GET ".Length - " HTTP/1.1".Length);
                        requestData.RequestType = HttpRequestType.GET;
                    }else
                    if (LS[i].StartsWith("POST "))
                    {
                        requestData.requestUrl = LS[i].Substring("POST ".Length, LS[i].Length - "POST ".Length - " HTTP/1.1".Length);
                        requestData.RequestType = HttpRequestType.GET;
                    }
                }
                else
                {
                    if (LS[i].StartsWith("User-Agent: "))
                    {
                        requestData.UA = LS[i].Substring("User-Agent: ".Length);
                    }
                }
            }
            Console.WriteLine("Read Completed, Lines:"+LS.Count);
            return requestData;
        }

        void MainThread()
        {
            //Receive-Response
            Console.WriteLine("New Thread Started.");
            while (willStop == false)
            {
                try
                {
                    var rec = ReceiveMessage();
                    var RealUrl = URLConventor.Convert(rec.requestUrl.Trim());
                    Console.WriteLine("Request:"+RealUrl);
                    HttpResponseData httpResponseData = new HttpResponseData();
                    if (File.Exists(RealUrl))
                    {
                        httpResponseData.content = File.ReadAllBytes(RealUrl);
                    }
                    else if(File.Exists(Path.Combine(RealUrl, "index.htm")))
                    {
                        httpResponseData.content = File.ReadAllBytes(Path.Combine(RealUrl, "index.htm"));
                    }
                    else if(File.Exists(Path.Combine(RealUrl, "index.html")))
                    {
                        httpResponseData.content = File.ReadAllBytes(Path.Combine(RealUrl, "index.html"));
                    }else
                    {
                        httpResponseData.content = File.ReadAllBytes("./404.html");
                    }
                    httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                    httpResponseData.Send(ref streamWriter);
                    httpResponseData = null;
                    System.GC.Collect();
                    StopImmediately();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Disastrous Error!");
                    StopImmediately();
                }
            }
            System.GC.Collect();
        }

    }

}
