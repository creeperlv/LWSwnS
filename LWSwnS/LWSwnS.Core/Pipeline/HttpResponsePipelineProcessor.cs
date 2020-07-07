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
        public void Remove(string ID)
        {
            if(Units.ContainsKey(ID))
            Units.Remove(ID);
        }
        public void Add(string ID,IPipedProcessUnit pipedProcessUnit)
        {
            if (Units.ContainsKey(ID))
            {
                throw new PipelineUnitDuplicatedException();
            }
            else
            {
                Units.Add(ID, pipedProcessUnit);
            }
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

    [Serializable]
    public class PipelineUnitDuplicatedException : Exception
    {
        public PipelineUnitDuplicatedException() { }
        public PipelineUnitDuplicatedException(string message) : base(message) { }
        public PipelineUnitDuplicatedException(string message, Exception inner) : base(message, inner) { }
        protected PipelineUnitDuplicatedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
