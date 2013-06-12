namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class UnitConverter : TypeConverter
    {
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
                return Unit.Empty;
            }
            if (culture != null)
            {
                return Unit.Parse(s, culture);
            }
            return Unit.Parse(s, CultureInfo.CurrentCulture);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(destinationType == typeof(string)))
            {
                if (!(destinationType == typeof(InstanceDescriptor)) || (value == null))
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }
                Unit unit = (Unit) value;
                MemberInfo member = null;
                object[] arguments = null;
                if (unit.IsEmpty)
                {
                    member = typeof(Unit).GetField("Empty");
                }
                else
                {
                    member = typeof(Unit).GetConstructor(new Type[] { typeof(double), typeof(UnitType) });
                    arguments = new object[] { unit.Value, unit.Type };
                }
                if (member != null)
                {
                    return new InstanceDescriptor(member, arguments);
                }
                return null;
            }
            if (value != null)
            {
                Unit unit2 = (Unit) value;
                if (!unit2.IsEmpty)
                {
                    Unit unit3 = (Unit) value;
                    return unit3.ToString(culture);
                }
            }
            return string.Empty;
        }
    }
}

