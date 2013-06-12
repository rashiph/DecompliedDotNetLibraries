namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class QNameComparer : IComparer
    {
        public int Compare(object o1, object o2)
        {
            XmlQualifiedName name = (XmlQualifiedName) o1;
            XmlQualifiedName name2 = (XmlQualifiedName) o2;
            int num = string.Compare(name.Namespace, name2.Namespace, StringComparison.Ordinal);
            if (num == 0)
            {
                return string.Compare(name.Name, name2.Name, StringComparison.Ordinal);
            }
            return num;
        }
    }
}

