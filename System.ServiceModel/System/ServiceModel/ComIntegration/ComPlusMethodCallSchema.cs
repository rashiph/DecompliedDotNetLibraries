namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;

    [DataContract(Name="ComPlusMethodCall")]
    internal class ComPlusMethodCallSchema : TraceRecord
    {
        [DataMember(Name="Action")]
        private string action;
        [DataMember(Name="appid")]
        private Guid appid;
        [DataMember(Name="clsid")]
        private Guid clsid;
        [DataMember(Name="From")]
        private Uri from;
        [DataMember(Name="iid")]
        private Guid iid;
        [DataMember(Name="InstanceID")]
        private int instanceID;
        [DataMember(Name="ManagedThreadID")]
        private int managedThreadID;
        [DataMember(Name="RequestingIdentity")]
        private string requestingIdentity;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallTraceRecord";
        [DataMember(Name="UnmanagedThreadID")]
        private int unmanagedThreadID;

        public ComPlusMethodCallSchema(Uri from, Guid appid, Guid clsid, Guid iid, string action, int instanceID, int managedThreadID, int unmanagedThreadID, string requestingIdentity)
        {
            this.from = from;
            this.appid = appid;
            this.clsid = clsid;
            this.iid = iid;
            this.action = action;
            this.instanceID = instanceID;
            this.managedThreadID = managedThreadID;
            this.unmanagedThreadID = unmanagedThreadID;
            this.requestingIdentity = requestingIdentity;
        }

        public override string ToString()
        {
            return System.ServiceModel.SR.GetString("ComPlusMethodCallSchema", new object[] { this.from.ToString(), this.appid.ToString(), this.clsid.ToString(), this.iid.ToString(), this.action, this.instanceID.ToString(CultureInfo.CurrentCulture), this.managedThreadID.ToString(CultureInfo.CurrentCulture), this.unmanagedThreadID.ToString(CultureInfo.CurrentCulture), this.requestingIdentity });
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallTraceRecord";
            }
        }
    }
}

