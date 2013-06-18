namespace System
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Text;

    internal class UriTemplateCompoundPathSegment : UriTemplatePathSegment, IComparable<UriTemplateCompoundPathSegment>
    {
        private CompoundSegmentClass csClass;
        private readonly string firstLiteral;
        private readonly List<VarAndLitPair> varLitPairs;

        private UriTemplateCompoundPathSegment(string originalSegment, bool endsWithSlash, string firstLiteral) : base(originalSegment, UriTemplatePartType.Compound, endsWithSlash)
        {
            this.firstLiteral = firstLiteral;
            this.varLitPairs = new List<VarAndLitPair>();
        }

        public override void Bind(string[] values, ref int valueIndex, StringBuilder path)
        {
            path.Append(this.firstLiteral);
            for (int i = 0; i < this.varLitPairs.Count; i++)
            {
                path.Append(values[valueIndex++]);
                VarAndLitPair pair = this.varLitPairs[i];
                path.Append(pair.Literal);
            }
            if (base.EndsWithSlash)
            {
                path.Append("/");
            }
        }

        private int ComparePrefixToOtherPrefix(UriTemplateCompoundPathSegment other)
        {
            return string.Compare(other.firstLiteral, this.firstLiteral, StringComparison.OrdinalIgnoreCase);
        }

        private int CompareSuffixToOtherSuffix(UriTemplateCompoundPathSegment other)
        {
            VarAndLitPair pair = this.varLitPairs[this.varLitPairs.Count - 1];
            string strB = ReverseString(pair.Literal);
            VarAndLitPair pair2 = other.varLitPairs[other.varLitPairs.Count - 1];
            return string.Compare(ReverseString(pair2.Literal), strB, StringComparison.OrdinalIgnoreCase);
        }

        private int CompareToOtherThatHasNoPrefixNorSuffix(UriTemplateCompoundPathSegment other)
        {
            return (other.varLitPairs.Count - this.varLitPairs.Count);
        }

        private int CompareToOtherThatHasOnlyPrefix(UriTemplateCompoundPathSegment other)
        {
            int num = this.ComparePrefixToOtherPrefix(other);
            if (num == 0)
            {
                return (other.varLitPairs.Count - this.varLitPairs.Count);
            }
            return num;
        }

        private int CompareToOtherThatHasOnlySuffix(UriTemplateCompoundPathSegment other)
        {
            int num = this.CompareSuffixToOtherSuffix(other);
            if (num == 0)
            {
                return (other.varLitPairs.Count - this.varLitPairs.Count);
            }
            return num;
        }

        private int CompareToOtherThatHasPrefixAndSuffix(UriTemplateCompoundPathSegment other)
        {
            int num = this.ComparePrefixToOtherPrefix(other);
            if (num != 0)
            {
                return num;
            }
            int num2 = this.CompareSuffixToOtherSuffix(other);
            if (num2 == 0)
            {
                return (other.varLitPairs.Count - this.varLitPairs.Count);
            }
            return num2;
        }

        public static UriTemplateCompoundPathSegment CreateFromUriTemplate(string segment, UriTemplate template)
        {
            string originalSegment = segment;
            bool endsWithSlash = segment.EndsWith("/", StringComparison.Ordinal);
            if (endsWithSlash)
            {
                segment = segment.Remove(segment.Length - 1);
            }
            int index = segment.IndexOf("{", StringComparison.Ordinal);
            string stringToUnescape = (index > 0) ? segment.Substring(0, index) : string.Empty;
            if (stringToUnescape.IndexOf("*", StringComparison.Ordinal) != -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidWildcardInVariableOrLiteral", new object[] { template.originalTemplate, "*" })));
            }
            UriTemplateCompoundPathSegment segment2 = new UriTemplateCompoundPathSegment(originalSegment, endsWithSlash, (stringToUnescape != string.Empty) ? Uri.UnescapeDataString(stringToUnescape) : string.Empty);
            do
            {
                bool flag2;
                string str4;
                int num2 = segment.IndexOf("}", index + 1, StringComparison.Ordinal);
                if (num2 < (index + 2))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidFormatSegmentOrQueryPart", new object[] { segment })));
                }
                string varName = template.AddPathVariable(UriTemplatePartType.Compound, segment.Substring(index + 1, (num2 - index) - 1), out flag2);
                if (flag2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTDefaultValueToCompoundSegmentVar", new object[] { template, originalSegment, varName })));
                }
                index = segment.IndexOf("{", num2 + 1, StringComparison.Ordinal);
                if (index > 0)
                {
                    if (index == (num2 + 1))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("template", System.ServiceModel.SR.GetString("UTDoesNotSupportAdjacentVarsInCompoundSegment", new object[] { template, segment }));
                    }
                    str4 = segment.Substring(num2 + 1, (index - num2) - 1);
                }
                else if ((num2 + 1) < segment.Length)
                {
                    str4 = segment.Substring(num2 + 1);
                }
                else
                {
                    str4 = string.Empty;
                }
                if (str4.IndexOf("*", StringComparison.Ordinal) != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidWildcardInVariableOrLiteral", new object[] { template.originalTemplate, "*" })));
                }
                if (str4.IndexOf('}') != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidFormatSegmentOrQueryPart", new object[] { segment })));
                }
                segment2.varLitPairs.Add(new VarAndLitPair(varName, (str4 == string.Empty) ? string.Empty : Uri.UnescapeDataString(str4)));
            }
            while (index > 0);
            if (string.IsNullOrEmpty(segment2.firstLiteral))
            {
                VarAndLitPair pair = segment2.varLitPairs[segment2.varLitPairs.Count - 1];
                if (string.IsNullOrEmpty(pair.Literal))
                {
                    segment2.csClass = CompoundSegmentClass.HasNoPrefixNorSuffix;
                    return segment2;
                }
                segment2.csClass = CompoundSegmentClass.HasOnlySuffix;
                return segment2;
            }
            VarAndLitPair pair2 = segment2.varLitPairs[segment2.varLitPairs.Count - 1];
            if (string.IsNullOrEmpty(pair2.Literal))
            {
                segment2.csClass = CompoundSegmentClass.HasOnlyPrefix;
                return segment2;
            }
            segment2.csClass = CompoundSegmentClass.HasPrefixAndSuffix;
            return segment2;
        }

        public override bool IsEquivalentTo(UriTemplatePathSegment other, bool ignoreTrailingSlash)
        {
            if (other == null)
            {
                return false;
            }
            if (!ignoreTrailingSlash && (base.EndsWithSlash != other.EndsWithSlash))
            {
                return false;
            }
            UriTemplateCompoundPathSegment segment = other as UriTemplateCompoundPathSegment;
            if (segment == null)
            {
                return false;
            }
            if (this.varLitPairs.Count != segment.varLitPairs.Count)
            {
                return false;
            }
            if (StringComparer.OrdinalIgnoreCase.Compare(this.firstLiteral, segment.firstLiteral) != 0)
            {
                return false;
            }
            for (int i = 0; i < this.varLitPairs.Count; i++)
            {
                VarAndLitPair pair = this.varLitPairs[i];
                VarAndLitPair pair2 = segment.varLitPairs[i];
                if (StringComparer.OrdinalIgnoreCase.Compare(pair.Literal, pair2.Literal) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool IsMatch(UriTemplateLiteralPathSegment segment, bool ignoreTrailingSlash)
        {
            if (!ignoreTrailingSlash && (base.EndsWithSlash != segment.EndsWithSlash))
            {
                return false;
            }
            return this.TryLookup(segment.AsUnescapedString(), null);
        }

        public override void Lookup(string segment, NameValueCollection boundParameters)
        {
            if (!this.TryLookup(segment, boundParameters))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTCSRLookupBeforeMatch")));
            }
        }

        private static string ReverseString(string stringToReverse)
        {
            char[] chArray = new char[stringToReverse.Length];
            for (int i = 0; i < stringToReverse.Length; i++)
            {
                chArray[i] = stringToReverse[(stringToReverse.Length - i) - 1];
            }
            return new string(chArray);
        }

        int IComparable<UriTemplateCompoundPathSegment>.CompareTo(UriTemplateCompoundPathSegment other)
        {
            switch (this.csClass)
            {
                case CompoundSegmentClass.HasPrefixAndSuffix:
                    switch (other.csClass)
                    {
                        case CompoundSegmentClass.HasPrefixAndSuffix:
                            return this.CompareToOtherThatHasPrefixAndSuffix(other);

                        case CompoundSegmentClass.HasOnlyPrefix:
                        case CompoundSegmentClass.HasOnlySuffix:
                        case CompoundSegmentClass.HasNoPrefixNorSuffix:
                            return -1;
                    }
                    return 0;

                case CompoundSegmentClass.HasOnlyPrefix:
                    switch (other.csClass)
                    {
                        case CompoundSegmentClass.HasPrefixAndSuffix:
                            return 1;

                        case CompoundSegmentClass.HasOnlyPrefix:
                            return this.CompareToOtherThatHasOnlyPrefix(other);

                        case CompoundSegmentClass.HasOnlySuffix:
                        case CompoundSegmentClass.HasNoPrefixNorSuffix:
                            return -1;
                    }
                    return 0;

                case CompoundSegmentClass.HasOnlySuffix:
                    switch (other.csClass)
                    {
                        case CompoundSegmentClass.HasPrefixAndSuffix:
                        case CompoundSegmentClass.HasOnlyPrefix:
                            return 1;

                        case CompoundSegmentClass.HasOnlySuffix:
                            return this.CompareToOtherThatHasOnlySuffix(other);

                        case CompoundSegmentClass.HasNoPrefixNorSuffix:
                            return -1;
                    }
                    return 0;

                case CompoundSegmentClass.HasNoPrefixNorSuffix:
                    switch (other.csClass)
                    {
                        case CompoundSegmentClass.HasPrefixAndSuffix:
                        case CompoundSegmentClass.HasOnlyPrefix:
                        case CompoundSegmentClass.HasOnlySuffix:
                            return 1;

                        case CompoundSegmentClass.HasNoPrefixNorSuffix:
                            return this.CompareToOtherThatHasNoPrefixNorSuffix(other);
                    }
                    return 0;
            }
            return 0;
        }

        private bool TryLookup(string segment, NameValueCollection boundParameters)
        {
            int startIndex = 0;
            if (!string.IsNullOrEmpty(this.firstLiteral))
            {
                if (!segment.StartsWith(this.firstLiteral, StringComparison.Ordinal))
                {
                    return false;
                }
                startIndex = this.firstLiteral.Length;
            }
            for (int i = 0; i < (this.varLitPairs.Count - 1); i++)
            {
                VarAndLitPair pair = this.varLitPairs[i];
                int num3 = segment.IndexOf(pair.Literal, startIndex, StringComparison.Ordinal);
                if (num3 < (startIndex + 1))
                {
                    return false;
                }
                if (boundParameters != null)
                {
                    string str = segment.Substring(startIndex, num3 - startIndex);
                    VarAndLitPair pair2 = this.varLitPairs[i];
                    boundParameters.Add(pair2.VarName, str);
                }
                VarAndLitPair pair3 = this.varLitPairs[i];
                startIndex = num3 + pair3.Literal.Length;
            }
            if (startIndex < segment.Length)
            {
                VarAndLitPair pair4 = this.varLitPairs[this.varLitPairs.Count - 1];
                if (string.IsNullOrEmpty(pair4.Literal))
                {
                    if (boundParameters != null)
                    {
                        VarAndLitPair pair5 = this.varLitPairs[this.varLitPairs.Count - 1];
                        boundParameters.Add(pair5.VarName, segment.Substring(startIndex));
                    }
                    return true;
                }
                VarAndLitPair pair6 = this.varLitPairs[this.varLitPairs.Count - 1];
                if ((startIndex + pair6.Literal.Length) < segment.Length)
                {
                    VarAndLitPair pair7 = this.varLitPairs[this.varLitPairs.Count - 1];
                    if (segment.EndsWith(pair7.Literal, StringComparison.Ordinal))
                    {
                        if (boundParameters != null)
                        {
                            VarAndLitPair pair8 = this.varLitPairs[this.varLitPairs.Count - 1];
                            VarAndLitPair pair9 = this.varLitPairs[this.varLitPairs.Count - 1];
                            boundParameters.Add(pair8.VarName, segment.Substring(startIndex, (segment.Length - startIndex) - pair9.Literal.Length));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private enum CompoundSegmentClass
        {
            Undefined,
            HasPrefixAndSuffix,
            HasOnlyPrefix,
            HasOnlySuffix,
            HasNoPrefixNorSuffix
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VarAndLitPair
        {
            private readonly string literal;
            private readonly string varName;
            public VarAndLitPair(string varName, string literal)
            {
                this.varName = varName;
                this.literal = literal;
            }

            public string Literal
            {
                get
                {
                    return this.literal;
                }
            }
            public string VarName
            {
                get
                {
                    return this.varName;
                }
            }
        }
    }
}

