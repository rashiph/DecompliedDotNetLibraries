namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.Design.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataMemberConverter : TypeConverter
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
            if (context != null)
            {
                IComponent instance = context.Instance as IComponent;
                if (instance != null)
                {
                    ISite site = instance.Site;
                    if (site != null)
                    {
                        IDesignerHost service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                        if (service != null)
                        {
                            IDesigner dataBoundControlDesigner = service.GetDesigner(instance);
                            DesignerDataSourceView view = this.GetView(dataBoundControlDesigner);
                            if (view != null)
                            {
                                IDataSourceDesigner dataSourceDesigner = view.DataSourceDesigner;
                                if (dataSourceDesigner != null)
                                {
                                    string[] viewNames = dataSourceDesigner.GetViewNames();
                                    if (viewNames != null)
                                    {
                                        array = new string[viewNames.Length];
                                        viewNames.CopyTo(array, 0);
                                    }
                                }
                            }
                            if (((array == null) && (dataBoundControlDesigner != null)) && (dataBoundControlDesigner is IDataSourceProvider))
                            {
                                IDataSourceProvider provider = dataBoundControlDesigner as IDataSourceProvider;
                                object dataSource = null;
                                if (provider != null)
                                {
                                    dataSource = provider.GetSelectedDataSource();
                                }
                                if (dataSource != null)
                                {
                                    array = DesignTimeData.GetDataMembers(dataSource);
                                }
                            }
                        }
                    }
                }
                if (array == null)
                {
                    array = new string[0];
                }
                Array.Sort(array, Comparer.Default);
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

