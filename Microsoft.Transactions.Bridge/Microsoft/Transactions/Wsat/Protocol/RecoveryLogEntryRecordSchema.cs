namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using System;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="RecoveryLogEntry")]
    internal class RecoveryLogEntryRecordSchema : TraceRecord
    {
        private byte[] recoveryData;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoveryLogEntryTraceRecord";
        [DataMember(Name="TransactionId", IsRequired=true)]
        private string transactionId;

        public RecoveryLogEntryRecordSchema(string transactionId, byte[] recoveryData)
        {
            this.transactionId = transactionId;
            this.recoveryData = recoveryData;
            if (this.recoveryData == null)
            {
                this.recoveryData = new byte[0];
            }
        }

        public override string ToString()
        {
            return Microsoft.Transactions.SR.GetString("RecoveryLogEntryRecordSchema", new object[] { this.transactionId, this.RecoveryDataLength.ToString(CultureInfo.CurrentCulture), this.RecoveryDataBase64 });
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/RecoveryLogEntryTraceRecord";
            }
        }

        [DataMember(Name="RecoveryDataBase64", IsRequired=true)]
        private string RecoveryDataBase64
        {
            get
            {
                return Convert.ToBase64String(this.recoveryData);
            }
            set
            {
            }
        }

        [DataMember(Name="RecoveryDataLength", IsRequired=true)]
        private long RecoveryDataLength
        {
            get
            {
                return (long) this.recoveryData.Length;
            }
            set
            {
            }
        }
    }
}

