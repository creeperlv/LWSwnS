using CLUNL.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace LWSwnS.Core.Pipeline
{
    public class HttpResponsePipelineProcessor : IPipelineProcessor
    {
        //List<IPipedProcessUnit> pipedProcessUnits = new List<IPipedProcessUnit>();
        Dictionary<string, IPipedProcessUnit> Units = new Dictionary<string, IPipedProcessUnit>();
        public void Init()
        {
            throw new NotImplementedException();
        }

        public void Init(ProcessUnitManifest manifest)
        {
            //pipedProcessUnits=manifest.GetUnitInstances();
        }

        public PipelineData Process(PipelineData Input, bool IgnoreError)
        {
            //HttpPipelineData
            foreach (var item in Units)
            {
                var outdata=item.Value.Process(Input);
                if (!outdata.CheckContinuity(Input))
                {
                    throw new PipelineDataContinuityException(item.Value);
                }
            }
            return null;
        }

        public PipelineData Process(PipelineData Input)
        {
            return Process(Input, false);
        }
    }
}
