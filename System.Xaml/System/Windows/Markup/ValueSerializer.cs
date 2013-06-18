namespace System.Windows.Markup
{
    using MS.Internal.Serialization;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xaml.Replacements;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class ValueSerializer
    {
        private static Hashtable _valueSerializers = new Hashtable();
        private static object _valueSerializersLock = new object();
        private static List<Type> Empty = new List<Type>();

        static ValueSerializer()
        {
            TypeDescriptor.Refreshed += new RefreshEventHandler(ValueSerializer.TypeDescriptorRefreshed);
        }

        protected ValueSerializer()
        {
        }

        public virtual bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return false;
        }

        public virtual bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return false;
        }

        public virtual object ConvertFromString(string value, IValueSerializerContext context)
        {
            throw this.GetConvertFromException(value);
        }

        public virtual string ConvertToString(object value, IValueSerializerContext context)
        {
            throw this.GetConvertToException(value, typeof(string));
        }

        protected Exception GetConvertFromException(object value)
        {
            string fullName;
            if (value == null)
            {
                fullName = System.Xaml.SR.Get("ToStringNull");
            }
            else
            {
                fullName = value.GetType().FullName;
            }
            return new NotSupportedException(System.Xaml.SR.Get("ConvertFromException", new object[] { base.GetType().Name, fullName }));
        }

        protected Exception GetConvertToException(object value, Type destinationType)
        {
            string fullName;
            if (value == null)
            {
                fullName = System.Xaml.SR.Get("ToStringNull");
            }
            else
            {
                fullName = value.GetType().FullName;
            }
            return new NotSupportedException(System.Xaml.SR.Get("ConvertToException", new object[] { base.GetType().Name, fullName, destinationType.FullName }));
        }

        public static ValueSerializer GetSerializerFor(PropertyDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            ValueSerializerAttribute attribute = descriptor.Attributes[typeof(ValueSerializerAttribute)] as ValueSerializerAttribute;
            if (attribute != null)
            {
                return (ValueSerializer) Activator.CreateInstance(attribute.ValueSerializerType);
            }
            ValueSerializer serializerFor = GetSerializerFor(descriptor.PropertyType);
            if ((serializerFor == null) || (serializerFor is TypeConverterValueSerializer))
            {
                TypeConverter converter = descriptor.Converter;
                if (((converter != null) && converter.CanConvertTo(typeof(string))) && (converter.CanConvertFrom(typeof(string)) && !(converter is ReferenceConverter)))
                {
                    serializerFor = new TypeConverterValueSerializer(converter);
                }
            }
            return serializerFor;
        }

        public static ValueSerializer GetSerializerFor(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object obj2 = _valueSerializers[type];
            if (obj2 != null)
            {
                if (obj2 != _valueSerializersLock)
                {
                    return (obj2 as ValueSerializer);
                }
                return null;
            }
            ValueSerializerAttribute attribute = TypeDescriptor.GetAttributes(type)[typeof(ValueSerializerAttribute)] as ValueSerializerAttribute;
            ValueSerializer serializer = null;
            if (attribute != null)
            {
                serializer = (ValueSerializer) Activator.CreateInstance(attribute.ValueSerializerType);
            }
            if (serializer == null)
            {
                if (type == typeof(string))
                {
                    serializer = new StringValueSerializer();
                }
                else
                {
                    TypeConverter typeConverter = TypeConverterHelper.GetTypeConverter(type);
                    if (typeConverter.GetType() == typeof(DateTimeConverter2))
                    {
                        serializer = new DateTimeValueSerializer();
                    }
                    else if ((typeConverter.CanConvertTo(typeof(string)) && typeConverter.CanConvertFrom(typeof(string))) && !(typeConverter is ReferenceConverter))
                    {
                        serializer = new TypeConverterValueSerializer(typeConverter);
                    }
                }
            }
            lock (_valueSerializersLock)
            {
                _valueSerializers[type] = (serializer == null) ? _valueSerializersLock : serializer;
            }
            return serializer;
        }

        public static ValueSerializer GetSerializerFor(PropertyDescriptor descriptor, IValueSerializerContext context)
        {
            if (context != null)
            {
                ValueSerializer valueSerializerFor = context.GetValueSerializerFor(descriptor);
                if (valueSerializerFor != null)
                {
                    return valueSerializerFor;
                }
            }
            return GetSerializerFor(descriptor);
        }

        public static ValueSerializer GetSerializerFor(Type type, IValueSerializerContext context)
        {
            if (context != null)
            {
                ValueSerializer valueSerializerFor = context.GetValueSerializerFor(type);
                if (valueSerializerFor != null)
                {
                    return valueSerializerFor;
                }
            }
            return GetSerializerFor(type);
        }

        private static void TypeDescriptorRefreshed(RefreshEventArgs args)
        {
            _valueSerializers = new Hashtable();
        }

        public virtual IEnumerable<Type> TypeReferences(object value, IValueSerializerContext context)
        {
            return Empty;
        }
    }
}

