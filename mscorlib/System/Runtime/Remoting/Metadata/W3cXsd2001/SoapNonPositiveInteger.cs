namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    [Serializable, ComVisible(true)]
    public sealed class SoapNonPositiveInteger : ISoapXsd
    {
        private decimal _value;

        public SoapNonPositiveInteger()
        {
        }

        public SoapNonPositiveInteger(decimal value)
        {
            this._value = decimal.Truncate(value);
            if (this._value > 0M)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:nonPositiveInteger", value }));
            }
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapNonPositiveInteger Parse(string value)
        {
            return new SoapNonPositiveInteger(decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
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
                if (this._value > 0M)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:nonPositiveInteger", value }));
                }
            }
        }

        public static string XsdType
        {
            get
            {
                return "nonPositiveInteger";
            }
        }
    }
}

