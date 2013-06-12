namespace System.Xml.Serialization
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    public interface IXmlSerializable
    {
        XmlSchema GetSchema();
        void ReadXml(XmlReader reader);
        void WriteXml(XmlWriter writer);
    }
}

