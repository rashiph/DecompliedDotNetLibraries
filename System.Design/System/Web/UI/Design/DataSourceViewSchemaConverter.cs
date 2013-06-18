namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataSourceViewSchemaConverter : TypeConverter
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
            return this.GetStandardValues(context, null);
        }

        public virtual TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context, Type typeFilter)
        {
            string[] destinationArray = null;
            if (context != null)
            {
                IDataSourceViewSchemaAccessor instance = context.Instance as IDataSourceViewSchemaAccessor;
                if (instance != null)
                {
                    IDataSourceViewSchema dataSourceViewSchema = instance.DataSourceViewSchema as IDataSourceViewSchema;
                    if (dataSourceViewSchema != null)
                    {
                        IDataSourceFieldSchema[] fields = dataSourceViewSchema.GetFields();
                        string[] sourceArray = new string[fields.Length];
                        int index = 0;
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (((typeFilter != null) && (fields[i].DataType == typeFilter)) || (typeFilter == null))
                            {
                                sourceArray[index] = fields[i].Name;
                                index++;
                            }
                        }
                        destinationArray = new string[index];
                        Array.Copy(sourceArray, destinationArray, index);
                    }
                }
                if (destinationArray == null)
                {
                    destinationArray = new string[0];
                }
                Array.Sort(destinationArray, Comparer.Default);
            }
            return new TypeConverter.StandardValuesCollection(destinationArray);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return ((context != null) && (context.Instance is IDataSourceViewSchemaAccessor));
        }
    }
}

