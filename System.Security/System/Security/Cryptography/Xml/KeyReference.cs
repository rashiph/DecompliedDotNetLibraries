namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class KeyReference : EncryptedReference
    {
        public KeyReference()
        {
            base.ReferenceType = "KeyReference";
        }

        public KeyReference(string uri) : base(uri)
        {
            base.ReferenceType = "KeyReference";
        }

        public KeyReference(string uri, TransformChain transformChain) : base(uri, transformChain)
        {
            base.ReferenceType = "KeyReference";
        }
    }
}

