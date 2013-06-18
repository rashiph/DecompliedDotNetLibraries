namespace System.Configuration.Install
{
    using System;
    using System.ComponentModel;

    internal class InstallerParentConverter : ReferenceConverter
    {
        public InstallerParentConverter(Type type) : base(type)
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            TypeConverter.StandardValuesCollection standardValues = base.GetStandardValues(context);
            object instance = context.Instance;
            int num = 0;
            int index = 0;
            object[] values = new object[standardValues.Count - 1];
            while (num < standardValues.Count)
            {
                if (standardValues[num] != instance)
                {
                    values[index] = standardValues[num];
                    index++;
                }
                num++;
            }
            return new TypeConverter.StandardValuesCollection(values);
        }
    }
}

