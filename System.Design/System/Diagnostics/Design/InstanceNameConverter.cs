namespace System.Diagnostics.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    internal class InstanceNameConverter : TypeConverter
    {
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
            PerformanceCounter counter = (context == null) ? null : (context.Instance as PerformanceCounter);
            string machineName = ".";
            string categoryName = string.Empty;
            if (counter != null)
            {
                machineName = counter.MachineName;
                categoryName = counter.CategoryName;
            }
            try
            {
                string[] instanceNames = new PerformanceCounterCategory(categoryName, machineName).GetInstanceNames();
                Array.Sort(instanceNames, Comparer.Default);
                return new TypeConverter.StandardValuesCollection(instanceNames);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

