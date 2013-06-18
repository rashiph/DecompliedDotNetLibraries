namespace System.Diagnostics.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    internal class CounterNameConverter : TypeConverter
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
                PerformanceCounterCategory category = new PerformanceCounterCategory(categoryName, machineName);
                string[] instanceNames = category.GetInstanceNames();
                PerformanceCounter[] counters = null;
                if (instanceNames.Length == 0)
                {
                    counters = category.GetCounters();
                }
                else
                {
                    counters = category.GetCounters(instanceNames[0]);
                }
                string[] array = new string[counters.Length];
                for (int i = 0; i < counters.Length; i++)
                {
                    array[i] = counters[i].CounterName;
                }
                Array.Sort(array, Comparer.Default);
                return new TypeConverter.StandardValuesCollection(array);
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

