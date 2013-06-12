namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class AutoValidator : BaseValidator
    {
        private const string x_schema = "x-schema";

        public AutoValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling) : base(reader, schemaCollection, eventHandling)
        {
            base.schemaInfo = new SchemaInfo();
        }

        public override void CompleteValidation()
        {
        }

        private ValidationType DetectValidationType()
        {
            if ((base.reader.Schemas != null) && (base.reader.Schemas.Count > 0))
            {
                XmlSchemaCollectionEnumerator enumerator = base.reader.Schemas.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    SchemaInfo schemaInfo = enumerator.CurrentNode.SchemaInfo;
                    if (schemaInfo.SchemaType == SchemaType.XSD)
                    {
                        return ValidationType.Schema;
                    }
                    if (schemaInfo.SchemaType == SchemaType.XDR)
                    {
                        return ValidationType.XDR;
                    }
                }
            }
            if (base.reader.NodeType == XmlNodeType.Element)
            {
                switch (base.SchemaNames.SchemaTypeFromRoot(base.reader.LocalName, base.reader.NamespaceURI))
                {
                    case SchemaType.XSD:
                        return ValidationType.Schema;

                    case SchemaType.XDR:
                        return ValidationType.XDR;
                }
                int attributeCount = base.reader.AttributeCount;
                for (int i = 0; i < attributeCount; i++)
                {
                    base.reader.MoveToAttribute(i);
                    string namespaceURI = base.reader.NamespaceURI;
                    string localName = base.reader.LocalName;
                    if (Ref.Equal(namespaceURI, base.SchemaNames.NsXmlNs))
                    {
                        if (XdrBuilder.IsXdrSchema(base.reader.Value))
                        {
                            base.reader.MoveToElement();
                            return ValidationType.XDR;
                        }
                    }
                    else
                    {
                        if (Ref.Equal(namespaceURI, base.SchemaNames.NsXsi))
                        {
                            base.reader.MoveToElement();
                            return ValidationType.Schema;
                        }
                        if (Ref.Equal(namespaceURI, base.SchemaNames.QnDtDt.Namespace) && Ref.Equal(localName, base.SchemaNames.QnDtDt.Name))
                        {
                            base.reader.SchemaTypeObject = XmlSchemaDatatype.FromXdrName(base.reader.Value);
                            base.reader.MoveToElement();
                            return ValidationType.XDR;
                        }
                    }
                }
                if (attributeCount > 0)
                {
                    base.reader.MoveToElement();
                }
            }
            return ValidationType.Auto;
        }

        public override object FindId(string name)
        {
            return null;
        }

        public override void Validate()
        {
            switch (this.DetectValidationType())
            {
                case ValidationType.Auto:
                case ValidationType.DTD:
                    break;

                case ValidationType.XDR:
                    base.reader.Validator = new XdrValidator(this);
                    base.reader.Validator.Validate();
                    return;

                case ValidationType.Schema:
                    base.reader.Validator = new XsdValidator(this);
                    base.reader.Validator.Validate();
                    break;

                default:
                    return;
            }
        }

        public override bool PreserveWhitespace
        {
            get
            {
                return false;
            }
        }
    }
}

