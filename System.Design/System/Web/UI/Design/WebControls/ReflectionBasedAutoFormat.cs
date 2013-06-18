namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web.UI;
    using System.Web.UI.Design;

    internal class ReflectionBasedAutoFormat : DesignerAutoFormat
    {
        private DataRow _schemeData;
        private readonly string _schemeName;
        private readonly string _schemes;
        private const char OM_CHAR = '.';
        private const char PERSIST_CHAR = '-';

        public ReflectionBasedAutoFormat(string schemeName, string schemes) : base(System.Design.SR.GetString(schemeName))
        {
            this._schemeName = schemeName;
            this._schemes = schemes;
        }

        public override void Apply(Control control)
        {
            this.EnsureInitialized();
            foreach (DataColumn column in this._schemeData.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (!string.Equals(columnName, "SchemeName", StringComparison.Ordinal))
                {
                    if (columnName.EndsWith("--ClearDefaults", StringComparison.Ordinal))
                    {
                        if (this._schemeData[columnName].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            this.ClearDefaults(control, columnName.Substring(0, columnName.Length - 15));
                        }
                    }
                    else
                    {
                        this.SetPropertyValue(control, columnName, this._schemeData[columnName].ToString());
                    }
                }
            }
        }

        private void ClearDefaults(Control control, string propertyName)
        {
            InstanceAndPropertyInfo memberInfo = GetMemberInfo(control, propertyName);
            if ((memberInfo.PropertyInfo != null) && (memberInfo.Instance != null))
            {
                object target = memberInfo.PropertyInfo.GetValue(memberInfo.Instance, null);
                target.GetType().InvokeMember("ClearDefaults", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, target, new object[0], CultureInfo.InvariantCulture);
            }
        }

        private void EnsureInitialized()
        {
            if (this._schemeData == null)
            {
                this._schemeData = ControlDesigner.GetSchemeDataRow(this._schemeName, this._schemes);
            }
        }

        private static InstanceAndPropertyInfo GetMemberInfo(Control control, string name)
        {
            Type propertyType = control.GetType();
            PropertyInfo propertyInfo = null;
            object obj2 = control;
            object obj3 = control;
            string str = name.Replace('-', '.');
            int startIndex = 0;
            while (startIndex < str.Length)
            {
                string str2;
                int index = str.IndexOf('.', startIndex);
                if (index < 0)
                {
                    str2 = str.Substring(startIndex);
                    startIndex = str.Length;
                }
                else
                {
                    str2 = str.Substring(startIndex, index - startIndex);
                    startIndex = index + 1;
                }
                BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
                try
                {
                    propertyInfo = propertyType.GetProperty(str2, bindingAttr);
                }
                catch (AmbiguousMatchException)
                {
                    bindingAttr |= BindingFlags.DeclaredOnly;
                    propertyInfo = propertyType.GetProperty(str2, bindingAttr);
                }
                if (propertyInfo != null)
                {
                    propertyType = propertyInfo.PropertyType;
                    if (obj3 != null)
                    {
                        obj2 = obj3;
                        obj3 = propertyInfo.GetValue(obj2, null);
                    }
                }
            }
            return new InstanceAndPropertyInfo(obj2, propertyInfo);
        }

        protected void SetPropertyValue(Control control, string propertyName, string propertyValue)
        {
            object obj2 = null;
            InstanceAndPropertyInfo memberInfo = GetMemberInfo(control, propertyName);
            PropertyInfo propertyInfo = memberInfo.PropertyInfo;
            TypeConverter converter = null;
            TypeConverterAttribute attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(TypeConverterAttribute), true) as TypeConverterAttribute;
            if (attribute != null)
            {
                Type type = Type.GetType(attribute.ConverterTypeName, false);
                if (type != null)
                {
                    converter = (TypeConverter) Activator.CreateInstance(type);
                }
            }
            if ((converter != null) && converter.CanConvertFrom(typeof(string)))
            {
                obj2 = converter.ConvertFromInvariantString(propertyValue);
            }
            else
            {
                converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                if ((converter != null) && converter.CanConvertFrom(typeof(string)))
                {
                    obj2 = converter.ConvertFromInvariantString(propertyValue);
                }
            }
            propertyInfo.SetValue(memberInfo.Instance, obj2, null);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InstanceAndPropertyInfo
        {
            public object Instance;
            public System.Reflection.PropertyInfo PropertyInfo;
            public InstanceAndPropertyInfo(object instance, System.Reflection.PropertyInfo propertyInfo)
            {
                this.Instance = instance;
                this.PropertyInfo = propertyInfo;
            }
        }
    }
}

