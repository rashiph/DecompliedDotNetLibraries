namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusTLBImportConverterEvent")]
    internal class ComPlusTLBImportConverterEventSchema : ComPlusTLBImportSchema
    {
        [DataMember(Name="EventCode")]
        private int eventCode;
        [DataMember(Name="EventKind")]
        private ImporterEventKind eventKind;
        [DataMember(Name="EventMessage")]
        private string eventMessage;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTLBImportConverterEventTraceRecord";

        public ComPlusTLBImportConverterEventSchema(Guid iid, Guid typeLibraryID, ImporterEventKind eventKind, int eventCode, string eventMessage) : base(iid, typeLibraryID)
        {
            this.eventKind = eventKind;
            this.eventCode = eventCode;
            this.eventMessage = eventMessage;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTLBImportConverterEventTraceRecord";
            }
        }
    }
}

