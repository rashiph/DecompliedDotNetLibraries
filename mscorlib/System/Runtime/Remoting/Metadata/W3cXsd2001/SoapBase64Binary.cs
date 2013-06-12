namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    [Serializable, ComVisible(true)]
    public sealed class SoapBase64Binary : ISoapXsd
    {
        private byte[] _value;

        public SoapBase64Binary()
        {
        }

        public SoapBase64Binary(byte[] value)
        {
            this._value = value;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapBase64Binary Parse(string value)
        {
            byte[] buffer;
            if ((value == null) || (value.Length == 0))
            {
                return new SoapBase64Binary(new byte[0]);
            }
            try
            {
                buffer = Convert.FromBase64String(SoapType.FilterBin64(value));
            }
            catch (Exception)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "base64Binary", value }));
            }
            return new SoapBase64Binary(buffer);
        }

        public override string ToString()
        {
            if (this._value == null)
            {
                return null;
            }
            return SoapType.LineFeedsBin64(Convert.ToBase64String(this._value));
        }

        public byte[] Value
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
                return "base64Binary";
            }
        }
    }
}

