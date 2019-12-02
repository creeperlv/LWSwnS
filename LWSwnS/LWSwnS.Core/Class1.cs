using LWSwnS.Diagnostic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LWSwnS.Core
{
    public class LWSwnSServerCore
    {
        TcpListener Web;
        TcpListener Shell;
        //Service
        public LWSwnSServerCore(string Address,int WebPort,int ShellPort)
        {
            {

                var ip = IPAddress.Parse(Address);
                var Endpoint = new IPEndPoint(ip, WebPort);
                Web = new TcpListener(Endpoint);

            }
            {

                var ip = IPAddress.Parse(Address);
                var Endpoint = new IPEndPoint(ip, ShellPort);
                Shell = new TcpListener(Endpoint);

            }
        }
        HttpServer httpServer;
        ShellServer shellServer;
        //This will start a new thread.
        public void StartListenWeb()
        {
            httpServer = new HttpServer(Web);
            httpServer.StartListen();
        }
        //This Method will not stop current thread.
        public void Stop()
        {
            try
            {
                httpServer.Stop();
            }
            catch (Exception)
            {
            }
            try
            {
                shellServer.Stop();
            }
            catch (Exception)
            {
            }
        }
        public void StartListenShell()
        {
            shellServer = new ShellServer(Shell);
            shellServer.StartListen();
        }
    }
}
