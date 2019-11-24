using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LWSwnS.Api.Data
{
    public class ShellDataExchange
    {
        public static string AES_PW;
        public static byte[] ObjectToBytes(object obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, obj);
            return memoryStream.GetBuffer();
        }
        public static object BytesToObject(byte[] data)
        {
            MemoryStream memoryStream = new MemoryStream(data);
            BinaryFormatter binary = new BinaryFormatter();
            return binary.Deserialize(memoryStream);
        }
        public static void SendCommand(string command, string parameter, object data, StreamWriter sw)
        {

            var RealBody = "NULL";
            if (data != null)
            {
                var bodyData = ShellDataExchange.ObjectToBytes(data);
                RealBody = Convert.ToBase64String(bodyData);
            }
            var content = command + " " + parameter + Environment.NewLine + RealBody;
            var ToSend = NETCore.Encrypt.EncryptProvider.AESEncrypt(content, AES_PW);
            sw.WriteLine(ToSend);
            sw.Flush();
        }
        public static ShellFeedbackData SendCommandAndWaitForResult(string command, string parameter, object data, StreamWriter sw, StreamReader sr)
        {

            SendCommand(command, parameter, data, sw);
            {

                if (sr == null)
                {
                    return null;
                }
                else
                {
                    var str = sr.ReadLine();
                    var content = NETCore.Encrypt.EncryptProvider.AESDecrypt(str, AES_PW);
                    StringReader stringReader = new StringReader(content);
                    var StatusLine = stringReader.ReadLine();
                    var doc = stringReader.ReadToEnd();
                    object obj = null;
                    if (doc != "NULL")
                    {
                        var rDataB = Convert.FromBase64String(doc);

                        MemoryStream memoryStream = new MemoryStream(rDataB);
                        BinaryFormatter binary = new BinaryFormatter();
                        obj = binary.Deserialize(memoryStream);
                    }
                    ShellFeedbackData shellFeedbackData = new ShellFeedbackData();
                    shellFeedbackData.StatusLine = StatusLine;
                    shellFeedbackData.DataBody = obj;
                    return shellFeedbackData;
                }
            }
        }
    }
}
