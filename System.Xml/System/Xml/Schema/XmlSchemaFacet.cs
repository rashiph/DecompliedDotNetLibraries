namespace System.Xml.Schema
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public abstract class XmlSchemaFacet : XmlSchemaAnnotated
    {
        private System.Xml.Schema.FacetType facetType;
        private bool isFixed;
        private string value;

        protected XmlSchemaFacet()
        {
        }

        internal System.Xml.Schema.FacetType FacetType
        {
            get
            {
                return this.facetType;
            }
            set
            {
                this.facetType = value;
            }
        }

        [XmlAttribute("fixed"), DefaultValue(false)]
        public virtual bool IsFixed
        {
            get
            {
                return this.isFixed;
            }
            set
            {
                if (!(this is XmlSchemaEnumerationFacet) && !(this is XmlSchemaPatternFacet))
                {
                    this.isFixed = value;
                }
            }
        }

        [XmlAttribute("value")]
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

