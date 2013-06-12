namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class ListFacetsChecker : FacetsChecker
    {
        internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            Array array = value as Array;
            RestrictionFacets restriction = datatype.Restriction;
            RestrictionFlags flags = (restriction != null) ? restriction.Flags : ((RestrictionFlags) 0);
            if ((flags & (RestrictionFlags.MaxLength | RestrictionFlags.MinLength | RestrictionFlags.Length)) != 0)
            {
                int length = array.Length;
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
            }
            if (((flags & RestrictionFlags.Enumeration) != 0) && !this.MatchEnumeration(value, restriction.Enumeration, datatype))
            {
                return new XmlSchemaException("Sch_EnumerationConstraintFailed", string.Empty);
            }
            return null;
        }

        internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            for (int i = 0; i < enumeration.Count; i++)
            {
                if (datatype.Compare(value, enumeration[i]) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

