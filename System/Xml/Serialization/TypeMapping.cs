namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    internal abstract class TypeMapping : Mapping
    {
        private bool includeInSchema = true;
        private bool reference;
        private bool referencedByElement;
        private bool referencedByTopLevelElement;
        private System.Xml.Serialization.TypeDesc typeDesc;
        private string typeName;
        private string typeNs;

        protected TypeMapping()
        {
        }

        internal virtual string DefaultElementName
        {
            get
            {
                if (!this.IsAnonymousType)
                {
                    return this.typeName;
                }
                return XmlConvert.EncodeLocalName(this.typeDesc.Name);
            }
        }

        internal bool IncludeInSchema
        {
            get
            {
                return this.includeInSchema;
            }
            set
            {
                this.includeInSchema = value;
            }
        }

        internal bool IsAnonymousType
        {
            get
            {
                if (this.typeName != null)
                {
                    return (this.typeName.Length == 0);
                }
                return true;
            }
        }

        internal virtual bool IsList
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        internal bool IsReference
        {
            get
            {
                return this.reference;
            }
            set
            {
                this.reference = value;
            }
        }

        internal string Namespace
        {
            get
            {
                return this.typeNs;
            }
            set
            {
                this.typeNs = value;
            }
        }

        internal bool ReferencedByElement
        {
            get
            {
                if (!this.referencedByElement)
                {
                    return this.referencedByTopLevelElement;
                }
                return true;
            }
            set
            {
                this.referencedByElement = value;
            }
        }

        internal bool ReferencedByTopLevelElement
        {
            get
            {
                return this.referencedByTopLevelElement;
            }
            set
            {
                this.referencedByTopLevelElement = value;
            }
        }

        internal System.Xml.Serialization.TypeDesc TypeDesc
        {
            get
            {
                return this.typeDesc;
            }
            set
            {
                this.typeDesc = value;
            }
        }

        internal string TypeName
        {
            get
            {
                return this.typeName;
            }
            set
            {
                this.typeName = value;
            }
        }
    }
}

