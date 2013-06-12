namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class SoapDate : ISoapXsd
    {
        private int _sign;
        private DateTime _value;
        private static string[] formats = new string[] { "yyyy-MM-dd", "'+'yyyy-MM-dd", "'-'yyyy-MM-dd", "yyyy-MM-ddzzz", "'+'yyyy-MM-ddzzz", "'-'yyyy-MM-ddzzz" };

        public SoapDate()
        {
            this._value = DateTime.MinValue.Date;
        }

        public SoapDate(DateTime value)
        {
            this._value = DateTime.MinValue.Date;
            this._value = value;
        }

        public SoapDate(DateTime value, int sign)
        {
            this._value = DateTime.MinValue.Date;
            this._value = value;
            this._sign = sign;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapDate Parse(string value)
        {
            int sign = 0;
            if (value[0] == '-')
            {
                sign = -1;
            }
            return new SoapDate(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None), sign);
        }

        public override string ToString()
        {
            if (this._sign < 0)
            {
                return this._value.ToString("'-'yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            return this._value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public int Sign
        {
            get
            {
                return this._sign;
            }
            set
            {
                this._sign = value;
            }
        }

        public DateTime Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value.Date;
            }
        }

        public static string XsdType
        {
            get
            {
                return "date";
            }
        }
    }
}

