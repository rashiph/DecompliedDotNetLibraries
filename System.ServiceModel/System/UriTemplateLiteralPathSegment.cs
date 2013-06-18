namespace System
{
    using System.Collections.Specialized;
    using System.ServiceModel;
    using System.Text;

    internal class UriTemplateLiteralPathSegment : UriTemplatePathSegment, IComparable<UriTemplateLiteralPathSegment>
    {
        private static Uri dummyUri = new Uri("http://localhost");
        private readonly string segment;

        private UriTemplateLiteralPathSegment(string segment) : base(segment, UriTemplatePartType.Literal, segment.EndsWith("/", StringComparison.Ordinal))
        {
            if (base.EndsWithSlash)
            {
                this.segment = segment.Remove(segment.Length - 1);
            }
            else
            {
                this.segment = segment;
            }
        }

        public string AsUnescapedString()
        {
            return Uri.UnescapeDataString(this.segment);
        }

        public override void Bind(string[] values, ref int valueIndex, StringBuilder path)
        {
            if (base.EndsWithSlash)
            {
                path.AppendFormat("{0}/", this.AsUnescapedString());
            }
            else
            {
                path.Append(this.AsUnescapedString());
            }
        }

        public int CompareTo(UriTemplateLiteralPathSegment other)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(this.segment, other.segment);
        }

        public static UriTemplateLiteralPathSegment CreateFromUriTemplate(string segment, UriTemplate template)
        {
            if (string.Compare(segment, "/", StringComparison.Ordinal) == 0)
            {
                return new UriTemplateLiteralPathSegment("/");
            }
            if (segment.IndexOf("*", StringComparison.Ordinal) != -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidWildcardInVariableOrLiteral", new object[] { template.originalTemplate, "*" })));
            }
            segment = segment.Replace("%2a", "*").Replace("%2A", "*");
            UriBuilder builder = new UriBuilder(dummyUri) {
                Path = segment
            };
            string str = builder.Uri.AbsolutePath.Substring(1);
            if (str == string.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("segment", System.ServiceModel.SR.GetString("UTInvalidFormatSegmentOrQueryPart", new object[] { segment }));
            }
            return new UriTemplateLiteralPathSegment(str);
        }

        public static UriTemplateLiteralPathSegment CreateFromWireData(string segment)
        {
            return new UriTemplateLiteralPathSegment(segment);
        }

        public override bool Equals(object obj)
        {
            UriTemplateLiteralPathSegment segment = obj as UriTemplateLiteralPathSegment;
            if (segment == null)
            {
                return false;
            }
            return ((base.EndsWithSlash == segment.EndsWithSlash) && StringComparer.OrdinalIgnoreCase.Equals(this.segment, segment.segment));
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(this.segment);
        }

        public override bool IsEquivalentTo(UriTemplatePathSegment other, bool ignoreTrailingSlash)
        {
            if (other == null)
            {
                return false;
            }
            if (other.Nature != UriTemplatePartType.Literal)
            {
                return false;
            }
            UriTemplateLiteralPathSegment segment = other as UriTemplateLiteralPathSegment;
            return this.IsMatch(segment, ignoreTrailingSlash);
        }

        public override bool IsMatch(UriTemplateLiteralPathSegment segment, bool ignoreTrailingSlash)
        {
            if (!ignoreTrailingSlash && (segment.EndsWithSlash != base.EndsWithSlash))
            {
                return false;
            }
            return (this.CompareTo(segment) == 0);
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(this.segment);
        }

        public override void Lookup(string segment, NameValueCollection boundParameters)
        {
        }
    }
}

