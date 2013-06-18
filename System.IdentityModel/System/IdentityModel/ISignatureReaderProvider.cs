namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal interface ISignatureReaderProvider
    {
        XmlDictionaryReader GetReader(object callbackContext);
    }
}

