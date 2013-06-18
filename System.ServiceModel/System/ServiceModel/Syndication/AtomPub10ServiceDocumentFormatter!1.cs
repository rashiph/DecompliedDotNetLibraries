namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), XmlRoot(ElementName="service", Namespace="http://www.w3.org/2007/app")]
    public class AtomPub10ServiceDocumentFormatter<TServiceDocument> : AtomPub10ServiceDocumentFormatter where TServiceDocument: ServiceDocument, new()
    {
        public AtomPub10ServiceDocumentFormatter() : base(typeof(TServiceDocument))
        {
        }

        public AtomPub10ServiceDocumentFormatter(TServiceDocument documentToWrite) : base(documentToWrite)
        {
        }

        protected override ServiceDocument CreateDocumentInstance()
        {
            return Activator.CreateInstance<TServiceDocument>();
        }
    }
}

