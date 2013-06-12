namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct CustomAttributeTypedArgument
    {
        private object m_value;
        private Type m_argumentType;
        public static bool operator ==(CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
        {
            return !left.Equals(right);
        }

        private static Type CustomAttributeEncodingToType(CustomAttributeEncoding encodedType)
        {
            switch (encodedType)
            {
                case CustomAttributeEncoding.Boolean:
                    return typeof(bool);

                case CustomAttributeEncoding.Char:
                    return typeof(char);

                case CustomAttributeEncoding.SByte:
                    return typeof(sbyte);

                case CustomAttributeEncoding.Byte:
                    return typeof(byte);

                case CustomAttributeEncoding.Int16:
                    return typeof(short);

                case CustomAttributeEncoding.UInt16:
                    return typeof(ushort);

                case CustomAttributeEncoding.Int32:
                    return typeof(int);

                case CustomAttributeEncoding.UInt32:
                    return typeof(uint);

                case CustomAttributeEncoding.Int64:
                    return typeof(long);

                case CustomAttributeEncoding.UInt64:
                    return typeof(ulong);

                case CustomAttributeEncoding.Float:
                    return typeof(float);

                case CustomAttributeEncoding.Double:
                    return typeof(double);

                case CustomAttributeEncoding.String:
                    return typeof(string);

                case CustomAttributeEncoding.Array:
                    return typeof(Array);

                case CustomAttributeEncoding.Type:
                    return typeof(Type);

                case CustomAttributeEncoding.Object:
                    return typeof(object);

                case CustomAttributeEncoding.Enum:
                    return typeof(Enum);
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) encodedType }), "encodedType");
        }

        [SecuritySafeCritical]
        private static unsafe object EncodedValueToRawValue(long val, CustomAttributeEncoding encodedType)
        {
            switch (encodedType)
            {
                case CustomAttributeEncoding.Boolean:
                    return (((byte) val) != 0);

                case CustomAttributeEncoding.Char:
                    return (char) ((ushort) val);

                case CustomAttributeEncoding.SByte:
                    return (sbyte) val;

                case CustomAttributeEncoding.Byte:
                    return (byte) val;

                case CustomAttributeEncoding.Int16:
                    return (short) val;

                case CustomAttributeEncoding.UInt16:
                    return (ushort) val;

                case CustomAttributeEncoding.Int32:
                    return (int) val;

                case CustomAttributeEncoding.UInt32:
                    return (uint) val;

                case CustomAttributeEncoding.Int64:
                    return val;

                case CustomAttributeEncoding.UInt64:
                    return (ulong) val;

                case CustomAttributeEncoding.Float:
                    return *(((float*) &val));

                case CustomAttributeEncoding.Double:
                    return *(((double*) &val));
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) val }), "val");
        }

        private static RuntimeType ResolveType(RuntimeModule scope, string typeName)
        {
            RuntimeType typeByNameUsingCARules = RuntimeTypeHandle.GetTypeByNameUsingCARules(typeName, scope);
            if (typeByNameUsingCARules == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_CATypeResolutionFailed"), new object[] { typeName }));
            }
            return typeByNameUsingCARules;
        }

        public CustomAttributeTypedArgument(Type argumentType, object value)
        {
            if (argumentType == null)
            {
                throw new ArgumentNullException("argumentType");
            }
            this.m_value = (value == null) ? null : CanonicalizeValue(value);
            this.m_argumentType = argumentType;
        }

        public CustomAttributeTypedArgument(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.m_value = CanonicalizeValue(value);
            this.m_argumentType = value.GetType();
        }

        private static object CanonicalizeValue(object value)
        {
            if (value.GetType().IsEnum)
            {
                return ((Enum) value).GetValue();
            }
            return value;
        }

        [SecuritySafeCritical]
        internal CustomAttributeTypedArgument(RuntimeModule scope, CustomAttributeEncodedArgument encodedArg)
        {
            CustomAttributeEncoding encodedType = encodedArg.CustomAttributeType.EncodedType;
            switch (encodedType)
            {
                case CustomAttributeEncoding.Undefined:
                    throw new ArgumentException("encodedArg");

                case CustomAttributeEncoding.Enum:
                    this.m_argumentType = ResolveType(scope, encodedArg.CustomAttributeType.EnumName);
                    this.m_value = EncodedValueToRawValue(encodedArg.PrimitiveValue, encodedArg.CustomAttributeType.EncodedEnumType);
                    return;

                case CustomAttributeEncoding.String:
                    this.m_argumentType = typeof(string);
                    this.m_value = encodedArg.StringValue;
                    return;

                case CustomAttributeEncoding.Type:
                    this.m_argumentType = typeof(Type);
                    this.m_value = null;
                    if (encodedArg.StringValue != null)
                    {
                        this.m_value = ResolveType(scope, encodedArg.StringValue);
                        return;
                    }
                    break;

                case CustomAttributeEncoding.Array:
                {
                    Type type;
                    encodedType = encodedArg.CustomAttributeType.EncodedArrayType;
                    if (encodedType == CustomAttributeEncoding.Enum)
                    {
                        type = ResolveType(scope, encodedArg.CustomAttributeType.EnumName);
                    }
                    else
                    {
                        type = CustomAttributeEncodingToType(encodedType);
                    }
                    this.m_argumentType = type.MakeArrayType();
                    if (encodedArg.ArrayValue == null)
                    {
                        this.m_value = null;
                        return;
                    }
                    CustomAttributeTypedArgument[] array = new CustomAttributeTypedArgument[encodedArg.ArrayValue.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = new CustomAttributeTypedArgument(scope, encodedArg.ArrayValue[i]);
                    }
                    this.m_value = Array.AsReadOnly<CustomAttributeTypedArgument>(array);
                    return;
                }
                default:
                    this.m_argumentType = CustomAttributeEncodingToType(encodedType);
                    this.m_value = EncodedValueToRawValue(encodedArg.PrimitiveValue, encodedType);
                    break;
            }
        }

        public override string ToString()
        {
            return this.ToString(false);
        }

        [SecuritySafeCritical]
        internal string ToString(bool typed)
        {
            if (this.ArgumentType.IsEnum)
            {
                return string.Format(CultureInfo.CurrentCulture, typed ? "{0}" : "({1}){0}", new object[] { this.Value, this.ArgumentType.FullName });
            }
            if (this.Value == null)
            {
                return string.Format(CultureInfo.CurrentCulture, typed ? "null" : "({0})null", new object[] { this.ArgumentType.Name });
            }
            if (this.ArgumentType == typeof(string))
            {
                return string.Format(CultureInfo.CurrentCulture, "\"{0}\"", new object[] { this.Value });
            }
            if (this.ArgumentType == typeof(char))
            {
                return string.Format(CultureInfo.CurrentCulture, "'{0}'", new object[] { this.Value });
            }
            if (this.ArgumentType == typeof(Type))
            {
                return string.Format(CultureInfo.CurrentCulture, "typeof({0})", new object[] { ((Type) this.Value).FullName });
            }
            if (this.ArgumentType.IsArray)
            {
                string str = null;
                IList<CustomAttributeTypedArgument> list = this.Value as IList<CustomAttributeTypedArgument>;
                Type elementType = this.ArgumentType.GetElementType();
                str = string.Format(CultureInfo.CurrentCulture, "new {0}[{1}] {{ ", new object[] { elementType.IsEnum ? elementType.FullName : elementType.Name, list.Count });
                for (int i = 0; i < list.Count; i++)
                {
                    object[] args = new object[] { list[i].ToString(elementType != typeof(object)) };
                    str = str + string.Format(CultureInfo.CurrentCulture, (i == 0) ? "{0}" : ", {0}", args);
                }
                return (str = str + " }");
            }
            return string.Format(CultureInfo.CurrentCulture, typed ? "{0}" : "({1}){0}", new object[] { this.Value, this.ArgumentType.Name });
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj == this);
        }

        public Type ArgumentType
        {
            get
            {
                return this.m_argumentType;
            }
        }
        public object Value
        {
            get
            {
                return this.m_value;
            }
        }
    }
}

