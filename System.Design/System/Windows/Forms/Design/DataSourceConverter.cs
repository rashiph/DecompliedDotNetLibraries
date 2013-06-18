namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Windows.Forms;

    internal class DataSourceConverter : ReferenceConverter
    {
        private ReferenceConverter listConverter;

        public DataSourceConverter() : base(typeof(IListSource))
        {
            this.listConverter = new ReferenceConverter(typeof(IList));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is System.Type))
            {
                return value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList list = new ArrayList(base.GetStandardValues(context));
            TypeConverter.StandardValuesCollection standardValues = this.listConverter.GetStandardValues(context);
            ArrayList values = new ArrayList();
            BindingSource instance = context.Instance as BindingSource;
            foreach (object obj2 in list)
            {
                if (obj2 != null)
                {
                    ListBindableAttribute attribute = (ListBindableAttribute) TypeDescriptor.GetAttributes(obj2)[typeof(ListBindableAttribute)];
                    if (((attribute == null) || attribute.ListBindable) && ((instance == null) || (instance != obj2)))
                    {
                        DataTable table = obj2 as DataTable;
                        if ((table == null) || !list.Contains(table.DataSet))
                        {
                            values.Add(obj2);
                        }
                    }
                }
            }
            foreach (object obj3 in standardValues)
            {
                if (obj3 != null)
                {
                    ListBindableAttribute attribute2 = (ListBindableAttribute) TypeDescriptor.GetAttributes(obj3)[typeof(ListBindableAttribute)];
                    if (((attribute2 == null) || attribute2.ListBindable) && ((instance == null) || (instance != obj3)))
                    {
                        values.Add(obj3);
                    }
                }
            }
            values.Add(null);
            return new TypeConverter.StandardValuesCollection(values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

