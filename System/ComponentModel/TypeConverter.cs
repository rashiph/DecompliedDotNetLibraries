namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class TypeConverter
    {
        private static bool firstLoadAppSetting = true;
        private static object loadAppSettingLock = new object();
        private const string s_UseCompatibleTypeConverterBehavior = "UseCompatibleTypeConverterBehavior";
        private static bool useCompatibleTypeConversion = false;

        public bool CanConvertFrom(Type sourceType)
        {
            return this.CanConvertFrom(null, sourceType);
        }

        public virtual bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(InstanceDescriptor));
        }

        public bool CanConvertTo(Type destinationType)
        {
            return this.CanConvertTo(null, destinationType);
        }

        public virtual bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public object ConvertFrom(object value)
        {
            return this.ConvertFrom(null, CultureInfo.CurrentCulture, value);
        }

        public virtual object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            InstanceDescriptor descriptor = value as InstanceDescriptor;
            if (descriptor == null)
            {
                throw this.GetConvertFromException(value);
            }
            return descriptor.Invoke();
        }

        public object ConvertFromInvariantString(string text)
        {
            return this.ConvertFromString(null, CultureInfo.InvariantCulture, text);
        }

        public object ConvertFromInvariantString(ITypeDescriptorContext context, string text)
        {
            return this.ConvertFromString(context, CultureInfo.InvariantCulture, text);
        }

        public object ConvertFromString(string text)
        {
            return this.ConvertFrom(null, null, text);
        }

        public object ConvertFromString(ITypeDescriptorContext context, string text)
        {
            return this.ConvertFrom(context, CultureInfo.CurrentCulture, text);
        }

        public object ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string text)
        {
            return this.ConvertFrom(context, culture, text);
        }

        public object ConvertTo(object value, Type destinationType)
        {
            return this.ConvertTo(null, null, value, destinationType);
        }

        public virtual object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(destinationType == typeof(string)))
            {
                throw this.GetConvertToException(value, destinationType);
            }
            if (value == null)
            {
                return string.Empty;
            }
            if ((culture != null) && (culture != CultureInfo.CurrentCulture))
            {
                IFormattable formattable = value as IFormattable;
                if (formattable != null)
                {
                    return formattable.ToString(null, culture);
                }
            }
            return value.ToString();
        }

        public string ConvertToInvariantString(object value)
        {
            return this.ConvertToString(null, CultureInfo.InvariantCulture, value);
        }

        public string ConvertToInvariantString(ITypeDescriptorContext context, object value)
        {
            return this.ConvertToString(context, CultureInfo.InvariantCulture, value);
        }

        public string ConvertToString(object value)
        {
            return (string) this.ConvertTo(null, CultureInfo.CurrentCulture, value, typeof(string));
        }

        public string ConvertToString(ITypeDescriptorContext context, object value)
        {
            return (string) this.ConvertTo(context, CultureInfo.CurrentCulture, value, typeof(string));
        }

        public string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return (string) this.ConvertTo(context, culture, value, typeof(string));
        }

        public object CreateInstance(IDictionary propertyValues)
        {
            return this.CreateInstance(null, propertyValues);
        }

        public virtual object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return null;
        }

        protected Exception GetConvertFromException(object value)
        {
            string fullName;
            if (value == null)
            {
                fullName = System.SR.GetString("ToStringNull");
            }
            else
            {
                fullName = value.GetType().FullName;
            }
            throw new NotSupportedException(System.SR.GetString("ConvertFromException", new object[] { base.GetType().Name, fullName }));
        }

        protected Exception GetConvertToException(object value, Type destinationType)
        {
            string fullName;
            if (value == null)
            {
                fullName = System.SR.GetString("ToStringNull");
            }
            else
            {
                fullName = value.GetType().FullName;
            }
            throw new NotSupportedException(System.SR.GetString("ConvertToException", new object[] { base.GetType().Name, fullName, destinationType.FullName }));
        }

        public bool GetCreateInstanceSupported()
        {
            return this.GetCreateInstanceSupported(null);
        }

        public virtual bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public PropertyDescriptorCollection GetProperties(object value)
        {
            return this.GetProperties(null, value);
        }

        public PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value)
        {
            return this.GetProperties(context, value, new Attribute[] { BrowsableAttribute.Yes });
        }

        public virtual PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return null;
        }

        public bool GetPropertiesSupported()
        {
            return this.GetPropertiesSupported(null);
        }

        public virtual bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public ICollection GetStandardValues()
        {
            return this.GetStandardValues(null);
        }

        public virtual StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return null;
        }

        public bool GetStandardValuesExclusive()
        {
            return this.GetStandardValuesExclusive(null);
        }

        public virtual bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public bool GetStandardValuesSupported()
        {
            return this.GetStandardValuesSupported(null);
        }

        public virtual bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public bool IsValid(object value)
        {
            return this.IsValid(null, value);
        }

        public virtual bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (UseCompatibleTypeConversion)
            {
                return true;
            }
            bool flag = true;
            try
            {
                if ((value == null) || this.CanConvertFrom(context, value.GetType()))
                {
                    this.ConvertFrom(context, CultureInfo.InvariantCulture, value);
                    return flag;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        protected PropertyDescriptorCollection SortProperties(PropertyDescriptorCollection props, string[] names)
        {
            props.Sort(names);
            return props;
        }

        private static bool UseCompatibleTypeConversion
        {
            get
            {
                if (firstLoadAppSetting)
                {
                    lock (loadAppSettingLock)
                    {
                        if (firstLoadAppSetting)
                        {
                            string str = ConfigurationManager.AppSettings["UseCompatibleTypeConverterBehavior"];
                            try
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    useCompatibleTypeConversion = bool.Parse(str.Trim());
                                }
                            }
                            catch
                            {
                                useCompatibleTypeConversion = false;
                            }
                            firstLoadAppSetting = false;
                        }
                    }
                }
                return useCompatibleTypeConversion;
            }
        }

        protected abstract class SimplePropertyDescriptor : PropertyDescriptor
        {
            private Type componentType;
            private Type propertyType;

            protected SimplePropertyDescriptor(Type componentType, string name, Type propertyType) : this(componentType, name, propertyType, new Attribute[0])
            {
            }

            protected SimplePropertyDescriptor(Type componentType, string name, Type propertyType, Attribute[] attributes) : base(name, attributes)
            {
                this.componentType = componentType;
                this.propertyType = propertyType;
            }

            public override bool CanResetValue(object component)
            {
                DefaultValueAttribute attribute = (DefaultValueAttribute) this.Attributes[typeof(DefaultValueAttribute)];
                if (attribute == null)
                {
                    return false;
                }
                return attribute.Value.Equals(this.GetValue(component));
            }

            public override void ResetValue(object component)
            {
                DefaultValueAttribute attribute = (DefaultValueAttribute) this.Attributes[typeof(DefaultValueAttribute)];
                if (attribute != null)
                {
                    this.SetValue(component, attribute.Value);
                }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get
                {
                    return this.componentType;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this.Attributes.Contains(ReadOnlyAttribute.Yes);
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this.propertyType;
                }
            }
        }

        public class StandardValuesCollection : ICollection, IEnumerable
        {
            private Array valueArray;
            private ICollection values;

            public StandardValuesCollection(ICollection values)
            {
                if (values == null)
                {
                    values = new object[0];
                }
                Array array = values as Array;
                if (array != null)
                {
                    this.valueArray = array;
                }
                this.values = values;
            }

            public void CopyTo(Array array, int index)
            {
                this.values.CopyTo(array, index);
            }

            public IEnumerator GetEnumerator()
            {
                return this.values.GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                this.CopyTo(array, index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    if (this.valueArray != null)
                    {
                        return this.valueArray.Length;
                    }
                    return this.values.Count;
                }
            }

            public object this[int index]
            {
                get
                {
                    if (this.valueArray == null)
                    {
                        IList values = this.values as IList;
                        if (values != null)
                        {
                            return values[index];
                        }
                        this.valueArray = new object[this.values.Count];
                        this.values.CopyTo(this.valueArray, 0);
                    }
                    return this.valueArray.GetValue(index);
                }
            }

            int ICollection.Count
            {
                get
                {
                    return this.Count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return null;
                }
            }
        }
    }
}

