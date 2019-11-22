using System;
using System.Collections.Generic;

namespace LWSwnS.Api
{
    public class ApiManager
    {
        public static Dictionary<string, Func<UniParamater, UniResult>> Functions = new Dictionary<string, Func<UniParamater, UniResult>>();
        public static void AddFunction(string Name, Func<UniParamater, UniResult> func)
        {
            if (Functions.ContainsKey(Name))
            {
                Functions[Name] = func;
            }
            else
            {
                Functions.Add(Name, func);
            }
        }
    }
    public class UniParamater : List<object> { }
    public class UniResult
    {
        public Type DataType;
        public Object Data;
    }
}
