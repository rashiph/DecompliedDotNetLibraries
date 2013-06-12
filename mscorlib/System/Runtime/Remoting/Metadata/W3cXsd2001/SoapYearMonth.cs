namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class SoapYearMonth : ISoapXsd
    {
        private int _sign;
        private DateTime _value;
        private static string[] formats = new string[] { "yyyy-MM", "'+'yyyy-MM", "'-'yyyy-MM", "yyyy-MMzzz", "'+'yyyy-MMzzz", "'-'yyyy-MMzzz" };

        public SoapYearMonth()
        {
            this._value = DateTime.MinValue;
        }

        public SoapYearMonth(DateTime value)
        {
            this._value = DateTime.MinValue;
            this._value = value;
        }

        public SoapYearMonth(DateTime value, int sign)
        {
            this._value = DateTime.MinValue;
            this._value = value;
            this._sign = sign;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapYearMonth Parse(string value)
        {
            int sign = 0;
            if (value[0] == '-')
            {
                sign = -1;
            }
            return new SoapYearMonth(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None), sign);
        }

        public override string ToString()
        {
            if (this._sign < 0)
            {
                return this._value.ToString("'-'yyyy-MM", CultureInfo.InvariantCulture);
            }
            return this._value.ToString("yyyy-MM", CultureInfo.InvariantCulture);
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
                this._value = value;
            }
        }

        public static string XsdType
        {
            get
            {
                return "gYearMonth";
            }
        }
    }
}

