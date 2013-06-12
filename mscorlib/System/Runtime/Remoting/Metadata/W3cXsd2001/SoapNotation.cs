namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class SoapNotation : ISoapXsd
    {
        private string _value;

        public SoapNotation()
        {
        }

        public SoapNotation(string value)
        {
            this._value = value;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapNotation Parse(string value)
        {
            return new SoapNotation(value);
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
                return "NOTATION";
            }
        }
    }
}

