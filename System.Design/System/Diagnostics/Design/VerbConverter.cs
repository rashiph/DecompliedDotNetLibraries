namespace System.Diagnostics.Design
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    internal class VerbConverter : TypeConverter
    {
        private const string DefaultVerb = "VerbEditorDefault";

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return ((string) value).Trim();
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ProcessStartInfo info = (context == null) ? null : (context.Instance as ProcessStartInfo);
            if (info != null)
            {
                return new TypeConverter.StandardValuesCollection(info.Verbs);
            }
            return null;
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

