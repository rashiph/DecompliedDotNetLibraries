namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;

    internal class ListViewItemCollectionEditor : CollectionEditor
    {
        public ListViewItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override string GetDisplayText(object value)
        {
            string str;
            if (value == null)
            {
                return string.Empty;
            }
            PropertyDescriptor defaultProperty = TypeDescriptor.GetDefaultProperty(base.CollectionType);
            if ((defaultProperty != null) && (defaultProperty.PropertyType == typeof(string)))
            {
                str = (string) defaultProperty.GetValue(value);
                if ((str != null) && (str.Length > 0))
                {
                    return str;
                }
            }
            str = TypeDescriptor.GetConverter(value).ConvertToString(value);
            if ((str != null) && (str.Length != 0))
            {
                return str;
            }
            return value.GetType().Name;
        }
    }
}

