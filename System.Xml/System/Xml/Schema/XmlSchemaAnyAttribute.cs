namespace System.Xml.Schema
{
    using System;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaAnyAttribute : XmlSchemaAnnotated
    {
        private System.Xml.Schema.NamespaceList namespaceList;
        private string ns;
        private XmlSchemaContentProcessing processContents;

        internal bool Allows(XmlQualifiedName qname)
        {
            return this.namespaceList.Allows(qname.Namespace);
        }

        internal void BuildNamespaceList(string targetNamespace)
        {
            if (this.ns != null)
            {
                this.namespaceList = new System.Xml.Schema.NamespaceList(this.ns, targetNamespace);
            }
            else
            {
                this.namespaceList = new System.Xml.Schema.NamespaceList();
            }
        }

        internal void BuildNamespaceListV1Compat(string targetNamespace)
        {
            if (this.ns != null)
            {
                this.namespaceList = new NamespaceListV1Compat(this.ns, targetNamespace);
            }
            else
            {
                this.namespaceList = new System.Xml.Schema.NamespaceList();
            }
        }

        internal static XmlSchemaAnyAttribute Intersection(XmlSchemaAnyAttribute o1, XmlSchemaAnyAttribute o2, bool v1Compat)
        {
            System.Xml.Schema.NamespaceList list = System.Xml.Schema.NamespaceList.Intersection(o1.NamespaceList, o2.NamespaceList, v1Compat);
            if (list != null)
            {
                return new XmlSchemaAnyAttribute { namespaceList = list, ProcessContents = o1.ProcessContents, Annotation = o1.Annotation };
            }
            return null;
        }

        internal static bool IsSubset(XmlSchemaAnyAttribute sub, XmlSchemaAnyAttribute super)
        {
            return System.Xml.Schema.NamespaceList.IsSubset(sub.NamespaceList, super.NamespaceList);
        }

        internal static XmlSchemaAnyAttribute Union(XmlSchemaAnyAttribute o1, XmlSchemaAnyAttribute o2, bool v1Compat)
        {
            System.Xml.Schema.NamespaceList list = System.Xml.Schema.NamespaceList.Union(o1.NamespaceList, o2.NamespaceList, v1Compat);
            if (list != null)
            {
                return new XmlSchemaAnyAttribute { namespaceList = list, processContents = o1.processContents, Annotation = o1.Annotation };
            }
            return null;
        }

        [XmlAttribute("namespace")]
        public string Namespace
        {
            get
            {
                return this.ns;
            }
            set
            {
                this.ns = value;
            }
        }

        [XmlIgnore]
        internal System.Xml.Schema.NamespaceList NamespaceList
        {
            get
            {
                return this.namespaceList;
            }
        }

        [XmlAttribute("processContents"), DefaultValue(0)]
        public XmlSchemaContentProcessing ProcessContents
        {
            get
            {
                return this.processContents;
            }
            set
            {
                this.processContents = value;
            }
        }

        [XmlIgnore]
        internal XmlSchemaContentProcessing ProcessContentsCorrect
        {
            get
            {
                if (this.processContents != XmlSchemaContentProcessing.None)
                {
                    return this.processContents;
                }
                return XmlSchemaContentProcessing.Strict;
            }
        }
    }
}

