namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    internal class TableLayoutPanelCellPositionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string str = ((string) value).Trim();
            if (str.Length == 0)
            {
                return null;
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }
            char ch = culture.TextInfo.ListSeparator[0];
            string[] strArray = str.Split(new char[] { ch });
            int[] numArray = new int[strArray.Length];
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = (int) converter.ConvertFromString(context, culture, strArray[i]);
            }
            if (numArray.Length != 2)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("TextParseFailedFormat", new object[] { str, "column, row" }));
            }
            return new TableLayoutPanelCellPosition(numArray[0], numArray[1]);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is TableLayoutPanelCellPosition))
            {
                TableLayoutPanelCellPosition position = (TableLayoutPanelCellPosition) value;
                return new InstanceDescriptor(typeof(TableLayoutPanelCellPosition).GetConstructor(new System.Type[] { typeof(int), typeof(int) }), new object[] { position.Column, position.Row });
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return new TableLayoutPanelCellPosition((int) propertyValues["Column"], (int) propertyValues["Row"]);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(TableLayoutPanelCellPosition), attributes).Sort(new string[] { "Column", "Row" });
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

