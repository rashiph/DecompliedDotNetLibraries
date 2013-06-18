namespace System
{
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Text;

    [DebuggerDisplay("Segment={originalSegment} Nature={nature}")]
    internal abstract class UriTemplatePathSegment
    {
        private readonly bool endsWithSlash;
        private readonly UriTemplatePartType nature;
        private readonly string originalSegment;

        protected UriTemplatePathSegment(string originalSegment, UriTemplatePartType nature, bool endsWithSlash)
        {
            this.originalSegment = originalSegment;
            this.nature = nature;
            this.endsWithSlash = endsWithSlash;
        }

        public abstract void Bind(string[] values, ref int valueIndex, StringBuilder path);
        public static UriTemplatePathSegment CreateFromUriTemplate(string segment, UriTemplate template)
        {
            switch (UriTemplateHelpers.IdentifyPartType(segment))
            {
                case UriTemplatePartType.Literal:
                    return UriTemplateLiteralPathSegment.CreateFromUriTemplate(segment, template);

                case UriTemplatePartType.Compound:
                    return UriTemplateCompoundPathSegment.CreateFromUriTemplate(segment, template);

                case UriTemplatePartType.Variable:
                    if (!segment.EndsWith("/", StringComparison.Ordinal))
                    {
                        return new UriTemplateVariablePathSegment(segment, false, template.AddPathVariable(UriTemplatePartType.Variable, segment.Substring(1, segment.Length - 2)));
                    }
                    return new UriTemplateVariablePathSegment(segment, true, template.AddPathVariable(UriTemplatePartType.Variable, segment.Substring(1, segment.Length - 3)));
            }
            return null;
        }

        public abstract bool IsEquivalentTo(UriTemplatePathSegment other, bool ignoreTrailingSlash);
        public bool IsMatch(UriTemplateLiteralPathSegment segment)
        {
            return this.IsMatch(segment, false);
        }

        public abstract bool IsMatch(UriTemplateLiteralPathSegment segment, bool ignoreTrailingSlash);
        public abstract void Lookup(string segment, NameValueCollection boundParameters);

        public bool EndsWithSlash
        {
            get
            {
                return this.endsWithSlash;
            }
        }

        public UriTemplatePartType Nature
        {
            get
            {
                return this.nature;
            }
        }

        public string OriginalSegment
        {
            get
            {
                return this.originalSegment;
            }
        }
    }
}

