namespace System.Data.Design
{
    using System;
    using System.Xml;

    internal interface IDataSourceXmlSerializable
    {
        void ReadXml(XmlElement xmlElement, DataSourceXmlSerializer serializer);
        void WriteXml(XmlWriter writer, DataSourceXmlSerializer serializer);
    }
}

