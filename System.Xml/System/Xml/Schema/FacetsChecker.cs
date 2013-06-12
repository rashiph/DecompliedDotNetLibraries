namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal abstract class FacetsChecker
    {
        protected FacetsChecker()
        {
        }

        internal virtual Exception CheckLexicalFacets(ref string parseString, XmlSchemaDatatype datatype)
        {
            this.CheckWhitespaceFacets(ref parseString, datatype);
            return this.CheckPatternFacets(datatype.Restriction, parseString);
        }

        internal Exception CheckPatternFacets(RestrictionFacets restriction, string value)
        {
            if ((restriction != null) && ((restriction.Flags & RestrictionFlags.Pattern) != 0))
            {
                for (int i = 0; i < restriction.Patterns.Count; i++)
                {
                    Regex regex = (Regex) restriction.Patterns[i];
                    if (!regex.IsMatch(value))
                    {
                        return new XmlSchemaException("Sch_PatternConstraintFailed", string.Empty);
                    }
                }
            }
            return null;
        }

        internal virtual Exception CheckValueFacets(byte value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(DateTime value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(decimal value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(double value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(short value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(int value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(long value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(float value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(string value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(byte[] value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(TimeSpan value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal virtual Exception CheckValueFacets(XmlQualifiedName value, XmlSchemaDatatype datatype)
        {
            return null;
        }

        internal void CheckWhitespaceFacets(ref string s, XmlSchemaDatatype datatype)
        {
            RestrictionFacets restriction = datatype.Restriction;
            switch (datatype.Variety)
            {
                case XmlSchemaDatatypeVariety.Atomic:
                    if (datatype.BuiltInWhitespaceFacet != XmlSchemaWhiteSpace.Collapse)
                    {
                        if (datatype.BuiltInWhitespaceFacet == XmlSchemaWhiteSpace.Replace)
                        {
                            s = XmlComplianceUtil.CDataNormalize(s);
                        }
                        else if ((restriction != null) && ((restriction.Flags & RestrictionFlags.WhiteSpace) != 0))
                        {
                            if (restriction.WhiteSpace == XmlSchemaWhiteSpace.Replace)
                            {
                                s = XmlComplianceUtil.CDataNormalize(s);
                                return;
                            }
                            if (restriction.WhiteSpace == XmlSchemaWhiteSpace.Collapse)
                            {
                                s = XmlComplianceUtil.NonCDataNormalize(s);
                            }
                        }
                        return;
                    }
                    s = XmlComplianceUtil.NonCDataNormalize(s);
                    return;

                case XmlSchemaDatatypeVariety.List:
                    s = s.Trim();
                    return;
            }
        }

        internal virtual RestrictionFacets ConstructRestriction(DatatypeImplementation datatype, XmlSchemaObjectCollection facets, XmlNameTable nameTable)
        {
            RestrictionFacets restriction = new RestrictionFacets();
            FacetsCompiler compiler = new FacetsCompiler(datatype, restriction);
            for (int i = 0; i < facets.Count; i++)
            {
                XmlSchemaFacet source = (XmlSchemaFacet) facets[i];
                if (source.Value == null)
                {
                    throw new XmlSchemaException("Sch_InvalidFacet", source);
                }
                IXmlNamespaceResolver nsmgr = new SchemaNamespaceManager(source);
                switch (source.FacetType)
                {
                    case FacetType.Length:
                        compiler.CompileLengthFacet(source);
                        break;

                    case FacetType.MinLength:
                        compiler.CompileMinLengthFacet(source);
                        break;

                    case FacetType.MaxLength:
                        compiler.CompileMaxLengthFacet(source);
                        break;

                    case FacetType.Pattern:
                        compiler.CompilePatternFacet(source as XmlSchemaPatternFacet);
                        break;

                    case FacetType.Whitespace:
                        compiler.CompileWhitespaceFacet(source);
                        break;

                    case FacetType.Enumeration:
                        compiler.CompileEnumerationFacet(source, nsmgr, nameTable);
                        break;

                    case FacetType.MinExclusive:
                        compiler.CompileMinExclusiveFacet(source);
                        break;

                    case FacetType.MinInclusive:
                        compiler.CompileMinInclusiveFacet(source);
                        break;

                    case FacetType.MaxExclusive:
                        compiler.CompileMaxExclusiveFacet(source);
                        break;

                    case FacetType.MaxInclusive:
                        compiler.CompileMaxInclusiveFacet(source);
                        break;

                    case FacetType.TotalDigits:
                        compiler.CompileTotalDigitsFacet(source);
                        break;

                    case FacetType.FractionDigits:
                        compiler.CompileFractionDigitsFacet(source);
                        break;

                    default:
                        throw new XmlSchemaException("Sch_UnknownFacet", source);
                }
            }
            compiler.FinishFacetCompile();
            compiler.CompileFacetCombinations();
            return restriction;
        }

        internal virtual bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
        {
            return false;
        }

        internal static decimal Power(int x, int y)
        {
            decimal num = 1M;
            decimal num2 = x;
            if (y > 0x1c)
            {
                return 79228162514264337593543950335M;
            }
            for (int i = 0; i < y; i++)
            {
                num *= num2;
            }
            return num;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FacetsCompiler
        {
            private DatatypeImplementation datatype;
            private RestrictionFacets derivedRestriction;
            private RestrictionFlags baseFlags;
            private RestrictionFlags baseFixedFlags;
            private RestrictionFlags validRestrictionFlags;
            private XmlSchemaDatatype nonNegativeInt;
            private XmlSchemaDatatype builtInType;
            private XmlTypeCode builtInEnum;
            private bool firstPattern;
            private StringBuilder regStr;
            private XmlSchemaPatternFacet pattern_facet;
            private static readonly Map[] c_map;
            public FacetsCompiler(DatatypeImplementation baseDatatype, RestrictionFacets restriction)
            {
                this.firstPattern = true;
                this.regStr = null;
                this.pattern_facet = null;
                this.datatype = baseDatatype;
                this.derivedRestriction = restriction;
                this.baseFlags = (this.datatype.Restriction != null) ? this.datatype.Restriction.Flags : ((RestrictionFlags) 0);
                this.baseFixedFlags = (this.datatype.Restriction != null) ? this.datatype.Restriction.FixedFlags : ((RestrictionFlags) 0);
                this.validRestrictionFlags = this.datatype.ValidRestrictionFlags;
                this.nonNegativeInt = DatatypeImplementation.GetSimpleTypeFromTypeCode(XmlTypeCode.NonNegativeInteger).Datatype;
                this.builtInEnum = (!(this.datatype is Datatype_union) && !(this.datatype is Datatype_List)) ? this.datatype.TypeCode : XmlTypeCode.None;
                this.builtInType = (this.builtInEnum > XmlTypeCode.None) ? DatatypeImplementation.GetSimpleTypeFromTypeCode(this.builtInEnum).Datatype : this.datatype;
            }

            internal void CompileLengthFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.Length, "Sch_LengthFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.Length, "Sch_DupLengthFacet");
                this.derivedRestriction.Length = XmlBaseConverter.DecimalToInt32((decimal) this.ParseFacetValue(this.nonNegativeInt, facet, "Sch_LengthFacetInvalid", null, null));
                if (((this.baseFixedFlags & RestrictionFlags.Length) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.Length, this.derivedRestriction.Length))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                if (((this.baseFlags & RestrictionFlags.Length) != 0) && (this.datatype.Restriction.Length < this.derivedRestriction.Length))
                {
                    throw new XmlSchemaException("Sch_LengthGtBaseLength", facet);
                }
                if (((this.baseFlags & RestrictionFlags.MinLength) != 0) && (this.datatype.Restriction.MinLength > this.derivedRestriction.Length))
                {
                    throw new XmlSchemaException("Sch_MaxMinLengthBaseLength", facet);
                }
                if (((this.baseFlags & RestrictionFlags.MaxLength) != 0) && (this.datatype.Restriction.MaxLength < this.derivedRestriction.Length))
                {
                    throw new XmlSchemaException("Sch_MaxMinLengthBaseLength", facet);
                }
                this.SetFlag(facet, RestrictionFlags.Length);
            }

            internal void CompileMinLengthFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.MinLength, "Sch_MinLengthFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.MinLength, "Sch_DupMinLengthFacet");
                this.derivedRestriction.MinLength = XmlBaseConverter.DecimalToInt32((decimal) this.ParseFacetValue(this.nonNegativeInt, facet, "Sch_MinLengthFacetInvalid", null, null));
                if (((this.baseFixedFlags & RestrictionFlags.MinLength) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.MinLength, this.derivedRestriction.MinLength))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                if (((this.baseFlags & RestrictionFlags.MinLength) != 0) && (this.datatype.Restriction.MinLength > this.derivedRestriction.MinLength))
                {
                    throw new XmlSchemaException("Sch_MinLengthGtBaseMinLength", facet);
                }
                if (((this.baseFlags & RestrictionFlags.Length) != 0) && (this.datatype.Restriction.Length < this.derivedRestriction.MinLength))
                {
                    throw new XmlSchemaException("Sch_MaxMinLengthBaseLength", facet);
                }
                this.SetFlag(facet, RestrictionFlags.MinLength);
            }

            internal void CompileMaxLengthFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.MaxLength, "Sch_MaxLengthFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.MaxLength, "Sch_DupMaxLengthFacet");
                this.derivedRestriction.MaxLength = XmlBaseConverter.DecimalToInt32((decimal) this.ParseFacetValue(this.nonNegativeInt, facet, "Sch_MaxLengthFacetInvalid", null, null));
                if (((this.baseFixedFlags & RestrictionFlags.MaxLength) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.MaxLength, this.derivedRestriction.MaxLength))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                if (((this.baseFlags & RestrictionFlags.MaxLength) != 0) && (this.datatype.Restriction.MaxLength < this.derivedRestriction.MaxLength))
                {
                    throw new XmlSchemaException("Sch_MaxLengthGtBaseMaxLength", facet);
                }
                if (((this.baseFlags & RestrictionFlags.Length) != 0) && (this.datatype.Restriction.Length > this.derivedRestriction.MaxLength))
                {
                    throw new XmlSchemaException("Sch_MaxMinLengthBaseLength", facet);
                }
                this.SetFlag(facet, RestrictionFlags.MaxLength);
            }

            internal void CompilePatternFacet(XmlSchemaPatternFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.Pattern, "Sch_PatternFacetProhibited");
                if (this.firstPattern)
                {
                    this.regStr = new StringBuilder();
                    this.regStr.Append("(");
                    this.regStr.Append(facet.Value);
                    this.pattern_facet = facet;
                    this.firstPattern = false;
                }
                else
                {
                    this.regStr.Append(")|(");
                    this.regStr.Append(facet.Value);
                }
                this.SetFlag(facet, RestrictionFlags.Pattern);
            }

            internal void CompileEnumerationFacet(XmlSchemaFacet facet, IXmlNamespaceResolver nsmgr, XmlNameTable nameTable)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.Enumeration, "Sch_EnumerationFacetProhibited");
                if (this.derivedRestriction.Enumeration == null)
                {
                    this.derivedRestriction.Enumeration = new ArrayList();
                }
                this.derivedRestriction.Enumeration.Add(this.ParseFacetValue(this.datatype, facet, "Sch_EnumerationFacetInvalid", nsmgr, nameTable));
                this.SetFlag(facet, RestrictionFlags.Enumeration);
            }

            internal void CompileWhitespaceFacet(XmlSchemaFacet facet)
            {
                XmlSchemaWhiteSpace whiteSpace;
                this.CheckProhibitedFlag(facet, RestrictionFlags.WhiteSpace, "Sch_WhiteSpaceFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.WhiteSpace, "Sch_DupWhiteSpaceFacet");
                if (facet.Value == "preserve")
                {
                    this.derivedRestriction.WhiteSpace = XmlSchemaWhiteSpace.Preserve;
                }
                else if (facet.Value == "replace")
                {
                    this.derivedRestriction.WhiteSpace = XmlSchemaWhiteSpace.Replace;
                }
                else
                {
                    if (facet.Value != "collapse")
                    {
                        throw new XmlSchemaException("Sch_InvalidWhiteSpace", facet.Value, facet);
                    }
                    this.derivedRestriction.WhiteSpace = XmlSchemaWhiteSpace.Collapse;
                }
                if (((this.baseFixedFlags & RestrictionFlags.WhiteSpace) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.WhiteSpace, this.derivedRestriction.WhiteSpace))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                if ((this.baseFlags & RestrictionFlags.WhiteSpace) != 0)
                {
                    whiteSpace = this.datatype.Restriction.WhiteSpace;
                }
                else
                {
                    whiteSpace = this.datatype.BuiltInWhitespaceFacet;
                }
                if ((whiteSpace == XmlSchemaWhiteSpace.Collapse) && ((this.derivedRestriction.WhiteSpace == XmlSchemaWhiteSpace.Replace) || (this.derivedRestriction.WhiteSpace == XmlSchemaWhiteSpace.Preserve)))
                {
                    throw new XmlSchemaException("Sch_WhiteSpaceRestriction1", facet);
                }
                if ((whiteSpace == XmlSchemaWhiteSpace.Replace) && (this.derivedRestriction.WhiteSpace == XmlSchemaWhiteSpace.Preserve))
                {
                    throw new XmlSchemaException("Sch_WhiteSpaceRestriction2", facet);
                }
                this.SetFlag(facet, RestrictionFlags.WhiteSpace);
            }

            internal void CompileMaxInclusiveFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.MaxInclusive, "Sch_MaxInclusiveFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.MaxInclusive, "Sch_DupMaxInclusiveFacet");
                this.derivedRestriction.MaxInclusive = this.ParseFacetValue(this.builtInType, facet, "Sch_MaxInclusiveFacetInvalid", null, null);
                if (((this.baseFixedFlags & RestrictionFlags.MaxInclusive) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.MaxInclusive, this.derivedRestriction.MaxInclusive))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                this.CheckValue(this.derivedRestriction.MaxInclusive, facet);
                this.SetFlag(facet, RestrictionFlags.MaxInclusive);
            }

            internal void CompileMaxExclusiveFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.MaxExclusive, "Sch_MaxExclusiveFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.MaxExclusive, "Sch_DupMaxExclusiveFacet");
                this.derivedRestriction.MaxExclusive = this.ParseFacetValue(this.builtInType, facet, "Sch_MaxExclusiveFacetInvalid", null, null);
                if (((this.baseFixedFlags & RestrictionFlags.MaxExclusive) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.MaxExclusive, this.derivedRestriction.MaxExclusive))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                this.CheckValue(this.derivedRestriction.MaxExclusive, facet);
                this.SetFlag(facet, RestrictionFlags.MaxExclusive);
            }

            internal void CompileMinInclusiveFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.MinInclusive, "Sch_MinInclusiveFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.MinInclusive, "Sch_DupMinInclusiveFacet");
                this.derivedRestriction.MinInclusive = this.ParseFacetValue(this.builtInType, facet, "Sch_MinInclusiveFacetInvalid", null, null);
                if (((this.baseFixedFlags & RestrictionFlags.MinInclusive) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.MinInclusive, this.derivedRestriction.MinInclusive))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                this.CheckValue(this.derivedRestriction.MinInclusive, facet);
                this.SetFlag(facet, RestrictionFlags.MinInclusive);
            }

            internal void CompileMinExclusiveFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.MinExclusive, "Sch_MinExclusiveFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.MinExclusive, "Sch_DupMinExclusiveFacet");
                this.derivedRestriction.MinExclusive = this.ParseFacetValue(this.builtInType, facet, "Sch_MinExclusiveFacetInvalid", null, null);
                if (((this.baseFixedFlags & RestrictionFlags.MinExclusive) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.MinExclusive, this.derivedRestriction.MinExclusive))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                this.CheckValue(this.derivedRestriction.MinExclusive, facet);
                this.SetFlag(facet, RestrictionFlags.MinExclusive);
            }

            internal void CompileTotalDigitsFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.TotalDigits, "Sch_TotalDigitsFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.TotalDigits, "Sch_DupTotalDigitsFacet");
                XmlSchemaDatatype datatype = DatatypeImplementation.GetSimpleTypeFromTypeCode(XmlTypeCode.PositiveInteger).Datatype;
                this.derivedRestriction.TotalDigits = XmlBaseConverter.DecimalToInt32((decimal) this.ParseFacetValue(datatype, facet, "Sch_TotalDigitsFacetInvalid", null, null));
                if (((this.baseFixedFlags & RestrictionFlags.TotalDigits) != 0) && !this.datatype.IsEqual(this.datatype.Restriction.TotalDigits, this.derivedRestriction.TotalDigits))
                {
                    throw new XmlSchemaException("Sch_FacetBaseFixed", facet);
                }
                if (((this.baseFlags & RestrictionFlags.TotalDigits) != 0) && (this.derivedRestriction.TotalDigits > this.datatype.Restriction.TotalDigits))
                {
                    throw new XmlSchemaException("Sch_TotalDigitsMismatch", string.Empty);
                }
                this.SetFlag(facet, RestrictionFlags.TotalDigits);
            }

            internal void CompileFractionDigitsFacet(XmlSchemaFacet facet)
            {
                this.CheckProhibitedFlag(facet, RestrictionFlags.FractionDigits, "Sch_FractionDigitsFacetProhibited");
                this.CheckDupFlag(facet, RestrictionFlags.FractionDigits, "Sch_DupFractionDigitsFacet");
                this.derivedRestriction.FractionDigits = XmlBaseConverter.DecimalToInt32((decimal) this.ParseFacetValue(this.nonNegativeInt, facet, "Sch_FractionDigitsFacetInvalid", null, null));
                if ((this.derivedRestriction.FractionDigits != 0) && (this.datatype.TypeCode != XmlTypeCode.Decimal))
                {
                    throw new XmlSchemaException("Sch_FractionDigitsFacetInvalid", Res.GetString("Sch_FractionDigitsNotOnDecimal"), facet);
                }
                if (((this.baseFlags & RestrictionFlags.FractionDigits) != 0) && (this.derivedRestriction.FractionDigits > this.datatype.Restriction.FractionDigits))
                {
                    throw new XmlSchemaException("Sch_TotalDigitsMismatch", string.Empty);
                }
                this.SetFlag(facet, RestrictionFlags.FractionDigits);
            }

            internal void FinishFacetCompile()
            {
                if (!this.firstPattern)
                {
                    if (this.derivedRestriction.Patterns == null)
                    {
                        this.derivedRestriction.Patterns = new ArrayList();
                    }
                    try
                    {
                        this.regStr.Append(")");
                        if (this.regStr.ToString().IndexOf('|') != -1)
                        {
                            this.regStr.Insert(0, "(");
                            this.regStr.Append(")");
                        }
                        this.derivedRestriction.Patterns.Add(new Regex(Preprocess(this.regStr.ToString()), RegexOptions.None));
                    }
                    catch (Exception exception)
                    {
                        throw new XmlSchemaException("Sch_PatternFacetInvalid", new string[] { exception.Message }, exception, this.pattern_facet.SourceUri, this.pattern_facet.LineNumber, this.pattern_facet.LinePosition, this.pattern_facet);
                    }
                }
            }

            private void CheckValue(object value, XmlSchemaFacet facet)
            {
                RestrictionFacets restriction = this.datatype.Restriction;
                switch (facet.FacetType)
                {
                    case FacetType.MinExclusive:
                        if (((this.baseFlags & RestrictionFlags.MinExclusive) != 0) && (this.datatype.Compare(value, restriction.MinExclusive) < 0))
                        {
                            throw new XmlSchemaException("Sch_MinExclusiveMismatch", string.Empty);
                        }
                        if (((this.baseFlags & RestrictionFlags.MinInclusive) != 0) && (this.datatype.Compare(value, restriction.MinInclusive) < 0))
                        {
                            throw new XmlSchemaException("Sch_MinExlIncMismatch", string.Empty);
                        }
                        if (((this.baseFlags & RestrictionFlags.MaxExclusive) != 0) && (this.datatype.Compare(value, restriction.MaxExclusive) >= 0))
                        {
                            throw new XmlSchemaException("Sch_MinExlMaxExlMismatch", string.Empty);
                        }
                        break;

                    case FacetType.MinInclusive:
                        if (((this.baseFlags & RestrictionFlags.MinInclusive) != 0) && (this.datatype.Compare(value, restriction.MinInclusive) < 0))
                        {
                            throw new XmlSchemaException("Sch_MinInclusiveMismatch", string.Empty);
                        }
                        if (((this.baseFlags & RestrictionFlags.MinExclusive) != 0) && (this.datatype.Compare(value, restriction.MinExclusive) < 0))
                        {
                            throw new XmlSchemaException("Sch_MinIncExlMismatch", string.Empty);
                        }
                        if (((this.baseFlags & RestrictionFlags.MaxExclusive) == 0) || (this.datatype.Compare(value, restriction.MaxExclusive) < 0))
                        {
                            break;
                        }
                        throw new XmlSchemaException("Sch_MinIncMaxExlMismatch", string.Empty);

                    case FacetType.MaxExclusive:
                        if (((this.baseFlags & RestrictionFlags.MaxExclusive) != 0) && (this.datatype.Compare(value, restriction.MaxExclusive) > 0))
                        {
                            throw new XmlSchemaException("Sch_MaxExclusiveMismatch", string.Empty);
                        }
                        if (((this.baseFlags & RestrictionFlags.MaxInclusive) == 0) || (this.datatype.Compare(value, restriction.MaxInclusive) <= 0))
                        {
                            break;
                        }
                        throw new XmlSchemaException("Sch_MaxExlIncMismatch", string.Empty);

                    case FacetType.MaxInclusive:
                        if (((this.baseFlags & RestrictionFlags.MaxInclusive) != 0) && (this.datatype.Compare(value, restriction.MaxInclusive) > 0))
                        {
                            throw new XmlSchemaException("Sch_MaxInclusiveMismatch", string.Empty);
                        }
                        if (((this.baseFlags & RestrictionFlags.MaxExclusive) == 0) || (this.datatype.Compare(value, restriction.MaxExclusive) < 0))
                        {
                            break;
                        }
                        throw new XmlSchemaException("Sch_MaxIncExlMismatch", string.Empty);

                    default:
                        return;
                }
            }

            internal void CompileFacetCombinations()
            {
                RestrictionFacets restriction = this.datatype.Restriction;
                if (((this.derivedRestriction.Flags & RestrictionFlags.MaxInclusive) != 0) && ((this.derivedRestriction.Flags & RestrictionFlags.MaxExclusive) != 0))
                {
                    throw new XmlSchemaException("Sch_MaxInclusiveExclusive", string.Empty);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.MinInclusive) != 0) && ((this.derivedRestriction.Flags & RestrictionFlags.MinExclusive) != 0))
                {
                    throw new XmlSchemaException("Sch_MinInclusiveExclusive", string.Empty);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.Length) != 0) && ((this.derivedRestriction.Flags & (RestrictionFlags.MaxLength | RestrictionFlags.MinLength)) != 0))
                {
                    throw new XmlSchemaException("Sch_LengthAndMinMax", string.Empty);
                }
                this.CopyFacetsFromBaseType();
                if ((((this.derivedRestriction.Flags & RestrictionFlags.MinLength) != 0) && ((this.derivedRestriction.Flags & RestrictionFlags.MaxLength) != 0)) && (this.derivedRestriction.MinLength > this.derivedRestriction.MaxLength))
                {
                    throw new XmlSchemaException("Sch_MinLengthGtMaxLength", string.Empty);
                }
                if ((((this.derivedRestriction.Flags & RestrictionFlags.MinInclusive) != 0) && ((this.derivedRestriction.Flags & RestrictionFlags.MaxInclusive) != 0)) && (this.datatype.Compare(this.derivedRestriction.MinInclusive, this.derivedRestriction.MaxInclusive) > 0))
                {
                    throw new XmlSchemaException("Sch_MinInclusiveGtMaxInclusive", string.Empty);
                }
                if ((((this.derivedRestriction.Flags & RestrictionFlags.MinInclusive) != 0) && ((this.derivedRestriction.Flags & RestrictionFlags.MaxExclusive) != 0)) && (this.datatype.Compare(this.derivedRestriction.MinInclusive, this.derivedRestriction.MaxExclusive) > 0))
                {
                    throw new XmlSchemaException("Sch_MinInclusiveGtMaxExclusive", string.Empty);
                }
                if ((((this.derivedRestriction.Flags & RestrictionFlags.MinExclusive) != 0) && ((this.derivedRestriction.Flags & RestrictionFlags.MaxExclusive) != 0)) && (this.datatype.Compare(this.derivedRestriction.MinExclusive, this.derivedRestriction.MaxExclusive) > 0))
                {
                    throw new XmlSchemaException("Sch_MinExclusiveGtMaxExclusive", string.Empty);
                }
                if ((((this.derivedRestriction.Flags & RestrictionFlags.MinExclusive) != 0) && ((this.derivedRestriction.Flags & RestrictionFlags.MaxInclusive) != 0)) && (this.datatype.Compare(this.derivedRestriction.MinExclusive, this.derivedRestriction.MaxInclusive) > 0))
                {
                    throw new XmlSchemaException("Sch_MinExclusiveGtMaxInclusive", string.Empty);
                }
                if (((this.derivedRestriction.Flags & (RestrictionFlags.FractionDigits | RestrictionFlags.TotalDigits)) == (RestrictionFlags.FractionDigits | RestrictionFlags.TotalDigits)) && (this.derivedRestriction.FractionDigits > this.derivedRestriction.TotalDigits))
                {
                    throw new XmlSchemaException("Sch_FractionDigitsGtTotalDigits", string.Empty);
                }
            }

            private void CopyFacetsFromBaseType()
            {
                RestrictionFacets restriction = this.datatype.Restriction;
                if (((this.derivedRestriction.Flags & RestrictionFlags.Length) == 0) && ((this.baseFlags & RestrictionFlags.Length) != 0))
                {
                    this.derivedRestriction.Length = restriction.Length;
                    this.SetFlag(RestrictionFlags.Length);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.MinLength) == 0) && ((this.baseFlags & RestrictionFlags.MinLength) != 0))
                {
                    this.derivedRestriction.MinLength = restriction.MinLength;
                    this.SetFlag(RestrictionFlags.MinLength);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.MaxLength) == 0) && ((this.baseFlags & RestrictionFlags.MaxLength) != 0))
                {
                    this.derivedRestriction.MaxLength = restriction.MaxLength;
                    this.SetFlag(RestrictionFlags.MaxLength);
                }
                if ((this.baseFlags & RestrictionFlags.Pattern) != 0)
                {
                    if (this.derivedRestriction.Patterns == null)
                    {
                        this.derivedRestriction.Patterns = restriction.Patterns;
                    }
                    else
                    {
                        this.derivedRestriction.Patterns.AddRange(restriction.Patterns);
                    }
                    this.SetFlag(RestrictionFlags.Pattern);
                }
                if ((this.baseFlags & RestrictionFlags.Enumeration) != 0)
                {
                    if (this.derivedRestriction.Enumeration == null)
                    {
                        this.derivedRestriction.Enumeration = restriction.Enumeration;
                    }
                    this.SetFlag(RestrictionFlags.Enumeration);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.WhiteSpace) == 0) && ((this.baseFlags & RestrictionFlags.WhiteSpace) != 0))
                {
                    this.derivedRestriction.WhiteSpace = restriction.WhiteSpace;
                    this.SetFlag(RestrictionFlags.WhiteSpace);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.MaxInclusive) == 0) && ((this.baseFlags & RestrictionFlags.MaxInclusive) != 0))
                {
                    this.derivedRestriction.MaxInclusive = restriction.MaxInclusive;
                    this.SetFlag(RestrictionFlags.MaxInclusive);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.MaxExclusive) == 0) && ((this.baseFlags & RestrictionFlags.MaxExclusive) != 0))
                {
                    this.derivedRestriction.MaxExclusive = restriction.MaxExclusive;
                    this.SetFlag(RestrictionFlags.MaxExclusive);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.MinInclusive) == 0) && ((this.baseFlags & RestrictionFlags.MinInclusive) != 0))
                {
                    this.derivedRestriction.MinInclusive = restriction.MinInclusive;
                    this.SetFlag(RestrictionFlags.MinInclusive);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.MinExclusive) == 0) && ((this.baseFlags & RestrictionFlags.MinExclusive) != 0))
                {
                    this.derivedRestriction.MinExclusive = restriction.MinExclusive;
                    this.SetFlag(RestrictionFlags.MinExclusive);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.TotalDigits) == 0) && ((this.baseFlags & RestrictionFlags.TotalDigits) != 0))
                {
                    this.derivedRestriction.TotalDigits = restriction.TotalDigits;
                    this.SetFlag(RestrictionFlags.TotalDigits);
                }
                if (((this.derivedRestriction.Flags & RestrictionFlags.FractionDigits) == 0) && ((this.baseFlags & RestrictionFlags.FractionDigits) != 0))
                {
                    this.derivedRestriction.FractionDigits = restriction.FractionDigits;
                    this.SetFlag(RestrictionFlags.FractionDigits);
                }
            }

            private object ParseFacetValue(XmlSchemaDatatype datatype, XmlSchemaFacet facet, string code, IXmlNamespaceResolver nsmgr, XmlNameTable nameTable)
            {
                object obj2;
                Exception innerException = datatype.TryParseValue(facet.Value, nameTable, nsmgr, out obj2);
                if (innerException != null)
                {
                    throw new XmlSchemaException(code, new string[] { innerException.Message }, innerException, facet.SourceUri, facet.LineNumber, facet.LinePosition, facet);
                }
                return obj2;
            }

            private static string Preprocess(string pattern)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("^");
                char[] chArray = pattern.ToCharArray();
                int length = pattern.Length;
                int startIndex = 0;
                for (int i = 0; i < (length - 2); i++)
                {
                    if (chArray[i] == '\\')
                    {
                        if (chArray[i + 1] == '\\')
                        {
                            i++;
                        }
                        else
                        {
                            char ch = chArray[i + 1];
                            for (int j = 0; j < c_map.Length; j++)
                            {
                                if (c_map[j].match == ch)
                                {
                                    if (startIndex < i)
                                    {
                                        builder.Append(chArray, startIndex, i - startIndex);
                                    }
                                    builder.Append(c_map[j].replacement);
                                    i++;
                                    startIndex = i + 1;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (startIndex < length)
                {
                    builder.Append(chArray, startIndex, length - startIndex);
                }
                builder.Append("$");
                return builder.ToString();
            }

            private void CheckProhibitedFlag(XmlSchemaFacet facet, RestrictionFlags flag, string errorCode)
            {
                if ((this.validRestrictionFlags & flag) == 0)
                {
                    throw new XmlSchemaException(errorCode, this.datatype.TypeCodeString, facet);
                }
            }

            private void CheckDupFlag(XmlSchemaFacet facet, RestrictionFlags flag, string errorCode)
            {
                if ((this.derivedRestriction.Flags & flag) != 0)
                {
                    throw new XmlSchemaException(errorCode, facet);
                }
            }

            private void SetFlag(XmlSchemaFacet facet, RestrictionFlags flag)
            {
                this.derivedRestriction.Flags |= flag;
                if (facet.IsFixed)
                {
                    this.derivedRestriction.FixedFlags |= flag;
                }
            }

            private void SetFlag(RestrictionFlags flag)
            {
                this.derivedRestriction.Flags |= flag;
                if ((this.baseFixedFlags & flag) != 0)
                {
                    this.derivedRestriction.FixedFlags |= flag;
                }
            }

            static FacetsCompiler()
            {
                c_map = new Map[] { new Map('c', @"\p{_xmlC}"), new Map('C', @"\P{_xmlC}"), new Map('d', @"\p{_xmlD}"), new Map('D', @"\P{_xmlD}"), new Map('i', @"\p{_xmlI}"), new Map('I', @"\P{_xmlI}"), new Map('w', @"\p{_xmlW}"), new Map('W', @"\P{_xmlW}") };
            }
            [StructLayout(LayoutKind.Sequential)]
            private struct Map
            {
                internal char match;
                internal string replacement;
                internal Map(char m, string r)
                {
                    this.match = m;
                    this.replacement = r;
                }
            }
        }
    }
}

