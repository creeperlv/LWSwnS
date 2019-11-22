using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LWSwnS.Core
{
    public class ShellServer
    {
        TcpListener TCPListener;
        bool ShellStop=false;
        public ShellServer(TcpListener listener)
        {
            TCPListener = listener;
        }
        public void StartListen()
        {

            Task.Run(() =>
            {
                TCPListener.Start();
                while (ShellStop == false)
                {
                    var a = TCPListener.AcceptTcpClient();
                    //var c = new TcpClientProcessor(a);
                    //tcpClients.Add(c);
                }
            });
        }
    }
}
