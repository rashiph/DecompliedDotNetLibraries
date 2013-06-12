namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class BinaryFacetsChecker : FacetsChecker
    {
        internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            byte[] buffer = (byte[]) value;
            return this.CheckValueFacets(buffer, datatype);
        }

        internal override Exception CheckValueFacets(byte[] value, XmlSchemaDatatype datatype)
        {
            RestrictionFacets restriction = datatype.Restriction;
            int length = value.Length;
            RestrictionFlags flags = (restriction != null) ? restriction.Flags : ((RestrictionFlags) 0);
            if (flags != 0)
            {
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
                if (((flags & RestrictionFlags.Enumeration) != 0) && !this.MatchEnumeration(value, restriction.Enumeration, datatype))
                {
                    return new XmlSchemaException("Sch_EnumerationConstraintFailed", string.Empty);
                }
            }
            return null;
        }

        internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            return this.MatchEnumeration((byte[]) value, enumeration, datatype);
        }

        private bool MatchEnumeration(byte[] value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            for (int i = 0; i < enumeration.Count; i++)
            {
                if (datatype.Compare(value, (byte[]) enumeration[i]) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

