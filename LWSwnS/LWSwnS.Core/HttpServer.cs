using LWSwnS.Api;
using LWSwnS.Api.Data;
using LWSwnS.Api.Web;
using LWSwnS.Configuration;
using LWSwnS.Core.Data;
using LWSwnS.Core.Extenstion;
using LWSwnS.Diagnostic;
using LWSwnS.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LWSwnS.Core
{
    public class HttpServer
    {
        public static X509Certificate serverCertificate = null;
        public static Version WebServerVersion = new Version(1, 2, 0, 0);
        public List<string> URLPrefix = new List<string>();
        public List<string> ExemptedFileTypes = new List<string>();
        public static Dictionary<string, IWebPage> WebPages = new Dictionary<string, IWebPage>();
        public List<TcpClientProcessor> tcpClients = new List<TcpClientProcessor>();
        public static UniversalConfigurationMark2 urlRules = new UniversalConfigurationMark2();
        TcpListener TCPListener;
        public event EventHandler<HttpRequestData> OnRequest;
        public HttpServer(TcpListener listener)
        {
            try
            {
                urlRules = UniversalConfigurationMark2.LoadFromFile("./Configs/URLRules.ini");
            }
            catch (Exception)
            {
            }
            if (ServerConfiguration.CurrentConfiguration.UseHttps == true)
            {
                Debugger.currentDebugger.Log(Language.GetString("General", "ExperimentalFeature", "You are using experimental feature that is untested: {featureName}.").Replace("{featureName}","HTTPS"), MessageType.Warning);
                serverCertificate = X509Certificate.CreateFromCertFile(ServerConfiguration.CurrentConfiguration.HttpsCert);
            }
                //serverCertificate = X509Certificate.CreateFromCertFile(ServerConfiguration.CurrentConfiguration.HttpsCert);
            ApiManager.AddFunction("IgnoreUrl", (UniParamater a) =>
            {
                URLPrefix.Add(a[0] as String);
                return new UniResult();
            });
            ApiManager.AddFunction("AddPersistentIgnoreUrlPrefix", (UniParamater a) =>
            {
                urlRules.AddItem("IgnorePrefix", a[0] as String);
                urlRules.SaveToFile("./Configs/URLRules.ini");
                return new UniResult();
            });
            ApiManager.AddFunction("AddPersistentRedirect", (UniParamater a) =>
            {
                urlRules.AddItem("Redirect." + a[0], a[1] as String);
                urlRules.SaveToFile("./Configs/URLRules.ini");
                return new UniResult();
            });
            ApiManager.AddFunction("AddPersistentIgnoreUrlSuffix", (UniParamater a) =>
            {
                urlRules.AddItem("IgnoreSuffix", a[0] as String);
                urlRules.SaveToFile("./Configs/URLRules.ini");
                return new UniResult();
            });
            ApiManager.AddFunction("ExemptFT", (UniParamater a) =>
            {
                if (!ExemptedFileTypes.Contains(a[0] as string))
                    ExemptedFileTypes.Add(a[0] as String);
                return new UniResult();
            });
            ApiManager.AddFunction("AddOnReq", (UniParamater p) =>
            {
                OnRequest += p[0] as EventHandler<HttpRequestData>;
                return new UniResult();
            });
            ApiManager.AddFunction("RmOnReq", (UniParamater p) =>
            {
                OnRequest -= p[0] as EventHandler<HttpRequestData>;
                return new UniResult();
            });
            TCPListener = listener;
            OnRequest += (a, b) =>
            {
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
                    if (b.requestUrl.Split('?')[0].ToUpper().EndsWith(item.ToUpper()))
                    {
                        return;
                    }
                }
                bool isMobile = false;
                if (ServerConfiguration.CurrentConfiguration.SplitModile == true)
                    isMobile = b.isMobile;
                var RealUrl = URLConventor.Convert(b.requestUrl.Trim(), isMobile);
                Debugger.currentDebugger.Log(Language.GetString("General", "HttpServer.Request", "Request:") + RealUrl);
                HttpResponseData httpResponseData = new HttpResponseData();
                try
                {
                    if (FileUtilities.GetFileFromURL(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder) != null)
                    {
                        if (!RealUrl.ToUpper().EndsWith(".ico".ToUpper()))
                        {
                            var content = File.ReadAllBytes(FileUtilities.GetFileFromURL(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder).FullName);
                            if (RealUrl.ToUpper().EndsWith(".html") || RealUrl.ToUpper().EndsWith(".htm"))
                            {
                                var str = Encoding.UTF8.GetString(content);
                                WebPagePresets.ApplyPreset(ref str);
                                content = Encoding.UTF8.GetBytes(str);
                            }
                            httpResponseData.content = content;

                        }
                    }
                    else if (FileUtilities.DirectoryExist(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder))
                    {
                        if (File.Exists(Path.Combine(RealUrl, "index.htm")))
                        {
                            string location = Path.Combine(RealUrl, "index.htm");
                            var content = File.ReadAllText(FileUtilities.GetFileFromURL(location, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder).FullName);
                            WebPagePresets.ApplyPreset(ref content);
                            httpResponseData.content = Encoding.UTF8.GetBytes(content);
                        }
                        else if (File.Exists(Path.Combine(RealUrl, "index.html")))
                        {
                            //httpResponseData.content = File.ReadAllBytes(Path.Combine(RealUrl, "index.html"));

                            string location = Path.Combine(RealUrl, "index.html");
                            var content = File.ReadAllText(FileUtilities.GetFileFromURL(location, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder).FullName);
                            WebPagePresets.ApplyPreset(ref content);
                            httpResponseData.content = Encoding.UTF8.GetBytes(content);
                        }
                    }
                    else
                    {
                        try
                        {

                            httpResponseData.content = Encoding.UTF8.GetBytes(SpecialPages.GetSpecialPage(KnownSpecialPages.Page404));
                        }
                        catch (Exception)
                        {
                        }
                    }


                }
                catch (Exception e)
                {
                    Debugger.currentDebugger.Log(e.Message, MessageType.Error);
                }
                httpResponseData.Additional = "Content-Type : text/html; charset=utf-8";
                if (RealUrl.ToUpper().EndsWith(".ico".ToUpper()))
                {
                    try
                    {

                        httpResponseData.Additional = "Content-Type: application/icon";
                        using (var fs = (FileUtilities.GetFileFromURL(RealUrl, isMobile ? URLConventor.MobileRootFolder : URLConventor.RootFolder)).Open(FileMode.Open))
                        {
                            httpResponseData.SendFile(ref b.streamWriter, fs);
                        }
                        //b.Processor.StopImmediately();
                        //It's a bit wired, without this, memory leak will occur.
                        //Can someone help me?
                    }
                    catch (Exception e)
                    {
                        Debugger.currentDebugger.Log("Error on sending ico: " + e.Message, Diagnostic.MessageType.Error);
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
        SslStream sslStream;
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
            if (ServerConfiguration.CurrentConfiguration.UseHttps)
            {
                try
                {
                    sslStream = new SslStream(networkStream);
                    sslStream.AuthenticateAsServer(HttpServer.serverCertificate, false, System.Security.Authentication.SslProtocols.Default, false);
                    streamReader = new StreamReader(sslStream);
                    streamWriter = new StreamWriter(sslStream);
                }
                catch (Exception e)
                {
                    Debugger.currentDebugger.Log(e.Message, MessageType.Error);

                    streamReader = new StreamReader(networkStream);
                    streamWriter = new StreamWriter(networkStream);
                }
            }
            else
            {
                streamReader = new StreamReader(networkStream);
                streamWriter = new StreamWriter(networkStream);
            }
            //SslStream ssl=new SslStream()
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
            s = streamReader.ReadLine();
            while (s != "" && s != null)
            {
                LS.Add(s);
                s = streamReader.ReadLine();
            }
            //Console.WriteLine("Readed:{0}.", LS.Count);
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
                        requestData.UA = LS[i].Substring("User-Agent:".Length).Trim();
                        requestData.isMobile = requestData.UA.IndexOf("Android") > 0 || requestData.UA.IndexOf("iPhone") > 0 || requestData.UA.IndexOf("Windows Phone") > 0 || requestData.UA.IndexOf("Lumia") > 0;
                    }
                    else if (LS[i].StartsWith("Range:"))
                    {
                        var rangeField = LS[i].Substring("Range:".Length).Trim();
                        rangeField = rangeField.Substring("bytes=".Length);
                        var ranges = rangeField.Split(',');
                        requestData.Range = new HttpRange();
                        foreach (var item in ranges)
                        {
                            var single = item.Trim();
                            var pair = single.Split('-');
                            long Lowest = 0;
                            long Highest = 0;
                            if (pair[0] == "")
                            {
                                Lowest = long.MinValue;
                            }
                            else Lowest = int.Parse(pair[0]);
                            if (pair[1] == "")
                            {
                                Highest = long.MinValue;
                            }
                            else Highest = long.Parse(pair[1]);
                            var SingleRange = new KeyValuePair<long, long>(Lowest, Highest);
                            requestData.Range.Ranges.Add(SingleRange);
                        }
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
                    if (rec.requestUrl.Split('?')[0].ToUpper().EndsWith("dll".ToUpper())&&ServerConfiguration.CurrentConfiguration.isDLLPageEnabled==true)
                    {
                        string path = rec.requestUrl.Split('?')[0];
                        path = URLConventor.Convert(path); string p = "";
                        try
                        {

                            p = rec.requestUrl.Substring(rec.requestUrl.IndexOf("?") + 1);

                        }
                        catch (Exception)
                        {
                        }
                        if (HttpServer.WebPages.ContainsKey(path))
                        {
                        }
                        else
                        {
                            if (File.Exists(rec.requestUrl.Split('?')[0]))
                            {
                                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath((new FileInfo(path)).FullName);
                                var types = asm.GetTypes();
                                foreach (var t in types)
                                {
                                    //t.
                                    if (typeof(IWebPage).IsAssignableFrom(t))
                                    {
                                        IWebPage wp = Activator.CreateInstance(t) as IWebPage;
                                        HttpServer.WebPages.Add(path, wp);
                                        break;
                                    }
                                }
                            }
                        }
                        string res = HttpServer.WebPages[path].Access(p, rec);
                        Debugger.currentDebugger.Log(Language.GetString("General", "HttpServer.Executed", "Executed:") + (new FileInfo(path)).Name + ": " + res);
                        continue;
                    }
                    else
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
