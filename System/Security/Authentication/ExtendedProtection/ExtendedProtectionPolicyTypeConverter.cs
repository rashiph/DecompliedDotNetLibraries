namespace System.Security.Authentication.ExtendedProtection
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    public class ExtendedProtectionPolicyTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                ExtendedProtectionPolicy policy = value as ExtendedProtectionPolicy;
                if (policy != null)
                {
                    Type[] typeArray;
                    object[] objArray;
                    if (policy.PolicyEnforcement == PolicyEnforcement.Never)
                    {
                        typeArray = new Type[] { typeof(PolicyEnforcement) };
                        objArray = new object[] { PolicyEnforcement.Never };
                    }
                    else
                    {
                        typeArray = new Type[] { typeof(PolicyEnforcement), typeof(ProtectionScenario), typeof(ICollection) };
                        object[] array = null;
                        if ((policy.CustomServiceNames != null) && (policy.CustomServiceNames.Count > 0))
                        {
                            array = new object[policy.CustomServiceNames.Count];
                            ((ICollection) policy.CustomServiceNames).CopyTo(array, 0);
                        }
                        objArray = new object[] { policy.PolicyEnforcement, policy.ProtectionScenario, array };
                    }
                    return new InstanceDescriptor(typeof(ExtendedProtectionPolicy).GetConstructor(typeArray), objArray);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

