namespace System.Web.UI.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataSourceConverter : TypeConverter
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
            object[] array = null;
            if (context != null)
            {
                ArrayList list = new ArrayList();
                IContainer container = context.Container;
                if (container != null)
                {
                    foreach (IComponent component in container.Components)
                    {
                        if (this.IsValidDataSource(component) && !Marshal.IsComObject(component))
                        {
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Modifiers"];
                            if (descriptor != null)
                            {
                                MemberAttributes attributes = (MemberAttributes) descriptor.GetValue(component);
                                if ((attributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
                                {
                                    continue;
                                }
                            }
                            ISite site = component.Site;
                            if (site != null)
                            {
                                string name = site.Name;
                                if (name != null)
                                {
                                    list.Add(name);
                                }
                            }
                        }
                    }
                }
                array = list.ToArray();
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
            return true;
        }

        protected virtual bool IsValidDataSource(IComponent component)
        {
            return ((component is IEnumerable) || (component is IListSource));
        }
    }
}

