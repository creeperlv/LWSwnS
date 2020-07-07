using CLUNL.Pipeline;
using LWSwnS.Core.Data;

namespace LWSwnS.Core.Pipeline
{
    public class HttpPipelineData : PipelineData
    {
        public HttpPipelineData(object PrimaryData, object SecondaryData, HttpRequestData Options) : base(PrimaryData, SecondaryData, Options)
        {

        }
    }
}
