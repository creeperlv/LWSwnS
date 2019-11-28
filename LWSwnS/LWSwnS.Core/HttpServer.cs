﻿using LWSwnS.Api;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LWSwnS.Core
{
    public class HttpServer
    {
        public List<string> URLPrefix = new List<string>();
        public List<string> ExemptedFileTypes = new List<string>();
        public List<TcpClientProcessor> tcpClients = new List<TcpClientProcessor>();
        public static UniversalConfigurationMark2 urlRules = new UniversalConfigurationMark2();
        TcpListener TCPListener;
        public event EventHandler<HttpRequestData> OnRequest;
        public HttpServer(TcpListener listener)
        {
            try
            {
                urlRules = UniversalConfigurationMark2.LoadFromFile("./URLRules.ini");
            }
            catch (Exception)
            {
            }
            ApiManager.AddFunction("IgnoreUrl", (UniParamater a) =>
            {
                URLPrefix.Add(a[0] as String);
                return new UniResult();
            });
            ApiManager.AddFunction("AddPersistentIgnoreUrlPrefix", (UniParamater a) =>
            {
                urlRules.AddItem("IgnorePrefix", a[0] as String);
                urlRules.SaveToFile("./URLRules.ini");
                return new UniResult();
            });
            ApiManager.AddFunction("AddPersistentRedirect", (UniParamater a) =>
            {
                urlRules.AddItem("Redirect." + a[0], a[1] as String);
                urlRules.SaveToFile("./URLRules.ini");
                return new UniResult();
            });
            ApiManager.AddFunction("AddPersistentIgnoreUrlSuffix", (UniParamater a) =>
            {
                urlRules.AddItem("IgnoreSuffix", a[0] as String);
                urlRules.SaveToFile("./URLRules.ini");
                return new UniResult();
            });
            ApiManager.AddFunction("ExemptFT", (UniParamater a) =>
            {
                ExemptedFileTypes.Add(a[0] as String);
                return new UniResult();
            });
            ApiManager.AddFunction("AddOnReq", (UniParamater p) =>
            {
                OnRequest += p[0] as EventHandler<HttpRequestData>;
                return new UniResult();
            });
            TCPListener = listener;
            OnRequest += (a, b) =>
            {
                var RealUrl = URLConventor.Convert(b.requestUrl.Trim());
                foreach (var item in URLPrefix)
                {
                    if (b.requestUrl.ToUpper().StartsWith(item.ToUpper()))
                    {
                        return;
                    }
                }
                foreach (var item in ExemptedFileTypes)
                {
                    if (b.requestUrl.ToUpper().EndsWith(item.ToUpper()))
                    {
                        return;
                    }
                }
                foreach (var item in urlRules.GetValues("IgnorePrefix"))
                {
                    if (b.requestUrl.ToUpper().StartsWith(item.ToUpper()))
                    {
                        return;
                    }
                }
                foreach (var item in urlRules.GetValues("IgnoreSuffix"))
                {
                    if (b.requestUrl.ToUpper().EndsWith(item.ToUpper()))
                    {
                        return;
                    }
                }
                Console.WriteLine("Request:" + RealUrl);
                HttpResponseData httpResponseData = new HttpResponseData();
                if (File.Exists(RealUrl))
                {
                    if (!RealUrl.ToUpper().EndsWith(".ico".ToUpper()))
                        httpResponseData.content = File.ReadAllBytes(RealUrl);
                }
                else if (File.Exists(Path.Combine(RealUrl, "index.htm")))
                {
                    httpResponseData.content = File.ReadAllBytes(Path.Combine(RealUrl, "index.htm"));
                }
                else if (File.Exists(Path.Combine(RealUrl, "index.html")))
                {
                    httpResponseData.content = File.ReadAllBytes(Path.Combine(RealUrl, "index.html"));
                }
                else
                {
                    httpResponseData.content = File.ReadAllBytes("./404.html");
                }

                httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                if (RealUrl.ToUpper().EndsWith(".ico".ToUpper()))
                {
                    try
                    {

                        httpResponseData.Additional = "Content-Type: application/icon";
                        using (var fs = (new FileInfo(RealUrl)).Open(FileMode.Open))
                        {
                            httpResponseData.SendFile(ref b.streamWriter, fs);
                        }
                        b.Processor.StopImmediately();
                        //It's a bit wired, without this, memory leak will occur.
                        //Can someone help me?
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error on sending ico: " + e.Message);
                    }
                }
                else
                {

                    httpResponseData.Send(ref b.streamWriter);
                }
            };
        }
        public void Stop()
        {
            foreach (var item in tcpClients)
            {
                item.StopImmediately();
            }
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
                    var c = new TcpClientProcessor(a, this);
                    tcpClients.Add(c);
                }
            });
        }
        public void HandleRequest(HttpRequestData data)
        {

            this.OnRequest(this, data);
        }
    }
    public class TcpClientProcessor
    {
        NetworkStream networkStream;
        StreamReader streamReader;
        StreamWriter streamWriter;
        TcpClient currentClient;
        HttpServer FatherServer;
        public TcpClientProcessor(TcpClient client, HttpServer server)
        {
            FatherServer = server;
            currentClient = client;
            networkStream = currentClient.GetStream();
            //networkStream.CanTimeout = true;
            networkStream.ReadTimeout = 3000;
            networkStream.WriteTimeout = 3000;
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);
            Task.Run(MainThread);
        }
        bool willStop = false;
        public void StopImmediately()
        {
            willStop = true;
            try
            {

                streamReader.Close();
                streamReader.Dispose();

            }
            catch (Exception)
            {
            }
            try
            {

                streamWriter.Close();
                streamWriter.Dispose();

            }
            catch (Exception)
            {
            }
            try
            {

                networkStream.Close();
                networkStream.Dispose();

            }
            catch (Exception)
            {
            }
            GC.Collect();
            FinalizeStop();
        }
        public void SoftStop()
        {
            willStop = true;
        }
        public void FinalizeStop()
        {
            try
            {

                FatherServer.tcpClients.Remove(this);
                GC.Collect();
            }
            catch (Exception)
            {

            }
        }
        HttpRequestData ReceiveMessage()
        {
            HttpRequestData requestData = new HttpRequestData();
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
                    }
                    else
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
            requestData.streamWriter = this.streamWriter;
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
                    rec.Processor = this;
                    FatherServer.HandleRequest(rec);
                    //httpResponseData = null;
                    GC.Collect();
                }
                catch (Exception)
                {
                    Console.WriteLine("Disastrous Error!");
                    StopImmediately();
                }
            }
            StopImmediately();
            GC.Collect();
        }

    }

}
