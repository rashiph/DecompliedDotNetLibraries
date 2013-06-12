namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class SchemaNamespaceManager : XmlNamespaceManager
    {
        private XmlSchemaObject node;

        public SchemaNamespaceManager(XmlSchemaObject node)
        {
            this.node = node;
        }

        public override string LookupNamespace(string prefix)
        {
            if (prefix == "xml")
            {
                return "http://www.w3.org/XML/1998/namespace";
            }
            for (XmlSchemaObject obj2 = this.node; obj2 != null; obj2 = obj2.Parent)
            {
                Hashtable namespaces = obj2.Namespaces.Namespaces;
                if ((namespaces != null) && (namespaces.Count > 0))
                {
                    object obj3 = namespaces[prefix];
                    if (obj3 != null)
                    {
                        return (string) obj3;
                    }
                }
            }
            if (prefix.Length != 0)
            {
                return null;
            }
            return string.Empty;
        }

        public override string LookupPrefix(string ns)
        {
            if (ns == "http://www.w3.org/XML/1998/namespace")
            {
                return "xml";
            }
            for (XmlSchemaObject obj2 = this.node; obj2 != null; obj2 = obj2.Parent)
            {
                Hashtable namespaces = obj2.Namespaces.Namespaces;
                if ((namespaces != null) && (namespaces.Count > 0))
                {
                    foreach (DictionaryEntry entry in namespaces)
                    {
                        if (entry.Value.Equals(ns))
                        {
                            return (string) entry.Key;
                        }
                    }
                }
            }
            return null;
        }
    }
}

