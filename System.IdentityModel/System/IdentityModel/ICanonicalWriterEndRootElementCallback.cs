namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal interface ICanonicalWriterEndRootElementCallback
    {
        void OnEndOfRootElement(XmlDictionaryWriter writer);
    }
}

