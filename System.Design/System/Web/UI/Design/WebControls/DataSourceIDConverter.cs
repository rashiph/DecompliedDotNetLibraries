namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.Design;

    public class DataSourceIDConverter : TypeConverter
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
            string[] values = null;
            if (context != null)
            {
                WebFormsRootDesigner designer = null;
                IDesignerHost service = (IDesignerHost) context.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    IComponent rootComponent = service.RootComponent;
                    if (rootComponent != null)
                    {
                        designer = service.GetDesigner(rootComponent) as WebFormsRootDesigner;
                    }
                }
                if ((designer != null) && !designer.IsDesignerViewLocked)
                {
                    IComponent instance = context.Instance as IComponent;
                    if (instance == null)
                    {
                        DesignerActionList list = context.Instance as DesignerActionList;
                        if (list != null)
                        {
                            instance = list.Component;
                        }
                    }
                    IList<IComponent> allComponents = ControlHelper.GetAllComponents(instance, new ControlHelper.IsValidComponentDelegate(this.IsValidDataSource));
                    List<string> list3 = new List<string>();
                    foreach (IComponent component3 in allComponents)
                    {
                        Control control = component3 as Control;
                        if (((control != null) && !string.IsNullOrEmpty(control.ID)) && !list3.Contains(control.ID))
                        {
                            list3.Add(control.ID);
                        }
                    }
                    list3.Sort(StringComparer.OrdinalIgnoreCase);
                    list3.Insert(0, System.Design.SR.GetString("DataSourceIDChromeConverter_NoDataSource"));
                    list3.Add(System.Design.SR.GetString("DataSourceIDChromeConverter_NewDataSource"));
                    values = list3.ToArray();
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
            return true;
        }

        protected virtual bool IsValidDataSource(IComponent component)
        {
            Control control = component as Control;
            if (control == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(control.ID))
            {
                return false;
            }
            return (component is IDataSource);
        }
    }
}

