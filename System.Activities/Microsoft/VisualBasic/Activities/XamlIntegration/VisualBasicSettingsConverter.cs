namespace Microsoft.VisualBasic.Activities.XamlIntegration
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;

    public sealed class VisualBasicSettingsConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == TypeHelper.StringType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == TypeHelper.StringType)
            {
                return false;
            }
            return base.CanConvertTo(context, destinationType);
        }

        private VisualBasicSettings CollectXmlNamespacesAndAssemblies(ITypeDescriptorContext context)
        {
            return VisualBasicExpressionConverter.CollectXmlNamespacesAndAssemblies(context);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if (str == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            if (str.Equals("Assembly references and imported namespaces for internal implementation"))
            {
                VisualBasicSettings settings = this.CollectXmlNamespacesAndAssemblies(context);
                if (settings != null)
                {
                    settings.SuppressXamlSerialization = true;
                }
                return settings;
            }
            if (!str.Equals(string.Empty) && !str.Equals("Assembly references and imported namespaces serialized as XML namespaces"))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidVisualBasicSettingsValue));
            }
            return this.CollectXmlNamespacesAndAssemblies(context);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

