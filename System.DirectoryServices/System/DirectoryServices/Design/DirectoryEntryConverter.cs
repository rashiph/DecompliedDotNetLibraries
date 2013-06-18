namespace System.DirectoryServices.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Globalization;

    internal class DirectoryEntryConverter : TypeConverter
    {
        private static Hashtable componentsCreated = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private static TypeConverter.StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if ((value != null) && (value is string))
            {
                string path = ((string) value).Trim();
                if (path.Length == 0)
                {
                    return null;
                }
                if ((path.CompareTo(Res.GetString("DSNotSet")) != 0) && (GetFromCache(path) == null))
                {
                    DirectoryEntry component = new DirectoryEntry(path);
                    componentsCreated[path] = component;
                    if (context != null)
                    {
                        context.Container.Add(component);
                    }
                    return component;
                }
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == null) || !(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value != null)
            {
                return ((DirectoryEntry) value).Path;
            }
            return Res.GetString("DSNotSet");
        }

        internal static DirectoryEntry GetFromCache(string path)
        {
            if (componentsCreated.ContainsKey(path))
            {
                DirectoryEntry entry = (DirectoryEntry) componentsCreated[path];
                if (entry.Site == null)
                {
                    componentsCreated.Remove(path);
                }
                else
                {
                    if (entry.Path == path)
                    {
                        return entry;
                    }
                    componentsCreated.Remove(path);
                }
            }
            return null;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (DirectoryEntryConverter.values == null)
            {
                object[] values = new object[1];
                DirectoryEntryConverter.values = new TypeConverter.StandardValuesCollection(values);
            }
            return DirectoryEntryConverter.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

