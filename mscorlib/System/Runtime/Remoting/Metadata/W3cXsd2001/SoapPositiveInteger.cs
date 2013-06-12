namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    [Serializable, ComVisible(true)]
    public sealed class SoapPositiveInteger : ISoapXsd
    {
        private decimal _value;

        public SoapPositiveInteger()
        {
        }

        public SoapPositiveInteger(decimal value)
        {
            this._value = decimal.Truncate(value);
            if (this._value < 1M)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:positiveInteger", value }));
            }
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapPositiveInteger Parse(string value)
        {
            return new SoapPositiveInteger(decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return this._value.ToString(CultureInfo.InvariantCulture);
        }

        public decimal Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = decimal.Truncate(value);
                if (this._value < 1M)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:positiveInteger", value }));
                }
            }
        }

        public static string XsdType
        {
            get
            {
                return "positiveInteger";
            }
        }
    }
}

