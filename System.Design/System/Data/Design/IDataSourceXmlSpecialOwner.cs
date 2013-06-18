namespace System.Data.Design
{
    using System;
    using System.Xml;

    internal interface IDataSourceXmlSpecialOwner
    {
        void ReadSpecialItem(string propertyName, XmlNode xmlNode, DataSourceXmlSerializer serializer);
        void WriteSpecialItem(string propertyName, XmlWriter writer, DataSourceXmlSerializer serializer);
    }
}

