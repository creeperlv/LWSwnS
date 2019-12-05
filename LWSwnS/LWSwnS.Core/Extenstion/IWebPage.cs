using LWSwnS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LWSwnS.Core.Extenstion
{
    public interface IWebPage
    {
        string Access(string parameter,HttpRequestData requestData);
    }
}
