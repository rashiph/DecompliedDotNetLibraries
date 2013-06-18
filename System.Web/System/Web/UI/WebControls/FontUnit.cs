namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web.UI;
    using System.Web.Util;

    [Serializable, StructLayout(LayoutKind.Sequential), TypeConverter(typeof(FontUnitConverter))]
    public struct FontUnit
    {
        public static readonly FontUnit Empty;
        public static readonly FontUnit Smaller;
        public static readonly FontUnit Larger;
        public static readonly FontUnit XXSmall;
        public static readonly FontUnit XSmall;
        public static readonly FontUnit Small;
        public static readonly FontUnit Medium;
        public static readonly FontUnit Large;
        public static readonly FontUnit XLarge;
        public static readonly FontUnit XXLarge;
        private readonly FontSize type;
        private readonly System.Web.UI.WebControls.Unit value;
        public FontUnit(FontSize type)
        {
            if ((type < FontSize.NotSet) || (type > FontSize.XXLarge))
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.type = type;
            if (this.type == FontSize.AsUnit)
            {
                this.value = System.Web.UI.WebControls.Unit.Point(10);
            }
            else
            {
                this.value = System.Web.UI.WebControls.Unit.Empty;
            }
        }

        public FontUnit(System.Web.UI.WebControls.Unit value)
        {
            this.type = FontSize.NotSet;
            if (!value.IsEmpty)
            {
                this.type = FontSize.AsUnit;
                this.value = value;
            }
            else
            {
                this.value = System.Web.UI.WebControls.Unit.Empty;
            }
        }

        public FontUnit(int value)
        {
            this.type = FontSize.AsUnit;
            this.value = System.Web.UI.WebControls.Unit.Point(value);
        }

        public FontUnit(double value) : this(new System.Web.UI.WebControls.Unit(value, UnitType.Point))
        {
        }

        public FontUnit(double value, UnitType type) : this(new System.Web.UI.WebControls.Unit(value, type))
        {
        }

        public FontUnit(string value) : this(value, CultureInfo.CurrentCulture)
        {
        }

        public FontUnit(string value, CultureInfo culture)
        {
            this.type = FontSize.NotSet;
            this.value = System.Web.UI.WebControls.Unit.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                char ch = char.ToLower(value[0], CultureInfo.InvariantCulture);
                switch (ch)
                {
                    case 's':
                        if (string.Equals(value, "small", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.Small;
                            return;
                        }
                        if (string.Equals(value, "smaller", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.Smaller;
                            return;
                        }
                        break;

                    case 'l':
                        if (string.Equals(value, "large", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.Large;
                            return;
                        }
                        if (string.Equals(value, "larger", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.Larger;
                            return;
                        }
                        break;

                    case 'x':
                        if (string.Equals(value, "xx-small", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "xxsmall", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.XXSmall;
                            return;
                        }
                        if (string.Equals(value, "x-small", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "xsmall", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.XSmall;
                            return;
                        }
                        if (string.Equals(value, "x-large", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "xlarge", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.XLarge;
                            return;
                        }
                        if (string.Equals(value, "xx-large", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "xxlarge", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.XXLarge;
                            return;
                        }
                        break;

                    default:
                        if ((ch == 'm') && string.Equals(value, "medium", StringComparison.OrdinalIgnoreCase))
                        {
                            this.type = FontSize.Medium;
                            return;
                        }
                        break;
                }
                this.value = new System.Web.UI.WebControls.Unit(value, culture, UnitType.Point);
                this.type = FontSize.AsUnit;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.type == FontSize.NotSet);
            }
        }
        public FontSize Type
        {
            get
            {
                return this.type;
            }
        }
        public System.Web.UI.WebControls.Unit Unit
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
            if ((obj == null) || !(obj is FontUnit))
            {
                return false;
            }
            FontUnit unit = (FontUnit) obj;
            return ((unit.type == this.type) && (unit.value == this.value));
        }

        public static bool operator ==(FontUnit left, FontUnit right)
        {
            return ((left.type == right.type) && (left.value == right.value));
        }

        public static bool operator !=(FontUnit left, FontUnit right)
        {
            if (left.type == right.type)
            {
                return (left.value != right.value);
            }
            return true;
        }

        public static FontUnit Parse(string s)
        {
            return new FontUnit(s, CultureInfo.InvariantCulture);
        }

        public static FontUnit Parse(string s, CultureInfo culture)
        {
            return new FontUnit(s, culture);
        }

        public static FontUnit Point(int n)
        {
            return new FontUnit(n);
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
            string str = string.Empty;
            if (this.IsEmpty)
            {
                return str;
            }
            switch (this.type)
            {
                case FontSize.AsUnit:
                    return this.value.ToString(formatProvider);

                case FontSize.XXSmall:
                    return "XX-Small";

                case FontSize.XSmall:
                    return "X-Small";

                case FontSize.XLarge:
                    return "X-Large";

                case FontSize.XXLarge:
                    return "XX-Large";
            }
            return PropertyConverter.EnumToString(typeof(FontSize), this.type);
        }

        public static implicit operator FontUnit(int n)
        {
            return Point(n);
        }

        static FontUnit()
        {
            Empty = new FontUnit();
            Smaller = new FontUnit(FontSize.Smaller);
            Larger = new FontUnit(FontSize.Larger);
            XXSmall = new FontUnit(FontSize.XXSmall);
            XSmall = new FontUnit(FontSize.XSmall);
            Small = new FontUnit(FontSize.Small);
            Medium = new FontUnit(FontSize.Medium);
            Large = new FontUnit(FontSize.Large);
            XLarge = new FontUnit(FontSize.XLarge);
            XXLarge = new FontUnit(FontSize.XXLarge);
        }
    }
}

