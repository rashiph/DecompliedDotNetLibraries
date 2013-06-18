namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ExpressionsCollectionConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return string.Empty;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

