namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class QNameFacetsChecker : FacetsChecker
    {
        internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            XmlQualifiedName name = (XmlQualifiedName) datatype.ValueConverter.ChangeType(value, typeof(XmlQualifiedName));
            return this.CheckValueFacets(name, datatype);
        }

        internal override Exception CheckValueFacets(XmlQualifiedName value, XmlSchemaDatatype datatype)
        {
            RestrictionFacets restriction = datatype.Restriction;
            RestrictionFlags flags = (restriction != null) ? restriction.Flags : ((RestrictionFlags) 0);
            if (flags != 0)
            {
                int length = value.ToString().Length;
                if (((flags & RestrictionFlags.Length) != 0) && (restriction.Length != length))
                {
                    return new XmlSchemaException("Sch_LengthConstraintFailed", string.Empty);
                }
                if (((flags & RestrictionFlags.MinLength) != 0) && (length < restriction.MinLength))
                {
                    return new XmlSchemaException("Sch_MinLengthConstraintFailed", string.Empty);
                }
                if (((flags & RestrictionFlags.MaxLength) != 0) && (restriction.MaxLength < length))
                {
                    return new XmlSchemaException("Sch_MaxLengthConstraintFailed", string.Empty);
                }
                if (((flags & RestrictionFlags.Enumeration) != 0) && !this.MatchEnumeration(value, restriction.Enumeration))
                {
                    return new XmlSchemaException("Sch_EnumerationConstraintFailed", string.Empty);
                }
            }
            return null;
        }

        private bool MatchEnumeration(XmlQualifiedName value, ArrayList enumeration)
        {
            for (int i = 0; i < enumeration.Count; i++)
            {
                if (value.Equals((XmlQualifiedName) enumeration[i]))
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            return this.MatchEnumeration((XmlQualifiedName) datatype.ValueConverter.ChangeType(value, typeof(XmlQualifiedName)), enumeration);
        }
    }
}

