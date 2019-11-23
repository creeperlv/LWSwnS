using LWSwnS.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LWSwnS.Core
{
    public class ShellServer
    {
        public static string ShellPassword;
        TcpListener TCPListener;
        bool ShellStop=false;
        public List<ShellClientProcessor> shellClients = new List<ShellClientProcessor>();
        public Dictionary<string, Action<string, object,StreamWriter>> Commands = new Dictionary<string, Action<string, object, StreamWriter>>();
        public ShellServer(TcpListener listener)
        {
            TCPListener = listener;
            ApiManager.AddFunction("REGCMD", (UniParamater p) => {
                var name = p[0] as String;
                var action = p[1] as Action<string, object, StreamWriter>;
                if (Commands.ContainsKey(name))
                {
                    Commands[name] = action;
                }
                else
                {
                    Commands.Add(name, action);
                }
                return new UniResult();
            });
        }
        public void StartListen()
        {

            Task.Run(() =>
            {
                TCPListener.Start();
                while (ShellStop == false)
                {
                    var a = TCPListener.AcceptTcpClient();
                    var p = new ShellClientProcessor(a, this);
                    shellClients.Add(p);
                    //var c = new TcpClientProcessor(a);
                    //tcpClients.Add(c);
                }
            });
        }
    }
    public class ShellClientProcessor
    {
        TcpClient client;
        ShellServer FatherServer;
        NetworkStream networkStream;
        StreamReader streamReader;
        StreamWriter streamWriter;
        public ShellClientProcessor(TcpClient tcpClient,ShellServer server)
        {
            client = tcpClient;
            FatherServer = server;
            networkStream = client.GetStream();
            //networkStream.CanTimeout = true;
            networkStream.ReadTimeout = 3000;
            networkStream.WriteTimeout = 3000;
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);
            Task.Run(MainProcess);
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

                FatherServer.shellClients.Remove(this);
                GC.Collect();
            }
            catch (Exception)
            {

            }
        }
        void MainProcess()
        {

            while (willStop == false)
            {
                var str = streamReader.ReadLine();
                var content=NETCore.Encrypt.EncryptProvider.AESDecrypt(str, ShellServer.ShellPassword);
                StringReader stringReader = new StringReader(content);
                var cmd=stringReader.ReadLine();
                var name = cmd.Substring(0,cmd.IndexOf(' '));
                var parameter = cmd.Substring(cmd.IndexOf(' ')+1);
                var doc=stringReader.ReadToEnd();
                object obj = null;
                if (doc != "NULL")
                {
                    var data = Convert.FromBase64String(doc);
                    MemoryStream memoryStream = new MemoryStream(data);
                    BinaryFormatter binary = new BinaryFormatter();
                    obj = binary.Deserialize(memoryStream);
                }
                foreach (var item in FatherServer.Commands)
                {
                    if (item.Key == name)
                    {
                        item.Value(parameter, obj,streamWriter);
                    }
                }
                //if ()
                //{

                //}
            }
        }
    }
}
