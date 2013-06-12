namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class SpecialFolderEnumConverter : AlphaSortedEnumConverter
    {
        public SpecialFolderEnumConverter(System.Type type) : base(type)
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            TypeConverter.StandardValuesCollection standardValues = base.GetStandardValues(context);
            ArrayList values = new ArrayList();
            int count = standardValues.Count;
            bool flag = false;
            for (int i = 0; i < count; i++)
            {
                if ((standardValues[i] is Environment.SpecialFolder) && standardValues[i].Equals(Environment.SpecialFolder.Personal))
                {
                    if (!flag)
                    {
                        flag = true;
                        values.Add(standardValues[i]);
                    }
                }
                else
                {
                    values.Add(standardValues[i]);
                }
            }
            return new TypeConverter.StandardValuesCollection(values);
        }
    }
}

