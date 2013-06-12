namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public class XmlSchemaRedefine : XmlSchemaExternal
    {
        private XmlSchemaObjectTable attributeGroups = new XmlSchemaObjectTable();
        private XmlSchemaObjectTable groups = new XmlSchemaObjectTable();
        private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();
        private XmlSchemaObjectTable types = new XmlSchemaObjectTable();

        public XmlSchemaRedefine()
        {
            base.Compositor = System.Xml.Schema.Compositor.Redefine;
        }

        internal override void AddAnnotation(XmlSchemaAnnotation annotation)
        {
            this.items.Add(annotation);
        }

        [XmlIgnore]
        public XmlSchemaObjectTable AttributeGroups
        {
            get
            {
                return this.attributeGroups;
            }
        }

        [XmlIgnore]
        public XmlSchemaObjectTable Groups
        {
            get
            {
                return this.groups;
            }
        }

        [XmlElement("simpleType", typeof(XmlSchemaSimpleType)), XmlElement("annotation", typeof(XmlSchemaAnnotation)), XmlElement("group", typeof(XmlSchemaGroup)), XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroup)), XmlElement("complexType", typeof(XmlSchemaComplexType))]
        public XmlSchemaObjectCollection Items
        {
            get
            {
                return this.items;
            }
        }

        [XmlIgnore]
        public XmlSchemaObjectTable SchemaTypes
        {
            get
            {
                return this.types;
            }
        }
    }
}

