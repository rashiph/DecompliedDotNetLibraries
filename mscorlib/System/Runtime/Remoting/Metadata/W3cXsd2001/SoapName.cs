namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class SoapName : ISoapXsd
    {
        private string _value;

        public SoapName()
        {
        }

        public SoapName(string value)
        {
            this._value = value;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapName Parse(string value)
        {
            return new SoapName(value);
        }

        public override string ToString()
        {
            return SoapType.Escape(this._value);
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
                return "Name";
            }
        }
    }
}

