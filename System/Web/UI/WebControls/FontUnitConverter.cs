namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class FontUnitConverter : TypeConverter
    {
        private TypeConverter.StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (!(destinationType == typeof(string)) && !(destinationType == typeof(InstanceDescriptor)))
            {
                return base.CanConvertTo(context, destinationType);
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return null;
            }
            string str = value as string;
            if (str == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            string s = str.Trim();
            if (s.Length == 0)
            {
                return FontUnit.Empty;
            }
            return FontUnit.Parse(s, culture);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            MemberInfo field;
            object[] objArray;
            string str;
            if (!(destinationType == typeof(string)))
            {
                if (!(destinationType == typeof(InstanceDescriptor)) || (value == null))
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }
                FontUnit unit = (FontUnit) value;
                field = null;
                objArray = null;
                if (unit.IsEmpty)
                {
                    field = typeof(FontUnit).GetField("Empty");
                    goto Label_016E;
                }
                if (unit.Type == FontSize.AsUnit)
                {
                    field = typeof(FontUnit).GetConstructor(new Type[] { typeof(Unit) });
                    objArray = new object[] { unit.Unit };
                    goto Label_016E;
                }
                str = null;
                switch (unit.Type)
                {
                    case FontSize.Smaller:
                        str = "Smaller";
                        break;

                    case FontSize.Larger:
                        str = "Larger";
                        break;

                    case FontSize.XXSmall:
                        str = "XXSmall";
                        break;

                    case FontSize.XSmall:
                        str = "XSmall";
                        break;

                    case FontSize.Small:
                        str = "Small";
                        break;

                    case FontSize.Medium:
                        str = "Medium";
                        break;

                    case FontSize.Large:
                        str = "Large";
                        break;

                    case FontSize.XLarge:
                        str = "XLarge";
                        break;

                    case FontSize.XXLarge:
                        str = "XXLarge";
                        break;
                }
            }
            else
            {
                if (value != null)
                {
                    FontUnit unit2 = (FontUnit) value;
                    if (unit2.Type != FontSize.NotSet)
                    {
                        FontUnit unit3 = (FontUnit) value;
                        return unit3.ToString(culture);
                    }
                }
                return string.Empty;
            }
            if (str != null)
            {
                field = typeof(FontUnit).GetField(str);
            }
        Label_016E:
            if (field != null)
            {
                return new InstanceDescriptor(field, objArray);
            }
            return null;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                object[] values = new object[] { FontUnit.Smaller, FontUnit.Larger, FontUnit.XXSmall, FontUnit.XSmall, FontUnit.Small, FontUnit.Medium, FontUnit.Large, FontUnit.XLarge, FontUnit.XXLarge };
                this.values = new TypeConverter.StandardValuesCollection(values);
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

