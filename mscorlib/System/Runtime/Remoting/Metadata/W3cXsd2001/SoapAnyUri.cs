namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class SoapAnyUri : ISoapXsd
    {
        private string _value;

        public SoapAnyUri()
        {
        }

        public SoapAnyUri(string value)
        {
            this._value = value;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapAnyUri Parse(string value)
        {
            return new SoapAnyUri(value);
        }

        public override string ToString()
        {
            return this._value;
        }

        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }

        public static string XsdType
        {
            get
            {
                return "anyURI";
            }
        }
    }
}

