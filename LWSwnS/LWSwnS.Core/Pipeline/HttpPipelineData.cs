using CLUNL.Pipeline;
using LWSwnS.Core.Data;

namespace LWSwnS.Core.Pipeline
{
    public class HttpPipelineData : PipelineData
    {
        public HttpPipelineData(object PrimaryData, object SecondaryData, HttpRequestPipelineData Options) : base(PrimaryData, SecondaryData, Options)
        {

        }
        public HttpPipelineData(object PrimaryData, object SecondaryData, HttpRequestData Options) : base(PrimaryData, SecondaryData, new HttpRequestPipelineData(Options))
        {

        }
    }
    public class HttpRequestPipelineData
    {
        public HttpRequestData requestData { get; }
        public bool Flag_Block_FollowedPipe=false;
        public HttpRequestPipelineData(HttpRequestData Data)
        {
            requestData = Data;
        }
    }
}
