namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Security.Principal;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class BuiltInRoleConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (((destinationType != null) && destinationType.Equals(typeof(WindowsBuiltInRole))) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType != null) && destinationType.Equals(typeof(WindowsBuiltInRole)))
            {
                User.ValidateBuiltInRoleEnumValue((BuiltInRole) value, "value");
                return this.GetWindowsBuiltInRole(value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private WindowsBuiltInRole GetWindowsBuiltInRole(object role)
        {
            WindowsBuiltInRole role2;
            string name = Enum.GetName(typeof(BuiltInRole), role);
            object obj2 = Enum.Parse(typeof(WindowsBuiltInRole), name);
            if (obj2 != null)
            {
                return (WindowsBuiltInRole) obj2;
            }
            return role2;
        }
    }
}

