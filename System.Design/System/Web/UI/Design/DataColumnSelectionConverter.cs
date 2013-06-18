namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataColumnSelectionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value.GetType() != typeof(string))
            {
                throw base.GetConvertFromException(value);
            }
            return (string) value;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] array = null;
            ArrayList list = new ArrayList();
            if (context != null)
            {
                IComponent instance = context.Instance as IComponent;
                if (instance != null)
                {
                    GridView view = instance as GridView;
                    if (view != null)
                    {
                        if (view.AutoGenerateColumns)
                        {
                            DataFieldConverter converter = new DataFieldConverter();
                            foreach (object obj2 in converter.GetStandardValues(context))
                            {
                                list.Add(obj2);
                            }
                        }
                        foreach (DataControlField field in view.Columns)
                        {
                            BoundField field2 = field as BoundField;
                            if (field2 != null)
                            {
                                string dataField = field2.DataField;
                                if (!list.Contains(dataField))
                                {
                                    list.Add(dataField);
                                }
                            }
                        }
                        list.Sort();
                        array = new string[list.Count];
                        list.CopyTo(array, 0);
                    }
                }
            }
            return new TypeConverter.StandardValuesCollection(array);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return ((context != null) && (context.Instance is IComponent));
        }
    }
}

