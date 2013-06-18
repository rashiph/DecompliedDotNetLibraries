namespace System.Data
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    internal sealed class ColumnTypeConverter : TypeConverter
    {
        private static Type[] types = new Type[] { 
            typeof(bool), typeof(byte), typeof(byte[]), typeof(char), typeof(DateTime), typeof(decimal), typeof(double), typeof(Guid), typeof(short), typeof(int), typeof(long), typeof(object), typeof(sbyte), typeof(float), typeof(string), typeof(TimeSpan), 
            typeof(ushort), typeof(uint), typeof(ulong), typeof(SqlInt16), typeof(SqlInt32), typeof(SqlInt64), typeof(SqlDecimal), typeof(SqlSingle), typeof(SqlDouble), typeof(SqlString), typeof(SqlBoolean), typeof(SqlBinary), typeof(SqlByte), typeof(SqlDateTime), typeof(SqlGuid), typeof(SqlMoney), 
            typeof(SqlBytes), typeof(SqlChars), typeof(SqlXml)
         };
        private TypeConverter.StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertTo(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if ((value == null) || !(value.GetType() == typeof(string)))
            {
                return base.ConvertFrom(context, culture, value);
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].ToString().Equals(value))
                {
                    return types[i];
                }
            }
            return typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == typeof(string))
            {
                if (value == null)
                {
                    return string.Empty;
                }
                value.ToString();
            }
            if ((value != null) && (destinationType == typeof(InstanceDescriptor)))
            {
                object obj2 = value;
                if (value is string)
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (types[i].ToString().Equals(value))
                        {
                            obj2 = types[i];
                        }
                    }
                }
                if ((value is Type) || (value is string))
                {
                    MethodInfo method = typeof(Type).GetMethod("GetType", new Type[] { typeof(string) });
                    if (method != null)
                    {
                        return new InstanceDescriptor(method, new object[] { ((Type) obj2).AssemblyQualifiedName });
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                object[] objArray;
                if (types != null)
                {
                    objArray = new object[types.Length];
                    Array.Copy(types, objArray, types.Length);
                }
                else
                {
                    objArray = null;
                }
                this.values = new TypeConverter.StandardValuesCollection(objArray);
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

