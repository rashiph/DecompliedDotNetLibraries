namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Text;

    [Serializable, ComVisible(true)]
    public sealed class SoapHexBinary : ISoapXsd
    {
        private byte[] _value;
        private StringBuilder sb;

        public SoapHexBinary()
        {
            this.sb = new StringBuilder(100);
        }

        public SoapHexBinary(byte[] value)
        {
            this.sb = new StringBuilder(100);
            this._value = value;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapHexBinary Parse(string value)
        {
            return new SoapHexBinary(ToByteArray(SoapType.FilterBin64(value)));
        }

        private static byte ToByte(char c, string value)
        {
            byte num = 0;
            string str = c.ToString();
            try
            {
                num = byte.Parse(c.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid", new object[] { "xsd:hexBinary", value }));
            }
            return num;
        }

        private static byte[] ToByteArray(string value)
        {
            char[] chArray = value.ToCharArray();
            if ((chArray.Length % 2) != 0)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:hexBinary", value }));
            }
            byte[] buffer = new byte[chArray.Length / 2];
            for (int i = 0; i < (chArray.Length / 2); i++)
            {
                buffer[i] = (byte) ((ToByte(chArray[i * 2], value) * 0x10) + ToByte(chArray[(i * 2) + 1], value));
            }
            return buffer;
        }

        public override string ToString()
        {
            this.sb.Length = 0;
            for (int i = 0; i < this._value.Length; i++)
            {
                string str = this._value[i].ToString("X", CultureInfo.InvariantCulture);
                if (str.Length == 1)
                {
                    this.sb.Append('0');
                }
                this.sb.Append(str);
            }
            return this.sb.ToString();
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
                return "hexBinary";
            }
        }
    }
}

