namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Xml.Serialization;

    public class SettingsPropertyValue
    {
        private bool _ChangedSinceLastSerialized;
        private bool _Deserialized;
        private bool _IsDirty;
        private SettingsProperty _Property;
        private object _SerializedValue;
        private bool _UsingDefaultValue = true;
        private object _Value;

        public SettingsPropertyValue(SettingsProperty property)
        {
            this._Property = property;
        }

        private static string ConvertObjectToString(object propValue, Type type, SettingsSerializeAs serializeAs, bool throwOnError)
        {
            if (serializeAs == SettingsSerializeAs.ProviderSpecific)
            {
                if ((type == typeof(string)) || type.IsPrimitive)
                {
                    serializeAs = SettingsSerializeAs.String;
                }
                else
                {
                    serializeAs = SettingsSerializeAs.Xml;
                }
            }
            try
            {
                XmlSerializer serializer;
                switch (serializeAs)
                {
                    case SettingsSerializeAs.String:
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(type);
                        if (((converter == null) || !converter.CanConvertTo(typeof(string))) || !converter.CanConvertFrom(typeof(string)))
                        {
                            break;
                        }
                        return converter.ConvertToInvariantString(propValue);
                    }
                    case SettingsSerializeAs.Xml:
                        goto Label_00D2;

                    case SettingsSerializeAs.Binary:
                    {
                        MemoryStream serializationStream = new MemoryStream();
                        try
                        {
                            new BinaryFormatter().Serialize(serializationStream, propValue);
                            return Convert.ToBase64String(serializationStream.ToArray());
                        }
                        finally
                        {
                            serializationStream.Close();
                        }
                        goto Label_00D2;
                    }
                    default:
                        goto Label_0105;
                }
                throw new ArgumentException(System.SR.GetString("Unable_to_convert_type_to_string", new object[] { type.ToString() }), "type");
            Label_00D2:
                serializer = new XmlSerializer(type);
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                serializer.Serialize((TextWriter) writer, propValue);
                return writer.ToString();
            }
            catch (Exception)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
        Label_0105:
            return null;
        }

        private object Deserialize()
        {
            object defaultValue = null;
            if (this.SerializedValue != null)
            {
                try
                {
                    if (this.SerializedValue is string)
                    {
                        defaultValue = GetObjectFromString(this.Property.PropertyType, this.Property.SerializeAs, (string) this.SerializedValue);
                    }
                    else
                    {
                        MemoryStream serializationStream = new MemoryStream((byte[]) this.SerializedValue);
                        try
                        {
                            defaultValue = new BinaryFormatter().Deserialize(serializationStream);
                        }
                        finally
                        {
                            serializationStream.Close();
                        }
                    }
                }
                catch (Exception exception)
                {
                    try
                    {
                        if (this.IsHostedInAspnet())
                        {
                            object[] args = new object[] { this.Property, this, exception };
                            Type.GetType("System.Web.Management.WebBaseEvent, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true).InvokeMember("RaisePropertyDeserializationWebErrorEvent", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, args, CultureInfo.InvariantCulture);
                        }
                    }
                    catch
                    {
                    }
                }
                if ((defaultValue != null) && !this.Property.PropertyType.IsAssignableFrom(defaultValue.GetType()))
                {
                    defaultValue = null;
                }
            }
            if (defaultValue == null)
            {
                this._UsingDefaultValue = true;
                if ((this.Property.DefaultValue == null) || (this.Property.DefaultValue.ToString() == "[null]"))
                {
                    if (this.Property.PropertyType.IsValueType)
                    {
                        return SecurityUtils.SecureCreateInstance(this.Property.PropertyType);
                    }
                    return null;
                }
                if (this.Property.DefaultValue is string)
                {
                    try
                    {
                        defaultValue = GetObjectFromString(this.Property.PropertyType, this.Property.SerializeAs, (string) this.Property.DefaultValue);
                    }
                    catch (Exception exception2)
                    {
                        throw new ArgumentException(System.SR.GetString("Could_not_create_from_default_value", new object[] { this.Property.Name, exception2.Message }));
                    }
                }
                else
                {
                    defaultValue = this.Property.DefaultValue;
                }
                if ((defaultValue != null) && !this.Property.PropertyType.IsAssignableFrom(defaultValue.GetType()))
                {
                    throw new ArgumentException(System.SR.GetString("Could_not_create_from_default_value_2", new object[] { this.Property.Name }));
                }
            }
            if (defaultValue == null)
            {
                if (this.Property.PropertyType == typeof(string))
                {
                    return "";
                }
                try
                {
                    defaultValue = SecurityUtils.SecureCreateInstance(this.Property.PropertyType);
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        private static object GetObjectFromString(Type type, SettingsSerializeAs serializeAs, string attValue)
        {
            StringReader reader;
            if ((type == typeof(string)) && (((attValue == null) || (attValue.Length < 1)) || (serializeAs == SettingsSerializeAs.String)))
            {
                return attValue;
            }
            if ((attValue != null) && (attValue.Length >= 1))
            {
                switch (serializeAs)
                {
                    case SettingsSerializeAs.String:
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(type);
                        if (((converter == null) || !converter.CanConvertTo(typeof(string))) || !converter.CanConvertFrom(typeof(string)))
                        {
                            throw new ArgumentException(System.SR.GetString("Unable_to_convert_type_from_string", new object[] { type.ToString() }), "type");
                        }
                        return converter.ConvertFromInvariantString(attValue);
                    }
                    case SettingsSerializeAs.Xml:
                        goto Label_0078;

                    case SettingsSerializeAs.Binary:
                    {
                        byte[] buffer = Convert.FromBase64String(attValue);
                        MemoryStream serializationStream = null;
                        try
                        {
                            serializationStream = new MemoryStream(buffer);
                            return new BinaryFormatter().Deserialize(serializationStream);
                        }
                        finally
                        {
                            if (serializationStream != null)
                            {
                                serializationStream.Close();
                            }
                        }
                        goto Label_0078;
                    }
                }
            }
            return null;
        Label_0078:
            reader = new StringReader(attValue);
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(reader);
        }

        private bool IsHostedInAspnet()
        {
            return (AppDomain.CurrentDomain.GetData(".appDomain") != null);
        }

        private object SerializePropertyValue()
        {
            object obj2;
            if (this._Value == null)
            {
                return null;
            }
            if (this.Property.SerializeAs != SettingsSerializeAs.Binary)
            {
                return ConvertObjectToString(this._Value, this.Property.PropertyType, this.Property.SerializeAs, this.Property.ThrowOnErrorSerializing);
            }
            MemoryStream serializationStream = new MemoryStream();
            try
            {
                new BinaryFormatter().Serialize(serializationStream, this._Value);
                obj2 = serializationStream.ToArray();
            }
            finally
            {
                serializationStream.Close();
            }
            return obj2;
        }

        public bool Deserialized
        {
            get
            {
                return this._Deserialized;
            }
            set
            {
                this._Deserialized = value;
            }
        }

        public bool IsDirty
        {
            get
            {
                return this._IsDirty;
            }
            set
            {
                this._IsDirty = value;
            }
        }

        public string Name
        {
            get
            {
                return this._Property.Name;
            }
        }

        public SettingsProperty Property
        {
            get
            {
                return this._Property;
            }
        }

        public object PropertyValue
        {
            get
            {
                if (!this._Deserialized)
                {
                    this._Value = this.Deserialize();
                    this._Deserialized = true;
                }
                if (((this._Value != null) && !this.Property.PropertyType.IsPrimitive) && (!(this._Value is string) && !(this._Value is DateTime)))
                {
                    this._UsingDefaultValue = false;
                    this._ChangedSinceLastSerialized = true;
                    this._IsDirty = true;
                }
                return this._Value;
            }
            set
            {
                this._Value = value;
                this._IsDirty = true;
                this._ChangedSinceLastSerialized = true;
                this._Deserialized = true;
                this._UsingDefaultValue = false;
            }
        }

        public object SerializedValue
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
            get
            {
                if (this._ChangedSinceLastSerialized)
                {
                    this._ChangedSinceLastSerialized = false;
                    this._SerializedValue = this.SerializePropertyValue();
                }
                return this._SerializedValue;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
            set
            {
                this._UsingDefaultValue = false;
                this._SerializedValue = value;
            }
        }

        public bool UsingDefaultValue
        {
            get
            {
                return this._UsingDefaultValue;
            }
        }
    }
}

