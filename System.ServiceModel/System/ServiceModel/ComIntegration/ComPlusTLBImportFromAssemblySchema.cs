namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusTLBImportFromAssembly")]
    internal class ComPlusTLBImportFromAssemblySchema : ComPlusTLBImportSchema
    {
        [DataMember(Name="Assembly")]
        private string assembly;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTLBImportFromAssemblyTraceRecord";

        public ComPlusTLBImportFromAssemblySchema(Guid iid, Guid typeLibraryID, string assembly) : base(iid, typeLibraryID)
        {
            this.assembly = assembly;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTLBImportFromAssemblyTraceRecord";
            }
        }
    }
}

