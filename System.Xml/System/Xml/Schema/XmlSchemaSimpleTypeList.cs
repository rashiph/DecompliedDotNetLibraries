namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaSimpleTypeList : XmlSchemaSimpleTypeContent
    {
        private XmlSchemaSimpleType baseItemType;
        private XmlSchemaSimpleType itemType;
        private XmlQualifiedName itemTypeName = XmlQualifiedName.Empty;

        internal override XmlSchemaObject Clone()
        {
            XmlSchemaSimpleTypeList list = (XmlSchemaSimpleTypeList) base.MemberwiseClone();
            list.ItemTypeName = this.itemTypeName.Clone();
            return list;
        }

        [XmlIgnore]
        public XmlSchemaSimpleType BaseItemType
        {
            get
            {
                return this.baseItemType;
            }
            set
            {
                this.baseItemType = value;
            }
        }

        [XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
        public XmlSchemaSimpleType ItemType
        {
            get
            {
                return this.itemType;
            }
            set
            {
                this.itemType = value;
            }
        }

        [XmlAttribute("itemType")]
        public XmlQualifiedName ItemTypeName
        {
            get
            {
                return this.itemTypeName;
            }
            set
            {
                this.itemTypeName = (value == null) ? XmlQualifiedName.Empty : value;
            }
        }
    }
}

