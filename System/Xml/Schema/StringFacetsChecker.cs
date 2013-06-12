namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;

    internal class StringFacetsChecker : FacetsChecker
    {
        private static Regex languagePattern;

        private Exception CheckBuiltInFacets(string s, XmlTypeCode typeCode, bool verifyUri)
        {
            Exception exception = null;
            switch (typeCode)
            {
                case XmlTypeCode.AnyUri:
                    if (verifyUri)
                    {
                        Uri uri;
                        exception = XmlConvert.TryToUri(s, out uri);
                    }
                    return exception;

                case XmlTypeCode.QName:
                case XmlTypeCode.Notation:
                    return exception;

                case XmlTypeCode.NormalizedString:
                    return XmlConvert.TryVerifyNormalizedString(s);

                case XmlTypeCode.Token:
                    return XmlConvert.TryVerifyTOKEN(s);

                case XmlTypeCode.Language:
                    if ((s != null) && (s.Length != 0))
                    {
                        if (!LanguagePattern.IsMatch(s))
                        {
                            return new XmlSchemaException("Sch_InvalidLanguageId", string.Empty);
                        }
                        return exception;
                    }
                    return new XmlSchemaException("Sch_EmptyAttributeValue", string.Empty);

                case XmlTypeCode.NmToken:
                    return XmlConvert.TryVerifyNMTOKEN(s);

                case XmlTypeCode.Name:
                    return XmlConvert.TryVerifyName(s);

                case XmlTypeCode.NCName:
                case XmlTypeCode.Id:
                case XmlTypeCode.Idref:
                case XmlTypeCode.Entity:
                    return XmlConvert.TryVerifyNCName(s);
            }
            return exception;
        }

        internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            string str = datatype.ValueConverter.ToString(value);
            return this.CheckValueFacets(str, datatype, true);
        }

        internal override Exception CheckValueFacets(string value, XmlSchemaDatatype datatype)
        {
            return this.CheckValueFacets(value, datatype, true);
        }

        internal Exception CheckValueFacets(string value, XmlSchemaDatatype datatype, bool verifyUri)
        {
            int length = value.Length;
            RestrictionFacets restriction = datatype.Restriction;
            RestrictionFlags flags = (restriction != null) ? restriction.Flags : ((RestrictionFlags) 0);
            Exception exception = this.CheckBuiltInFacets(value, datatype.TypeCode, verifyUri);
            if (exception != null)
            {
                return exception;
            }
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
            return this.MatchEnumeration(datatype.ValueConverter.ToString(value), enumeration, datatype);
        }

        private bool MatchEnumeration(string value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            if (datatype.TypeCode == XmlTypeCode.AnyUri)
            {
                for (int i = 0; i < enumeration.Count; i++)
                {
                    if (value.Equals(((Uri) enumeration[i]).OriginalString))
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int j = 0; j < enumeration.Count; j++)
                {
                    if (value.Equals((string) enumeration[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Regex LanguagePattern
        {
            get
            {
                if (languagePattern == null)
                {
                    Regex regex = new Regex("^([a-zA-Z]{1,8})(-[a-zA-Z0-9]{1,8})*$", RegexOptions.None);
                    Interlocked.CompareExchange<Regex>(ref languagePattern, regex, null);
                }
                return languagePattern;
            }
        }
    }
}

