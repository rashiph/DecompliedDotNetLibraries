namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), DataContract]
    public abstract class CategoriesDocumentFormatter
    {
        private CategoriesDocument document;

        protected CategoriesDocumentFormatter()
        {
        }

        protected CategoriesDocumentFormatter(CategoriesDocument documentToWrite)
        {
            if (documentToWrite == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("documentToWrite");
            }
            this.document = documentToWrite;
        }

        public abstract bool CanRead(XmlReader reader);
        protected virtual InlineCategoriesDocument CreateInlineCategoriesDocument()
        {
            return new InlineCategoriesDocument();
        }

        protected virtual ReferencedCategoriesDocument CreateReferencedCategoriesDocument()
        {
            return new ReferencedCategoriesDocument();
        }

        public abstract void ReadFrom(XmlReader reader);
        protected virtual void SetDocument(CategoriesDocument document)
        {
            this.document = document;
        }

        public abstract void WriteTo(XmlWriter writer);

        public CategoriesDocument Document
        {
            get
            {
                return this.document;
            }
        }

        public abstract string Version { get; }
    }
}

