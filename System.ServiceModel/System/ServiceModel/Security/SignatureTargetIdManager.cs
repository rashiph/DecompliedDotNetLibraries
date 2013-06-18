namespace System.ServiceModel.Security
{
    using System;
    using System.Xml;

    internal abstract class SignatureTargetIdManager
    {
        protected SignatureTargetIdManager()
        {
        }

        public abstract string ExtractId(XmlDictionaryReader reader);
        public abstract void WriteIdAttribute(XmlDictionaryWriter writer, string id);

        public abstract string DefaultIdNamespacePrefix { get; }

        public abstract string DefaultIdNamespaceUri { get; }
    }
}

