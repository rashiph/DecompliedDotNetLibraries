namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name="ComPlusMethodCallTxMismatch")]
    internal class ComPlusMethodCallTxMismatchSchema : ComPlusMethodCallSchema
    {
        [DataMember(Name="CurrentTransactionID")]
        private Guid currentTransactionID;
        [DataMember(Name="IncomingTransactionID")]
        private Guid incomingTransactionID;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallTxMismatchTraceRecord";

        public ComPlusMethodCallTxMismatchSchema(Uri from, Guid appid, Guid clsid, Guid iid, string action, int instanceID, int managedThreadID, int unmanagedThreadID, string requestingIdentity, Guid incomingTransactionID, Guid currentTransactionID) : base(from, appid, clsid, iid, action, instanceID, managedThreadID, unmanagedThreadID, requestingIdentity)
        {
            this.incomingTransactionID = incomingTransactionID;
            this.currentTransactionID = currentTransactionID;
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallTxMismatchTraceRecord";
            }
        }
    }
}

