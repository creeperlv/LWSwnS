using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LWSwnS.Core
{
    public class ShellServer
    {
        TcpListener tcpListener;
        public ShellServer(TcpListener listener)
        {
            tcpListener = listener;
        }
        public void StartListen()
        {

        }
    }
}
