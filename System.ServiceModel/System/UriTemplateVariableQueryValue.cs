namespace System
{
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Text;

    internal class UriTemplateVariableQueryValue : UriTemplateQueryValue
    {
        private readonly string varName;

        public UriTemplateVariableQueryValue(string varName) : base(UriTemplatePartType.Variable)
        {
            this.varName = varName;
        }

        public override void Bind(string keyName, string[] values, ref int valueIndex, StringBuilder query)
        {
            if (values[valueIndex] == null)
            {
                valueIndex++;
            }
            else
            {
                query.AppendFormat("&{0}={1}", UrlUtility.UrlEncode(keyName, Encoding.UTF8), UrlUtility.UrlEncode(values[valueIndex++], Encoding.UTF8));
            }
        }

        public override bool IsEquivalentTo(UriTemplateQueryValue other)
        {
            if (other == null)
            {
                return false;
            }
            return (other.Nature == UriTemplatePartType.Variable);
        }

        public override void Lookup(string value, NameValueCollection boundParameters)
        {
            boundParameters.Add(this.varName, value);
        }
    }
}

