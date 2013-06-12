namespace System.Xml
{
    using System;
    using System.Xml.Schema;

    internal class DomNameTable
    {
        private int count;
        private XmlName[] entries;
        private const int InitialSize = 0x40;
        private int mask;
        private XmlNameTable nameTable;
        private XmlDocument ownerDocument;

        public DomNameTable(XmlDocument document)
        {
            this.ownerDocument = document;
            this.nameTable = document.NameTable;
            this.entries = new XmlName[0x40];
            this.mask = 0x3f;
        }

        public XmlName AddName(string prefix, string localName, string ns, IXmlSchemaInfo schemaInfo)
        {
            if (prefix == null)
            {
                prefix = string.Empty;
            }
            if (ns == null)
            {
                ns = string.Empty;
            }
            int hashCode = XmlName.GetHashCode(localName);
            for (XmlName name = this.entries[hashCode & this.mask]; name != null; name = name.next)
            {
                if ((((name.HashCode == hashCode) && ((name.LocalName == localName) || name.LocalName.Equals(localName))) && ((name.Prefix == prefix) || name.Prefix.Equals(prefix))) && (((name.NamespaceURI == ns) || name.NamespaceURI.Equals(ns)) && name.Equals(schemaInfo)))
                {
                    return name;
                }
            }
            prefix = this.nameTable.Add(prefix);
            localName = this.nameTable.Add(localName);
            ns = this.nameTable.Add(ns);
            int index = hashCode & this.mask;
            XmlName name2 = XmlName.Create(prefix, localName, ns, hashCode, this.ownerDocument, this.entries[index], schemaInfo);
            this.entries[index] = name2;
            if (this.count++ == this.mask)
            {
                this.Grow();
            }
            return name2;
        }

        public XmlName GetName(string prefix, string localName, string ns, IXmlSchemaInfo schemaInfo)
        {
            if (prefix == null)
            {
                prefix = string.Empty;
            }
            if (ns == null)
            {
                ns = string.Empty;
            }
            int hashCode = XmlName.GetHashCode(localName);
            for (XmlName name = this.entries[hashCode & this.mask]; name != null; name = name.next)
            {
                if ((((name.HashCode == hashCode) && ((name.LocalName == localName) || name.LocalName.Equals(localName))) && ((name.Prefix == prefix) || name.Prefix.Equals(prefix))) && (((name.NamespaceURI == ns) || name.NamespaceURI.Equals(ns)) && name.Equals(schemaInfo)))
                {
                    return name;
                }
            }
            return null;
        }

        private void Grow()
        {
            int num = (this.mask * 2) + 1;
            XmlName[] entries = this.entries;
            XmlName[] nameArray2 = new XmlName[num + 1];
            for (int i = 0; i < entries.Length; i++)
            {
                XmlName next;
                for (XmlName name = entries[i]; name != null; name = next)
                {
                    int index = name.HashCode & num;
                    next = name.next;
                    name.next = nameArray2[index];
                    nameArray2[index] = name;
                }
            }
            this.entries = nameArray2;
            this.mask = num;
        }
    }
}

