namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Xml;

    internal class Numeric10FacetsChecker : FacetsChecker
    {
        private decimal maxValue;
        private decimal minValue;
        private static readonly char[] signs = new char[] { '+', '-' };

        internal Numeric10FacetsChecker(decimal minVal, decimal maxVal)
        {
            this.minValue = minVal;
            this.maxValue = maxVal;
        }

        internal Exception CheckTotalAndFractionDigits(decimal value, int totalDigits, int fractionDigits, bool checkTotal, bool checkFraction)
        {
            decimal num = decimal.op_Decrement(FacetsChecker.Power(10, totalDigits));
            int num2 = 0;
            if (value < 0M)
            {
                value = decimal.Negate(value);
            }
            while (decimal.Truncate(value) != value)
            {
                value *= 10M;
                num2++;
            }
            if (checkTotal && ((value > num) || (num2 > totalDigits)))
            {
                return new XmlSchemaException("Sch_TotalDigitsConstraintFailed", string.Empty);
            }
            if (checkFraction && (num2 > fractionDigits))
            {
                return new XmlSchemaException("Sch_FractionDigitsConstraintFailed", string.Empty);
            }
            return null;
        }

        internal override Exception CheckValueFacets(byte value, XmlSchemaDatatype datatype)
        {
            decimal num = value;
            return this.CheckValueFacets(num, datatype);
        }

        internal override Exception CheckValueFacets(decimal value, XmlSchemaDatatype datatype)
        {
            RestrictionFacets restriction = datatype.Restriction;
            RestrictionFlags flags = (restriction != null) ? restriction.Flags : ((RestrictionFlags) 0);
            XmlValueConverter valueConverter = datatype.ValueConverter;
            if ((value > this.maxValue) || (value < this.minValue))
            {
                return new OverflowException(Res.GetString("XmlConvert_Overflow", new object[] { value.ToString(CultureInfo.InvariantCulture), datatype.TypeCodeString }));
            }
            if (flags == 0)
            {
                return null;
            }
            if (((flags & RestrictionFlags.MaxInclusive) != 0) && (value > valueConverter.ToDecimal(restriction.MaxInclusive)))
            {
                return new XmlSchemaException("Sch_MaxInclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MaxExclusive) != 0) && (value >= valueConverter.ToDecimal(restriction.MaxExclusive)))
            {
                return new XmlSchemaException("Sch_MaxExclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MinInclusive) != 0) && (value < valueConverter.ToDecimal(restriction.MinInclusive)))
            {
                return new XmlSchemaException("Sch_MinInclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.MinExclusive) != 0) && (value <= valueConverter.ToDecimal(restriction.MinExclusive)))
            {
                return new XmlSchemaException("Sch_MinExclusiveConstraintFailed", string.Empty);
            }
            if (((flags & RestrictionFlags.Enumeration) != 0) && !this.MatchEnumeration(value, restriction.Enumeration, valueConverter))
            {
                return new XmlSchemaException("Sch_EnumerationConstraintFailed", string.Empty);
            }
            return this.CheckTotalAndFractionDigits(value, restriction.TotalDigits, restriction.FractionDigits, (flags & RestrictionFlags.TotalDigits) != 0, (flags & RestrictionFlags.FractionDigits) != 0);
        }

        internal override Exception CheckValueFacets(short value, XmlSchemaDatatype datatype)
        {
            decimal num = value;
            return this.CheckValueFacets(num, datatype);
        }

        internal override Exception CheckValueFacets(int value, XmlSchemaDatatype datatype)
        {
            decimal num = value;
            return this.CheckValueFacets(num, datatype);
        }

        internal override Exception CheckValueFacets(long value, XmlSchemaDatatype datatype)
        {
            decimal num = value;
            return this.CheckValueFacets(num, datatype);
        }

        internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            decimal num = datatype.ValueConverter.ToDecimal(value);
            return this.CheckValueFacets(num, datatype);
        }

        internal bool MatchEnumeration(decimal value, ArrayList enumeration, XmlValueConverter valueConverter)
        {
            for (int i = 0; i < enumeration.Count; i++)
            {
                if (value == valueConverter.ToDecimal(enumeration[i]))
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            return this.MatchEnumeration(datatype.ValueConverter.ToDecimal(value), enumeration, datatype.ValueConverter);
        }
    }
}

