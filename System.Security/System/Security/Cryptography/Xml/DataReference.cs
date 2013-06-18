namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class DataReference : EncryptedReference
    {
        public DataReference()
        {
            base.ReferenceType = "DataReference";
        }

        public DataReference(string uri) : base(uri)
        {
            base.ReferenceType = "DataReference";
        }

        public DataReference(string uri, TransformChain transformChain) : base(uri, transformChain)
        {
            base.ReferenceType = "DataReference";
        }
    }
}

