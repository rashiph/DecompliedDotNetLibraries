namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class TreeViewImageKeyConverter : ImageKeyConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(string)) && (value == null))
            {
                return System.Windows.Forms.SR.GetString("toStringDefault");
            }
            string str = value as string;
            if ((str != null) && (str.Length == 0))
            {
                return System.Windows.Forms.SR.GetString("toStringDefault");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

