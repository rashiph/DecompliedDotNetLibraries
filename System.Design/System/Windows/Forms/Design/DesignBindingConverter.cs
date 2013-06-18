namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Globalization;

    internal class DesignBindingConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type destType)
        {
            return (typeof(string) == destType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type sourceType)
        {
            return (typeof(string) == sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string strA = (string) value;
            if (((strA == null) || (strA.Length == 0)) || (string.Compare(strA, System.Design.SR.GetString("DataGridNoneString"), true, CultureInfo.CurrentCulture) == 0))
            {
                return DesignBinding.Null;
            }
            int index = strA.IndexOf("-");
            if (index == -1)
            {
                throw new ArgumentException(System.Design.SR.GetString("DesignBindingBadParseString", new object[] { strA }));
            }
            string a = strA.Substring(0, index - 1).Trim();
            string dataMember = strA.Substring(index + 1).Trim();
            if ((context == null) || (context.Container == null))
            {
                throw new ArgumentException(System.Design.SR.GetString("DesignBindingContextRequiredWhenParsing", new object[] { strA }));
            }
            IComponent dataSource = DesignerUtils.CheckForNestedContainer(context.Container).Components[a];
            if (dataSource != null)
            {
                return new DesignBinding(dataSource, dataMember);
            }
            if (!string.Equals(a, "(List)", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(System.Design.SR.GetString("DesignBindingComponentNotFound", new object[] { a }));
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type sourceType)
        {
            DesignBinding binding = (DesignBinding) value;
            if (binding.IsNull)
            {
                return System.Design.SR.GetString("DataGridNoneString");
            }
            string name = "";
            if (binding.DataSource is IComponent)
            {
                IComponent dataSource = (IComponent) binding.DataSource;
                if (dataSource.Site != null)
                {
                    name = dataSource.Site.Name;
                }
            }
            if (name.Length == 0)
            {
                if (((binding.DataSource is IListSource) || (binding.DataSource is IList)) || (binding.DataSource is Array))
                {
                    name = "(List)";
                }
                else
                {
                    string className = TypeDescriptor.GetClassName(binding.DataSource);
                    int num = className.LastIndexOf('.');
                    if (num != -1)
                    {
                        className = className.Substring(num + 1);
                    }
                    name = string.Format(CultureInfo.CurrentCulture, "({0})", new object[] { className });
                }
            }
            return (name + " - " + binding.DataMember);
        }
    }
}

