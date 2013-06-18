namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusDispatchMethodSchema")]
    internal class ComPlusDispatchMethodSchema : TraceRecord
    {
        [DataMember(Name="Name")]
        private string name;
        [DataMember(Name="ParameterInfo")]
        private List<DispatchProxy.ParamInfo> paramList;
        [DataMember(Name="ReturnValueInfo")]
        private DispatchProxy.ParamInfo returnValue;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusDispatchMethodTraceRecord";

        public ComPlusDispatchMethodSchema(string name, List<DispatchProxy.ParamInfo> paramList, DispatchProxy.ParamInfo returnValue)
        {
            this.name = name;
            this.paramList = paramList;
            this.returnValue = returnValue;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusDispatchMethodTraceRecord";
            }
        }
    }
}

