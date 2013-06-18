namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusActivity")]
    internal class ComPlusActivitySchema : TraceRecord
    {
        [DataMember(Name="ActivityID")]
        private Guid activityID;
        [DataMember(Name="LogicalThreadID")]
        private Guid logicalThreadID;
        [DataMember(Name="ManagedThreadID")]
        private int managedThreadID;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusActivityTraceRecord";
        [DataMember(Name="UnmanagedThreadID")]
        private int unmanagedThreadID;

        public ComPlusActivitySchema(Guid activityID, Guid logicalThreadID, int managedThreadID, int unmanagedThreadID)
        {
            this.activityID = activityID;
            this.logicalThreadID = logicalThreadID;
            this.managedThreadID = managedThreadID;
            this.unmanagedThreadID = unmanagedThreadID;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusActivityTraceRecord";
            }
        }
    }
}

