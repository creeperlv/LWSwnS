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
            networkStream.Close();
            networkStream.Dispose();
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
            while (willStop == false)
            {
                try
                {
                    var rec = ReceiveMessage();
                    var RealUrl = URLConventor.Convert(rec.requestUrl.Trim());
                    Console.WriteLine("Resuest location:" + RealUrl);
                    HttpResponseData httpResponseData = new HttpResponseData();
                    Console.WriteLine("Resuest location:" + Path.Combine(RealUrl, "index.htm"));
                    Console.WriteLine("Resuest location:" + Path.Combine(RealUrl, "index.html"));
                    if (File.Exists(RealUrl))
                    {
                        Console.WriteLine("It is a file");
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
                        throw new Exception("Wrong Request");
                    }


                    httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                    httpResponseData.Send(ref streamWriter);

                }
                catch (Exception e)
                {
                    try
                    {

                        HttpResponseData httpResponseData = new HttpResponseData();
                        httpResponseData.StatusLine = "HTTP/1.1 404 Error";
                        httpResponseData.content = System.Text.Encoding.Default.GetBytes($"<html><body><p>Error: {e.Message}</p></body><html>");
                        httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                        httpResponseData.Send(ref streamWriter);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Disastrous Error!");
                    }
                }
            }
        }

    }

}
