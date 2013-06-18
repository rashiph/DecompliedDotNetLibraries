namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class NamespaceFrame
    {
        private Hashtable m_rendered = new Hashtable();
        private Hashtable m_unrendered = new Hashtable();

        internal NamespaceFrame()
        {
        }

        internal void AddRendered(XmlAttribute attr)
        {
            this.m_rendered.Add(Utils.GetNamespacePrefix(attr), attr);
        }

        internal void AddUnrendered(XmlAttribute attr)
        {
            this.m_unrendered.Add(Utils.GetNamespacePrefix(attr), attr);
        }

        internal XmlAttribute GetRendered(string nsPrefix)
        {
            return (XmlAttribute) this.m_rendered[nsPrefix];
        }

        internal Hashtable GetUnrendered()
        {
            return this.m_unrendered;
        }

        internal XmlAttribute GetUnrendered(string nsPrefix)
        {
            return (XmlAttribute) this.m_unrendered[nsPrefix];
        }
    }
}

