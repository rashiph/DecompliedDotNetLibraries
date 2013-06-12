namespace System.Xml.Schema
{
    using System;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class XmlSchemaObject
    {
        private bool isProcessing;
        private int lineNum;
        private int linePos;
        private XmlSerializerNamespaces namespaces;
        private XmlSchemaObject parent;
        private string sourceUri;

        protected XmlSchemaObject()
        {
        }

        internal virtual void AddAnnotation(XmlSchemaAnnotation annotation)
        {
        }

        internal virtual XmlSchemaObject Clone()
        {
            return (XmlSchemaObject) base.MemberwiseClone();
        }

        internal virtual void OnAdd(XmlSchemaObjectCollection container, object item)
        {
        }

        internal virtual void OnClear(XmlSchemaObjectCollection container)
        {
        }

        internal virtual void OnRemove(XmlSchemaObjectCollection container, object item)
        {
        }

        internal virtual void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
        {
        }

        [XmlIgnore]
        internal virtual string IdAttribute
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        [XmlIgnore]
        internal bool IsProcessing
        {
            get
            {
                return this.isProcessing;
            }
            set
            {
                this.isProcessing = value;
            }
        }

        [XmlIgnore]
        public int LineNumber
        {
            get
            {
                return this.lineNum;
            }
            set
            {
                this.lineNum = value;
            }
        }

        [XmlIgnore]
        public int LinePosition
        {
            get
            {
                return this.linePos;
            }
            set
            {
                this.linePos = value;
            }
        }

        [XmlIgnore]
        internal virtual string NameAttribute
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces
        {
            get
            {
                if (this.namespaces == null)
                {
                    this.namespaces = new XmlSerializerNamespaces();
                }
                return this.namespaces;
            }
            set
            {
                this.namespaces = value;
            }
        }

        [XmlIgnore]
        public XmlSchemaObject Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        [XmlIgnore]
        public string SourceUri
        {
            get
            {
                return this.sourceUri;
            }
            set
            {
                this.sourceUri = value;
            }
        }
    }
}

