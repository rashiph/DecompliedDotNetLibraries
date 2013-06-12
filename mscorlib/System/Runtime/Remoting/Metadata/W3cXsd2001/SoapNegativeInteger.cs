namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    [Serializable, ComVisible(true)]
    public sealed class SoapNegativeInteger : ISoapXsd
    {
        private decimal _value;

        public SoapNegativeInteger()
        {
        }

        public SoapNegativeInteger(decimal value)
        {
            this._value = decimal.Truncate(value);
            if (value > -1M)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:negativeInteger", value }));
            }
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapNegativeInteger Parse(string value)
        {
            return new SoapNegativeInteger(decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
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
                if (this._value > -1M)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:negativeInteger", value }));
                }
            }
        }

        public static string XsdType
        {
            get
            {
                return "negativeInteger";
            }
        }
    }
}

