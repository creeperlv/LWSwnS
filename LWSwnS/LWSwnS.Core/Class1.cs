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
        bool WebStop = false;
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
        //This Method will not stop current thread.
        //This will start a new thread.
        public void StartListenWeb()
        {
            httpServer = new HttpServer(Web);
            httpServer.StartListen();
        }
        public void StartListenShell()
        {
            shellServer = new ShellServer(Shell);
            shellServer.StartListen();
        }
    }
}
