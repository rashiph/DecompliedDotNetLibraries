namespace System.Xml
{
    using System;
    using System.Xml.Schema;

    internal class XmlName : IXmlSchemaInfo
    {
        private int hashCode;
        private string localName;
        private string name;
        internal XmlName next;
        private string ns;
        internal XmlDocument ownerDoc;
        private string prefix;

        internal XmlName(string prefix, string localName, string ns, int hashCode, XmlDocument ownerDoc, XmlName next)
        {
            this.prefix = prefix;
            this.localName = localName;
            this.ns = ns;
            this.name = null;
            this.hashCode = hashCode;
            this.ownerDoc = ownerDoc;
            this.next = next;
        }

        public static XmlName Create(string prefix, string localName, string ns, int hashCode, XmlDocument ownerDoc, XmlName next, IXmlSchemaInfo schemaInfo)
        {
            if (schemaInfo == null)
            {
                return new XmlName(prefix, localName, ns, hashCode, ownerDoc, next);
            }
            return new XmlNameEx(prefix, localName, ns, hashCode, ownerDoc, next, schemaInfo);
        }

        public virtual bool Equals(IXmlSchemaInfo schemaInfo)
        {
            return (schemaInfo == null);
        }

        public static int GetHashCode(string name)
        {
            int num = 0;
            if (name == null)
            {
                return num;
            }
            for (int i = name.Length - 1; i >= 0; i--)
            {
                char ch = name[i];
                if (ch == ':')
                {
                    break;
                }
                num += (num << 7) ^ ch;
            }
            num -= num >> 0x11;
            num -= num >> 11;
            return (num - (num >> 5));
        }

        public int HashCode
        {
            get
            {
                return this.hashCode;
            }
        }

        public virtual bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsNil
        {
            get
            {
                return false;
            }
        }

        public string LocalName
        {
            get
            {
                return this.localName;
            }
        }

        public virtual XmlSchemaSimpleType MemberType
        {
            get
            {
                return null;
            }
        }

        public string Name
        {
            get
            {
                if (this.name == null)
                {
                    if (this.prefix.Length > 0)
                    {
                        if (this.localName.Length > 0)
                        {
                            string array = this.prefix + ":" + this.localName;
                            lock (this.ownerDoc.NameTable)
                            {
                                if (this.name == null)
                                {
                                    this.name = this.ownerDoc.NameTable.Add(array);
                                }
                                goto Label_0099;
                            }
                        }
                        this.name = this.prefix;
                    }
                    else
                    {
                        this.name = this.localName;
                    }
                }
            Label_0099:
                return this.name;
            }
        }

        public string NamespaceURI
        {
            get
            {
                return this.ns;
            }
        }

        public XmlDocument OwnerDocument
        {
            get
            {
                return this.ownerDoc;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public virtual XmlSchemaAttribute SchemaAttribute
        {
            get
            {
                return null;
            }
        }

        public virtual XmlSchemaElement SchemaElement
        {
            get
            {
                return null;
            }
        }

        public virtual XmlSchemaType SchemaType
        {
            get
            {
                return null;
            }
        }

        public virtual XmlSchemaValidity Validity
        {
            get
            {
                return XmlSchemaValidity.NotKnown;
            }
        }
    }
}

