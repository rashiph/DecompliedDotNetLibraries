namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class SoapMonth : ISoapXsd
    {
        private DateTime _value;
        private static string[] formats = new string[] { "--MM--", "--MM--zzz" };

        public SoapMonth()
        {
            this._value = DateTime.MinValue;
        }

        public SoapMonth(DateTime value)
        {
            this._value = DateTime.MinValue;
            this._value = value;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapMonth Parse(string value)
        {
            return new SoapMonth(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None));
        }

        public override string ToString()
        {
            return this._value.ToString("--MM--", CultureInfo.InvariantCulture);
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
                return "gMonth";
            }
        }
    }
}

