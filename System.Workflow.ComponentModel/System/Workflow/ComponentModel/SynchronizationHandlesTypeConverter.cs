namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;

    internal sealed class SynchronizationHandlesTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return UnStringify(value as string);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is ICollection<string>))
            {
                return Stringify(value as ICollection<string>);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        internal static string Stringify(ICollection<string> synchronizationHandles)
        {
            string str = string.Empty;
            if (synchronizationHandles != null)
            {
                foreach (string str2 in synchronizationHandles)
                {
                    if (str2 != null)
                    {
                        if (str != string.Empty)
                        {
                            str = str + ", ";
                        }
                        str = str + str2.Replace(",", @"\,");
                    }
                }
            }
            return str;
        }

        internal static ICollection<string> UnStringify(string stringifiedValue)
        {
            ICollection<string> is2 = new List<string>();
            stringifiedValue = stringifiedValue.Replace(@"\,", ">");
            foreach (string str in stringifiedValue.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string item = str.Trim().Replace('>', ',');
                if ((item != string.Empty) && !is2.Contains(item))
                {
                    is2.Add(item);
                }
            }
            return is2;
        }
    }
}

