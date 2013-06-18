namespace System.Data
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal sealed class DefaultValueTypeConverter : StringConverter
    {
        private static string dbNullString = "<DBNull>";
        private static string nullString = "<null>";

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if ((value != null) && (value.GetType() == typeof(string)))
            {
                string strA = (string) value;
                if (string.Compare(strA, nullString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return null;
                }
                if (string.Compare(strA, dbNullString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return DBNull.Value;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == typeof(string))
            {
                if (value == null)
                {
                    return nullString;
                }
                if (value == DBNull.Value)
                {
                    return dbNullString;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

