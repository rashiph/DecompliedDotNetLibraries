namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class DateTimeFacetsChecker : FacetsChecker
    {
        internal override Exception CheckValueFacets(DateTime value, XmlSchemaDatatype datatype)
        {
            RestrictionFacets restriction = datatype.Restriction;
            RestrictionFlags flags = (restriction != null) ? restriction.Flags : ((RestrictionFlags) 0);
            if (((flags & RestrictionFlags.MaxInclusive) != 0) && (datatype.Compare(value, (DateTime) restriction.MaxInclusive) > 0))
            {
                return new XmlSchemaException("Sch_MaxInclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MaxExclusive) != 0) && (datatype.Compare(value, (DateTime) restriction.MaxExclusive) >= 0))
            {
                return new XmlSchemaException("Sch_MaxExclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MinInclusive) != 0) && (datatype.Compare(value, (DateTime) restriction.MinInclusive) < 0))
            {
                return new XmlSchemaException("Sch_MinInclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MinExclusive) != 0) && (datatype.Compare(value, (DateTime) restriction.MinExclusive) <= 0))
            {
                return new XmlSchemaException("Sch_MinExclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.Enumeration) != 0) && !this.MatchEnumeration(value, restriction.Enumeration, datatype))
            {
                return new XmlSchemaException("Sch_EnumerationConstraintFailed", string.Empty);
            }
            return null;
        }

        internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            DateTime time = datatype.ValueConverter.ToDateTime(value);
            return this.CheckValueFacets(time, datatype);
        }

        private bool MatchEnumeration(DateTime value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            for (int i = 0; i < enumeration.Count; i++)
            {
                if (datatype.Compare(value, (DateTime) enumeration[i]) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            return this.MatchEnumeration(datatype.ValueConverter.ToDateTime(value), enumeration, datatype);
        }
    }
}

