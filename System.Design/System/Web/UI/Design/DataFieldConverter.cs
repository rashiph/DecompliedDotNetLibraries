namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.Design.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataFieldConverter : TypeConverter
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
            object[] values = null;
            if (context != null)
            {
                IComponent instance = context.Instance as IComponent;
                if (instance != null)
                {
                    ISite site = instance.Site;
                    if (site != null)
                    {
                        IDesignerHost host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                        if (host != null)
                        {
                            IDesigner dataBoundControlDesigner = host.GetDesigner(instance);
                            DesignerDataSourceView view = this.GetView(dataBoundControlDesigner);
                            if (view != null)
                            {
                                IDataSourceViewSchema schema = null;
                                try
                                {
                                    schema = view.Schema;
                                }
                                catch (Exception exception)
                                {
                                    IComponentDesignerDebugService service = (IComponentDesignerDebugService) site.GetService(typeof(IComponentDesignerDebugService));
                                    if (service != null)
                                    {
                                        service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                                    }
                                }
                                if (schema != null)
                                {
                                    IDataSourceFieldSchema[] fields = schema.GetFields();
                                    if (fields != null)
                                    {
                                        values = new object[fields.Length];
                                        for (int i = 0; i < fields.Length; i++)
                                        {
                                            values[i] = fields[i].Name;
                                        }
                                    }
                                }
                            }
                            if (((values == null) && (dataBoundControlDesigner != null)) && (dataBoundControlDesigner is IDataSourceProvider))
                            {
                                IDataSourceProvider provider = dataBoundControlDesigner as IDataSourceProvider;
                                IEnumerable dataSource = null;
                                if (provider != null)
                                {
                                    dataSource = provider.GetResolvedSelectedDataSource();
                                }
                                if (dataSource != null)
                                {
                                    PropertyDescriptorCollection dataFields = DesignTimeData.GetDataFields(dataSource);
                                    if (dataFields != null)
                                    {
                                        ArrayList list = new ArrayList();
                                        foreach (PropertyDescriptor descriptor in dataFields)
                                        {
                                            list.Add(descriptor.Name);
                                        }
                                        values = list.ToArray();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new TypeConverter.StandardValuesCollection(values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return ((context != null) && (context.Instance is IComponent));
        }

        private DesignerDataSourceView GetView(IDesigner dataBoundControlDesigner)
        {
            DataBoundControlDesigner designer = dataBoundControlDesigner as DataBoundControlDesigner;
            if (designer != null)
            {
                return designer.DesignerView;
            }
            BaseDataListDesigner designer2 = dataBoundControlDesigner as BaseDataListDesigner;
            if (designer2 != null)
            {
                return designer2.DesignerView;
            }
            RepeaterDesigner designer3 = dataBoundControlDesigner as RepeaterDesigner;
            if (designer3 != null)
            {
                return designer3.DesignerView;
            }
            return null;
        }
    }
}

