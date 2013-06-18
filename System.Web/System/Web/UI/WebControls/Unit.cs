namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Util;

    [Serializable, StructLayout(LayoutKind.Sequential), TypeConverter(typeof(UnitConverter))]
    public struct Unit
    {
        internal const int MaxValue = 0x7fff;
        internal const int MinValue = -32768;
        public static readonly Unit Empty;
        private readonly UnitType type;
        private readonly double value;
        public Unit(int value)
        {
            if ((value < -32768) || (value > 0x7fff))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            this.value = value;
            this.type = UnitType.Pixel;
        }

        public Unit(double value)
        {
            if ((value < -32768.0) || (value > 32767.0))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            this.value = (int) value;
            this.type = UnitType.Pixel;
        }

        public Unit(double value, UnitType type)
        {
            if ((value < -32768.0) || (value > 32767.0))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (type == UnitType.Pixel)
            {
                this.value = (int) value;
            }
            else
            {
                this.value = value;
            }
            this.type = type;
        }

        public Unit(string value) : this(value, CultureInfo.CurrentCulture, UnitType.Pixel)
        {
        }

        public Unit(string value, CultureInfo culture) : this(value, culture, UnitType.Pixel)
        {
        }

        internal Unit(string value, CultureInfo culture, UnitType defaultType)
        {
            if (string.IsNullOrEmpty(value))
            {
                this.value = 0.0;
                this.type = (UnitType) 0;
            }
            else
            {
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                string str = value.Trim().ToLower(CultureInfo.InvariantCulture);
                int length = str.Length;
                int num2 = -1;
                for (int i = 0; i < length; i++)
                {
                    char ch = str[i];
                    if (((ch < '0') || (ch > '9')) && (((ch != '-') && (ch != '.')) && (ch != ',')))
                    {
                        break;
                    }
                    num2 = i;
                }
                if (num2 == -1)
                {
                    throw new FormatException(System.Web.SR.GetString("UnitParseNoDigits", new object[] { value }));
                }
                if (num2 < (length - 1))
                {
                    this.type = GetTypeFromString(str.Substring(num2 + 1).Trim());
                }
                else
                {
                    this.type = defaultType;
                }
                string text = str.Substring(0, num2 + 1);
                try
                {
                    TypeConverter converter = new SingleConverter();
                    this.value = (float) converter.ConvertFromString(null, culture, text);
                    if (this.type == UnitType.Pixel)
                    {
                        this.value = (int) this.value;
                    }
                }
                catch
                {
                    throw new FormatException(System.Web.SR.GetString("UnitParseNumericPart", new object[] { value, text, this.type.ToString("G") }));
                }
                if ((this.value < -32768.0) || (this.value > 32767.0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.type == ((UnitType) 0));
            }
        }
        public UnitType Type
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return this.type;
                }
                return UnitType.Pixel;
            }
        }
        public double Value
        {
            get
            {
                return this.value;
            }
        }
        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this.type.GetHashCode(), this.value.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is Unit))
            {
                return false;
            }
            Unit unit = (Unit) obj;
            return ((unit.type == this.type) && (unit.value == this.value));
        }

        public static bool operator ==(Unit left, Unit right)
        {
            return ((left.type == right.type) && (left.value == right.value));
        }

        public static bool operator !=(Unit left, Unit right)
        {
            if (left.type == right.type)
            {
                return !(left.value == right.value);
            }
            return true;
        }

        private static string GetStringFromType(UnitType type)
        {
            switch (type)
            {
                case UnitType.Pixel:
                    return "px";

                case UnitType.Point:
                    return "pt";

                case UnitType.Pica:
                    return "pc";

                case UnitType.Inch:
                    return "in";

                case UnitType.Mm:
                    return "mm";

                case UnitType.Cm:
                    return "cm";

                case UnitType.Percentage:
                    return "%";

                case UnitType.Em:
                    return "em";

                case UnitType.Ex:
                    return "ex";
            }
            return string.Empty;
        }

        private static UnitType GetTypeFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return UnitType.Pixel;
            }
            if (value.Equals("px"))
            {
                return UnitType.Pixel;
            }
            if (value.Equals("pt"))
            {
                return UnitType.Point;
            }
            if (value.Equals("%"))
            {
                return UnitType.Percentage;
            }
            if (value.Equals("pc"))
            {
                return UnitType.Pica;
            }
            if (value.Equals("in"))
            {
                return UnitType.Inch;
            }
            if (value.Equals("mm"))
            {
                return UnitType.Mm;
            }
            if (value.Equals("cm"))
            {
                return UnitType.Cm;
            }
            if (value.Equals("em"))
            {
                return UnitType.Em;
            }
            if (!value.Equals("ex"))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            return UnitType.Ex;
        }

        public static Unit Parse(string s)
        {
            return new Unit(s, CultureInfo.CurrentCulture);
        }

        public static Unit Parse(string s, CultureInfo culture)
        {
            return new Unit(s, culture);
        }

        public static Unit Percentage(double n)
        {
            return new Unit(n, UnitType.Percentage);
        }

        public static Unit Pixel(int n)
        {
            return new Unit(n);
        }

        public static Unit Point(int n)
        {
            return new Unit((double) n, UnitType.Point);
        }

        public override string ToString()
        {
            return this.ToString((IFormatProvider) CultureInfo.CurrentCulture);
        }

        public string ToString(CultureInfo culture)
        {
            return this.ToString((IFormatProvider) culture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            string str;
            if (this.IsEmpty)
            {
                return string.Empty;
            }
            if (this.type == UnitType.Pixel)
            {
                str = ((int) this.value).ToString(formatProvider);
            }
            else
            {
                str = ((float) this.value).ToString(formatProvider);
            }
            return (str + GetStringFromType(this.type));
        }

        public static implicit operator Unit(int n)
        {
            return Pixel(n);
        }

        static Unit()
        {
            Empty = new Unit();
        }
    }
}

