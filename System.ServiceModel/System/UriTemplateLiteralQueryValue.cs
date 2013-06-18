namespace System
{
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Text;

    internal class UriTemplateLiteralQueryValue : UriTemplateQueryValue, IComparable<UriTemplateLiteralQueryValue>
    {
        private readonly string value;

        private UriTemplateLiteralQueryValue(string value) : base(UriTemplatePartType.Literal)
        {
            this.value = value;
        }

        public string AsEscapedString()
        {
            return UrlUtility.UrlEncode(this.value, Encoding.UTF8);
        }

        public string AsRawUnescapedString()
        {
            return this.value;
        }

        public override void Bind(string keyName, string[] values, ref int valueIndex, StringBuilder query)
        {
            query.AppendFormat("&{0}={1}", UrlUtility.UrlEncode(keyName, Encoding.UTF8), this.AsEscapedString());
        }

        public int CompareTo(UriTemplateLiteralQueryValue other)
        {
            return string.Compare(this.value, other.value, StringComparison.Ordinal);
        }

        public static UriTemplateLiteralQueryValue CreateFromUriTemplate(string value)
        {
            return new UriTemplateLiteralQueryValue(UrlUtility.UrlDecode(value, Encoding.UTF8));
        }

        public override bool Equals(object obj)
        {
            UriTemplateLiteralQueryValue value2 = obj as UriTemplateLiteralQueryValue;
            if (value2 == null)
            {
                return false;
            }
            return (this.value == value2.value);
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override bool IsEquivalentTo(UriTemplateQueryValue other)
        {
            if (other == null)
            {
                return false;
            }
            if (other.Nature != UriTemplatePartType.Literal)
            {
                return false;
            }
            UriTemplateLiteralQueryValue value2 = other as UriTemplateLiteralQueryValue;
            return (this.CompareTo(value2) == 0);
        }

        public override void Lookup(string value, NameValueCollection boundParameters)
        {
        }
    }
}

