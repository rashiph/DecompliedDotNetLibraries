namespace System.Diagnostics.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    internal class CategoryValueConverter : TypeConverter
    {
        private string previousMachineName;
        private TypeConverter.StandardValuesCollection values;

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
            if (counter != null)
            {
                machineName = counter.MachineName;
            }
            if (machineName != this.previousMachineName)
            {
                this.previousMachineName = machineName;
                try
                {
                    PerformanceCounter.CloseSharedResources();
                    PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories(machineName);
                    string[] array = new string[categories.Length];
                    for (int i = 0; i < categories.Length; i++)
                    {
                        array[i] = categories[i].CategoryName;
                    }
                    Array.Sort(array, Comparer.Default);
                    this.values = new TypeConverter.StandardValuesCollection(array);
                }
                catch (Exception)
                {
                    this.values = null;
                }
            }
            return this.values;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

