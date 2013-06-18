namespace System
{
    using System.Collections.Specialized;
    using System.Text;

    internal class UriTemplateVariablePathSegment : UriTemplatePathSegment
    {
        private readonly string varName;

        public UriTemplateVariablePathSegment(string originalSegment, bool endsWithSlash, string varName) : base(originalSegment, UriTemplatePartType.Variable, endsWithSlash)
        {
            this.varName = varName;
        }

        public override void Bind(string[] values, ref int valueIndex, StringBuilder path)
        {
            if (base.EndsWithSlash)
            {
                path.AppendFormat("{0}/", values[valueIndex++]);
            }
            else
            {
                path.Append(values[valueIndex++]);
            }
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
            return (other.Nature == UriTemplatePartType.Variable);
        }

        public override bool IsMatch(UriTemplateLiteralPathSegment segment, bool ignoreTrailingSlash)
        {
            if (!ignoreTrailingSlash && (base.EndsWithSlash != segment.EndsWithSlash))
            {
                return false;
            }
            return !segment.IsNullOrEmpty();
        }

        public override void Lookup(string segment, NameValueCollection boundParameters)
        {
            boundParameters.Add(this.varName, segment);
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

