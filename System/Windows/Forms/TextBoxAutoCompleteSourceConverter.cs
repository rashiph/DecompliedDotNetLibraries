namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class TextBoxAutoCompleteSourceConverter : EnumConverter
    {
        public TextBoxAutoCompleteSourceConverter(System.Type type) : base(type)
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            TypeConverter.StandardValuesCollection standardValues = base.GetStandardValues(context);
            ArrayList values = new ArrayList();
            int count = standardValues.Count;
            for (int i = 0; i < count; i++)
            {
                if (!standardValues[i].ToString().Equals("ListItems"))
                {
                    values.Add(standardValues[i]);
                }
            }
            return new TypeConverter.StandardValuesCollection(values);
        }
    }
}

