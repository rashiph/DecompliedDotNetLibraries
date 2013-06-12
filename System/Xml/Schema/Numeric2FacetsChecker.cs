namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class Numeric2FacetsChecker : FacetsChecker
    {
        internal override Exception CheckValueFacets(double value, XmlSchemaDatatype datatype)
        {
            RestrictionFacets restriction = datatype.Restriction;
            RestrictionFlags flags = (restriction != null) ? restriction.Flags : ((RestrictionFlags) 0);
            XmlValueConverter valueConverter = datatype.ValueConverter;
            if (((flags & RestrictionFlags.MaxInclusive) != 0) && (value > valueConverter.ToDouble(restriction.MaxInclusive)))
            {
                return new XmlSchemaException("Sch_MaxInclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MaxExclusive) != 0) && (value >= valueConverter.ToDouble(restriction.MaxExclusive)))
            {
                return new XmlSchemaException("Sch_MaxExclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MinInclusive) != 0) && (value < valueConverter.ToDouble(restriction.MinInclusive)))
            {
                return new XmlSchemaException("Sch_MinInclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MinExclusive) != 0) && (value <= valueConverter.ToDouble(restriction.MinExclusive)))
            {
                return new XmlSchemaException("Sch_MinExclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.Enumeration) != 0) && !this.MatchEnumeration(value, restriction.Enumeration, valueConverter))
            {
                return new XmlSchemaException("Sch_EnumerationConstraintFailed", string.Empty);
            }
            return null;
        }

        internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            double num = datatype.ValueConverter.ToDouble(value);
            return this.CheckValueFacets(num, datatype);
        }

        internal override Exception CheckValueFacets(float value, XmlSchemaDatatype datatype)
        {
            double num = value;
            return this.CheckValueFacets(num, datatype);
        }

        private bool MatchEnumeration(double value, ArrayList enumeration, XmlValueConverter valueConverter)
        {
            for (int i = 0; i < enumeration.Count; i++)
            {
                if (value == valueConverter.ToDouble(enumeration[i]))
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            return this.MatchEnumeration(datatype.ValueConverter.ToDouble(value), enumeration, datatype.ValueConverter);
        }
    }
}

