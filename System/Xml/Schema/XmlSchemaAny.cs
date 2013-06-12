namespace System.Xml.Schema
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaAny : XmlSchemaParticle
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

        internal override string NameString
        {
            get
            {
                switch (this.namespaceList.Type)
                {
                    case System.Xml.Schema.NamespaceList.ListType.Any:
                        return "##any:*";

                    case System.Xml.Schema.NamespaceList.ListType.Other:
                        return "##other:*";

                    case System.Xml.Schema.NamespaceList.ListType.Set:
                    {
                        StringBuilder builder = new StringBuilder();
                        int num = 1;
                        foreach (string str in this.namespaceList.Enumerate)
                        {
                            builder.Append(str + ":*");
                            if (num < this.namespaceList.Enumerate.Count)
                            {
                                builder.Append(" ");
                            }
                            num++;
                        }
                        return builder.ToString();
                    }
                }
                return string.Empty;
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

        [XmlIgnore]
        internal string ResolvedNamespace
        {
            get
            {
                if ((this.ns != null) && (this.ns.Length != 0))
                {
                    return this.ns;
                }
                return "##any";
            }
        }
    }
}

