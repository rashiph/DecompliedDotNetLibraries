namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal interface ISecurityElement
    {
        void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager);

        bool HasId { get; }

        string Id { get; }
    }
}

