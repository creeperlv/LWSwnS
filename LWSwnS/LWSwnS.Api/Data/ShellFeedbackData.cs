using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LWSwnS.Api.Data
{
    public class ShellFeedbackData
    {
        public string StatusLine = "OK";
        public object DataBody=null;
        public StreamWriter writer;
        public void SendBack()
        {
            var RealBody = "NULL";
            //First Step: Combine Data.
            if (DataBody != null)
            {
                var bodyData = ShellDataExchange.ObjectToBytes(DataBody);
                RealBody= Convert.ToBase64String(bodyData);
            }
            var content = StatusLine + Environment.NewLine + RealBody;
            var ToSend = NETCore.Encrypt.EncryptProvider.AESEncrypt(content, ShellDataExchange.AES_PW);
            writer.WriteLine(ToSend);
            writer.Flush();
        }
    }
}
