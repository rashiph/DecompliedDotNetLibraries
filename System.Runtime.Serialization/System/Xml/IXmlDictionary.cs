namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;

    public interface IXmlDictionary
    {
        bool TryLookup(int key, out XmlDictionaryString result);
        bool TryLookup(string value, out XmlDictionaryString result);
        bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result);
    }
}

