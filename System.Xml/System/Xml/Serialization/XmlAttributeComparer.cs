namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class XmlAttributeComparer : IComparer
    {
        public int Compare(object o1, object o2)
        {
            XmlAttribute attribute = (XmlAttribute) o1;
            XmlAttribute attribute2 = (XmlAttribute) o2;
            int num = string.Compare(attribute.NamespaceURI, attribute2.NamespaceURI, StringComparison.Ordinal);
            if (num == 0)
            {
                return string.Compare(attribute.Name, attribute2.Name, StringComparison.Ordinal);
            }
            return num;
        }
    }
}

