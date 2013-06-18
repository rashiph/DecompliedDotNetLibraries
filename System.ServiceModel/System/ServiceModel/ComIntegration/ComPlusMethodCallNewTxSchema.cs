namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name="ComPlusMethodCallNewTx")]
    internal class ComPlusMethodCallNewTxSchema : ComPlusMethodCallSchema
    {
        [DataMember(Name="NewTransactionID")]
        private Guid newTransactionID;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallNewTxTraceRecord";

        public ComPlusMethodCallNewTxSchema(Uri from, Guid appid, Guid clsid, Guid iid, string action, int instanceID, int managedThreadID, int unmanagedThreadID, string requestingIdentity, Guid newTransactionID) : base(from, appid, clsid, iid, action, instanceID, managedThreadID, unmanagedThreadID, requestingIdentity)
        {
            this.newTransactionID = newTransactionID;
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMethodCallNewTxTraceRecord";
            }
        }
    }
}

