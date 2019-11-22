using System;
using System.Collections.Generic;
using System.IO;
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
        TcpClient currentClient;
        public TcpClientProcessor(TcpClient client)
        {
            currentClient = client;
            networkStream = currentClient.GetStream();
            streamReader = new StreamReader(networkStream);
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
        List<String> ReceiveMessage()
        {
            List<String> LS = new List<string>();
            string s;
            while ((s = streamReader.ReadLine()) != "")
            {
                LS.Add(s);
            }
            Console.WriteLine("Read Completed, Lines:"+LS.Count);
            return LS;
        }
        void MainThread()
        {
            //Receive-Response
            while (willStop == false)
            {
                try
                {
                    var rec=ReceiveMessage();
                }
                catch (Exception)
                {
                }
            }
        }

    }

}
