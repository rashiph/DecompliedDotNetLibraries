namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class AlphabeticalEnumConverter : EnumConverter
    {
        public AlphabeticalEnumConverter(Type type) : base(type)
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (base.Values == null)
            {
                Array values = Enum.GetValues(base.EnumType);
                object[] keys = new object[values.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = this.ConvertTo(context, null, values.GetValue(i), typeof(string));
                }
                Array.Sort(keys, values, 0, values.Length, Comparer.Default);
                base.Values = new TypeConverter.StandardValuesCollection(values);
            }
            return base.Values;
        }
    }
}

