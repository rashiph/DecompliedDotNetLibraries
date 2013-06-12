namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaSimpleTypeUnion : XmlSchemaSimpleTypeContent
    {
        private XmlSchemaSimpleType[] baseMemberTypes;
        private XmlSchemaObjectCollection baseTypes = new XmlSchemaObjectCollection();
        private XmlQualifiedName[] memberTypes;

        internal override XmlSchemaObject Clone()
        {
            if ((this.memberTypes == null) || (this.memberTypes.Length <= 0))
            {
                return this;
            }
            XmlSchemaSimpleTypeUnion union = (XmlSchemaSimpleTypeUnion) base.MemberwiseClone();
            XmlQualifiedName[] nameArray = new XmlQualifiedName[this.memberTypes.Length];
            for (int i = 0; i < this.memberTypes.Length; i++)
            {
                nameArray[i] = this.memberTypes[i].Clone();
            }
            union.MemberTypes = nameArray;
            return union;
        }

        internal void SetBaseMemberTypes(XmlSchemaSimpleType[] baseMemberTypes)
        {
            this.baseMemberTypes = baseMemberTypes;
        }

        [XmlIgnore]
        public XmlSchemaSimpleType[] BaseMemberTypes
        {
            get
            {
                return this.baseMemberTypes;
            }
        }

        [XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
        public XmlSchemaObjectCollection BaseTypes
        {
            get
            {
                return this.baseTypes;
            }
        }

        [XmlAttribute("memberTypes")]
        public XmlQualifiedName[] MemberTypes
        {
            get
            {
                return this.memberTypes;
            }
            set
            {
                this.memberTypes = value;
            }
        }
    }
}

