namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name="ComPlusMethodCallContextTx")]
    internal class ComPlusMethodCallContextTxSchema : ComPlusMethodCallSchema
    {
        [DataMember(Name="ContextTransactionID")]
        private Guid contextTransactionID;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallContextTxTraceRecord";

        public ComPlusMethodCallContextTxSchema(Uri from, Guid appid, Guid clsid, Guid iid, string action, int instanceID, int managedThreadID, int unmanagedThreadID, string requestingIdentity, Guid contextTransactionID) : base(from, appid, clsid, iid, action, instanceID, managedThreadID, unmanagedThreadID, requestingIdentity)
        {
            this.contextTransactionID = contextTransactionID;
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallContextTxTraceRecord";
            }
        }
    }
}

