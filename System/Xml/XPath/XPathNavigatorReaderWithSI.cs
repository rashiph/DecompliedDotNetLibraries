namespace System.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    internal class XPathNavigatorReaderWithSI : XPathNavigatorReader, IXmlSchemaInfo
    {
        internal XPathNavigatorReaderWithSI(XPathNavigator navToRead, IXmlLineInfo xli, IXmlSchemaInfo xsi) : base(navToRead, xli, xsi)
        {
        }

        public override bool IsDefault
        {
            get
            {
                if (!base.IsReading)
                {
                    return false;
                }
                return base.schemaInfo.IsDefault;
            }
        }

        public virtual bool IsNil
        {
            get
            {
                if (!base.IsReading)
                {
                    return false;
                }
                return base.schemaInfo.IsNil;
            }
        }

        public virtual XmlSchemaSimpleType MemberType
        {
            get
            {
                if (!base.IsReading)
                {
                    return null;
                }
                return base.schemaInfo.MemberType;
            }
        }

        public virtual XmlSchemaAttribute SchemaAttribute
        {
            get
            {
                if (!base.IsReading)
                {
                    return null;
                }
                return base.schemaInfo.SchemaAttribute;
            }
        }

        public virtual XmlSchemaElement SchemaElement
        {
            get
            {
                if (!base.IsReading)
                {
                    return null;
                }
                return base.schemaInfo.SchemaElement;
            }
        }

        public virtual XmlSchemaType SchemaType
        {
            get
            {
                if (!base.IsReading)
                {
                    return null;
                }
                return base.schemaInfo.SchemaType;
            }
        }

        public virtual XmlSchemaValidity Validity
        {
            get
            {
                if (!base.IsReading)
                {
                    return XmlSchemaValidity.NotKnown;
                }
                return base.schemaInfo.Validity;
            }
        }
    }
}

