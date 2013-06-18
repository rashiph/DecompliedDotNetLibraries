namespace System.Xml.Schema
{
    using System;
    using System.Collections.Generic;

    internal class XmlSchemaInfoEqualityComparer : IEqualityComparer<XmlSchemaInfo>
    {
        public bool Equals(XmlSchemaInfo si1, XmlSchemaInfo si2)
        {
            if (si1 == si2)
            {
                return true;
            }
            if ((si1 == null) || (si2 == null))
            {
                return false;
            }
            return (((((si1.ContentType == si2.ContentType) && (si1.IsDefault == si2.IsDefault)) && ((si1.IsNil == si2.IsNil) && (si1.MemberType == si2.MemberType))) && (((si1.SchemaAttribute == si2.SchemaAttribute) && (si1.SchemaElement == si2.SchemaElement)) && (si1.SchemaType == si2.SchemaType))) && (si1.Validity == si2.Validity));
        }

        public int GetHashCode(XmlSchemaInfo si)
        {
            if (si == null)
            {
                return 0;
            }
            int contentType = (int) si.ContentType;
            if (si.IsDefault)
            {
                contentType ^= 1;
            }
            if (si.IsNil)
            {
                contentType ^= 1;
            }
            XmlSchemaSimpleType memberType = si.MemberType;
            if (memberType != null)
            {
                contentType ^= memberType.GetHashCode();
            }
            XmlSchemaAttribute schemaAttribute = si.SchemaAttribute;
            if (schemaAttribute != null)
            {
                contentType ^= schemaAttribute.GetHashCode();
            }
            XmlSchemaElement schemaElement = si.SchemaElement;
            if (schemaElement != null)
            {
                contentType ^= schemaElement.GetHashCode();
            }
            XmlSchemaType schemaType = si.SchemaType;
            if (schemaType != null)
            {
                contentType ^= schemaType.GetHashCode();
            }
            return (contentType ^ si.Validity);
        }
    }
}

