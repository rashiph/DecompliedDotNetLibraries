namespace System.Reflection.Emit
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [ClassInterface(ClassInterfaceType.None), ComVisible(true), ComDefaultInterface(typeof(_CustomAttributeBuilder)), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class CustomAttributeBuilder : _CustomAttributeBuilder
    {
        internal byte[] m_blob;
        internal ConstructorInfo m_con;
        internal object[] m_constructorArgs;

        public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs)
        {
            this.InitCustomAttributeBuilder(con, constructorArgs, new PropertyInfo[0], new object[0], new FieldInfo[0], new object[0]);
        }

        public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, FieldInfo[] namedFields, object[] fieldValues)
        {
            this.InitCustomAttributeBuilder(con, constructorArgs, new PropertyInfo[0], new object[0], namedFields, fieldValues);
        }

        public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
        {
            this.InitCustomAttributeBuilder(con, constructorArgs, namedProperties, propertyValues, new FieldInfo[0], new object[0]);
        }

        public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
        {
            this.InitCustomAttributeBuilder(con, constructorArgs, namedProperties, propertyValues, namedFields, fieldValues);
        }

        [SecurityCritical]
        internal void CreateCustomAttribute(ModuleBuilder mod, int tkOwner)
        {
            this.CreateCustomAttribute(mod, tkOwner, mod.GetConstructorToken(this.m_con).Token, false);
        }

        [SecurityCritical]
        internal void CreateCustomAttribute(ModuleBuilder mod, int tkOwner, int tkAttrib, bool toDisk)
        {
            TypeBuilder.DefineCustomAttribute(mod, tkOwner, tkAttrib, this.m_blob, toDisk, typeof(DebuggableAttribute) == this.m_con.DeclaringType);
        }

        private void EmitString(BinaryWriter writer, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            uint length = (uint) bytes.Length;
            if (length <= 0x7f)
            {
                writer.Write((byte) length);
            }
            else if (length <= 0x3fff)
            {
                writer.Write((byte) ((length >> 8) | 0x80));
                writer.Write((byte) (length & 0xff));
            }
            else
            {
                writer.Write((byte) ((length >> 0x18) | 0xc0));
                writer.Write((byte) ((length >> 0x10) & 0xff));
                writer.Write((byte) ((length >> 8) & 0xff));
                writer.Write((byte) (length & 0xff));
            }
            writer.Write(bytes);
        }

        private void EmitType(BinaryWriter writer, Type type)
        {
            if (type.IsPrimitive)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        writer.Write((byte) 2);
                        return;

                    case TypeCode.Char:
                        writer.Write((byte) 3);
                        return;

                    case TypeCode.SByte:
                        writer.Write((byte) 4);
                        return;

                    case TypeCode.Byte:
                        writer.Write((byte) 5);
                        return;

                    case TypeCode.Int16:
                        writer.Write((byte) 6);
                        return;

                    case TypeCode.UInt16:
                        writer.Write((byte) 7);
                        return;

                    case TypeCode.Int32:
                        writer.Write((byte) 8);
                        return;

                    case TypeCode.UInt32:
                        writer.Write((byte) 9);
                        return;

                    case TypeCode.Int64:
                        writer.Write((byte) 10);
                        return;

                    case TypeCode.UInt64:
                        writer.Write((byte) 11);
                        return;

                    case TypeCode.Single:
                        writer.Write((byte) 12);
                        return;

                    case TypeCode.Double:
                        writer.Write((byte) 13);
                        return;
                }
            }
            else if (type.IsEnum)
            {
                writer.Write((byte) 0x55);
                this.EmitString(writer, type.AssemblyQualifiedName);
            }
            else if (type == typeof(string))
            {
                writer.Write((byte) 14);
            }
            else if (type == typeof(Type))
            {
                writer.Write((byte) 80);
            }
            else if (type.IsArray)
            {
                writer.Write((byte) 0x1d);
                this.EmitType(writer, type.GetElementType());
            }
            else
            {
                writer.Write((byte) 0x51);
            }
        }

        private void EmitValue(BinaryWriter writer, Type type, object value)
        {
            if (type.IsEnum)
            {
                switch (Type.GetTypeCode(Enum.GetUnderlyingType(type)))
                {
                    case TypeCode.SByte:
                        writer.Write((sbyte) value);
                        return;

                    case TypeCode.Byte:
                        writer.Write((byte) value);
                        return;

                    case TypeCode.Int16:
                        writer.Write((short) value);
                        return;

                    case TypeCode.UInt16:
                        writer.Write((ushort) value);
                        return;

                    case TypeCode.Int32:
                        writer.Write((int) value);
                        return;

                    case TypeCode.UInt32:
                        writer.Write((uint) value);
                        return;

                    case TypeCode.Int64:
                        writer.Write((long) value);
                        return;

                    case TypeCode.UInt64:
                        writer.Write((ulong) value);
                        return;
                }
            }
            else if (type == typeof(string))
            {
                if (value == null)
                {
                    writer.Write((byte) 0xff);
                }
                else
                {
                    this.EmitString(writer, (string) value);
                }
            }
            else if (type == typeof(Type))
            {
                if (value == null)
                {
                    writer.Write((byte) 0xff);
                }
                else
                {
                    string str = TypeNameBuilder.ToString((Type) value, TypeNameBuilder.Format.AssemblyQualifiedName);
                    if (str == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidTypeForCA", new object[] { value.GetType() }));
                    }
                    this.EmitString(writer, str);
                }
            }
            else if (type.IsArray)
            {
                if (value == null)
                {
                    writer.Write(uint.MaxValue);
                }
                else
                {
                    Array array = (Array) value;
                    Type elementType = type.GetElementType();
                    writer.Write(array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        this.EmitValue(writer, elementType, array.GetValue(i));
                    }
                }
            }
            else if (type.IsPrimitive)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        writer.Write(((bool) value) ? ((byte) 1) : ((byte) 0));
                        return;

                    case TypeCode.Char:
                        writer.Write(Convert.ToUInt16((char) value));
                        return;

                    case TypeCode.SByte:
                        writer.Write((sbyte) value);
                        return;

                    case TypeCode.Byte:
                        writer.Write((byte) value);
                        return;

                    case TypeCode.Int16:
                        writer.Write((short) value);
                        return;

                    case TypeCode.UInt16:
                        writer.Write((ushort) value);
                        return;

                    case TypeCode.Int32:
                        writer.Write((int) value);
                        return;

                    case TypeCode.UInt32:
                        writer.Write((uint) value);
                        return;

                    case TypeCode.Int64:
                        writer.Write((long) value);
                        return;

                    case TypeCode.UInt64:
                        writer.Write((ulong) value);
                        return;

                    case TypeCode.Single:
                        writer.Write((float) value);
                        return;

                    case TypeCode.Double:
                        writer.Write((double) value);
                        return;
                }
            }
            else if (type == typeof(object))
            {
                Type type3 = (value == null) ? typeof(string) : ((value is Type) ? typeof(Type) : value.GetType());
                if (type3 == typeof(object))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadParameterTypeForCAB", new object[] { type3.ToString() }));
                }
                this.EmitType(writer, type3);
                this.EmitValue(writer, type3, value);
            }
            else
            {
                string str2 = "null";
                if (value != null)
                {
                    str2 = value.GetType().ToString();
                }
                throw new ArgumentException(Environment.GetResourceString("Argument_BadParameterTypeForCAB", new object[] { str2 }));
            }
        }

        internal void InitCustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
        {
            int num;
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }
            if (constructorArgs == null)
            {
                throw new ArgumentNullException("constructorArgs");
            }
            if (namedProperties == null)
            {
                throw new ArgumentNullException("namedProperties");
            }
            if (propertyValues == null)
            {
                throw new ArgumentNullException("propertyValues");
            }
            if (namedFields == null)
            {
                throw new ArgumentNullException("namedFields");
            }
            if (fieldValues == null)
            {
                throw new ArgumentNullException("fieldValues");
            }
            if (namedProperties.Length != propertyValues.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ArrayLengthsDiffer"), "namedProperties, propertyValues");
            }
            if (namedFields.Length != fieldValues.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ArrayLengthsDiffer"), "namedFields, fieldValues");
            }
            if (((con.Attributes & MethodAttributes.Static) == MethodAttributes.Static) || ((con.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadConstructor"));
            }
            if ((con.CallingConvention & CallingConventions.Standard) != CallingConventions.Standard)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadConstructorCallConv"));
            }
            this.m_con = con;
            this.m_constructorArgs = new object[constructorArgs.Length];
            Array.Copy(constructorArgs, this.m_constructorArgs, constructorArgs.Length);
            Type[] parameterTypes = con.GetParameterTypes();
            if (parameterTypes.Length != constructorArgs.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadParameterCountsForConstructor"));
            }
            for (num = 0; num < parameterTypes.Length; num++)
            {
                if (!this.ValidateType(parameterTypes[num]))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeInCustomAttribute"));
                }
            }
            for (num = 0; num < parameterTypes.Length; num++)
            {
                if (constructorArgs[num] != null)
                {
                    TypeCode typeCode = Type.GetTypeCode(parameterTypes[num]);
                    if ((typeCode != Type.GetTypeCode(constructorArgs[num].GetType())) && ((typeCode != TypeCode.Object) || !this.ValidateType(constructorArgs[num].GetType())))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_BadParameterTypeForConstructor", new object[] { num }));
                    }
                }
            }
            MemoryStream output = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(output);
            writer.Write((ushort) 1);
            for (num = 0; num < constructorArgs.Length; num++)
            {
                this.EmitValue(writer, parameterTypes[num], constructorArgs[num]);
            }
            writer.Write((ushort) (namedProperties.Length + namedFields.Length));
            for (num = 0; num < namedProperties.Length; num++)
            {
                if (namedProperties[num] == null)
                {
                    throw new ArgumentNullException("namedProperties[" + num + "]");
                }
                Type propertyType = namedProperties[num].PropertyType;
                if ((propertyValues[num] == null) && propertyType.IsPrimitive)
                {
                    throw new ArgumentNullException("propertyValues[" + num + "]");
                }
                if (!this.ValidateType(propertyType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeInCustomAttribute"));
                }
                if (!namedProperties[num].CanWrite)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NotAWritableProperty"));
                }
                if ((((namedProperties[num].DeclaringType != con.DeclaringType) && !(con.DeclaringType is TypeBuilderInstantiation)) && (!con.DeclaringType.IsSubclassOf(namedProperties[num].DeclaringType) && !TypeBuilder.IsTypeEqual(namedProperties[num].DeclaringType, con.DeclaringType))) && (!(namedProperties[num].DeclaringType is TypeBuilder) || !con.DeclaringType.IsSubclassOf(((TypeBuilder) namedProperties[num].DeclaringType).m_runtimeType)))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadPropertyForConstructorBuilder"));
                }
                if (((propertyValues[num] != null) && (propertyType != typeof(object))) && (Type.GetTypeCode(propertyValues[num].GetType()) != Type.GetTypeCode(propertyType)))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
                }
                writer.Write((byte) 0x54);
                this.EmitType(writer, propertyType);
                this.EmitString(writer, namedProperties[num].Name);
                this.EmitValue(writer, propertyType, propertyValues[num]);
            }
            for (num = 0; num < namedFields.Length; num++)
            {
                if (namedFields[num] == null)
                {
                    throw new ArgumentNullException("namedFields[" + num + "]");
                }
                Type fieldType = namedFields[num].FieldType;
                if ((fieldValues[num] == null) && fieldType.IsPrimitive)
                {
                    throw new ArgumentNullException("fieldValues[" + num + "]");
                }
                if (!this.ValidateType(fieldType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeInCustomAttribute"));
                }
                if ((((namedFields[num].DeclaringType != con.DeclaringType) && !(con.DeclaringType is TypeBuilderInstantiation)) && (!con.DeclaringType.IsSubclassOf(namedFields[num].DeclaringType) && !TypeBuilder.IsTypeEqual(namedFields[num].DeclaringType, con.DeclaringType))) && (!(namedFields[num].DeclaringType is TypeBuilder) || !con.DeclaringType.IsSubclassOf(((TypeBuilder) namedFields[num].DeclaringType).m_runtimeType)))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadFieldForConstructorBuilder"));
                }
                if (((fieldValues[num] != null) && (fieldType != typeof(object))) && (Type.GetTypeCode(fieldValues[num].GetType()) != Type.GetTypeCode(fieldType)))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
                }
                writer.Write((byte) 0x53);
                this.EmitType(writer, fieldType);
                this.EmitString(writer, namedFields[num].Name);
                this.EmitValue(writer, fieldType, fieldValues[num]);
            }
            this.m_blob = ((MemoryStream) writer.BaseStream).ToArray();
        }

        [SecurityCritical]
        internal int PrepareCreateCustomAttributeToDisk(ModuleBuilder mod)
        {
            return mod.InternalGetConstructorToken(this.m_con, true).Token;
        }

        void _CustomAttributeBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _CustomAttributeBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _CustomAttributeBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _CustomAttributeBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        private bool ValidateType(Type t)
        {
            if ((t.IsPrimitive || (t == typeof(string))) || (t == typeof(Type)))
            {
                return true;
            }
            if (t.IsEnum)
            {
                switch (Type.GetTypeCode(Enum.GetUnderlyingType(t)))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
                return false;
            }
            if (!t.IsArray)
            {
                return (t == typeof(object));
            }
            if (t.GetArrayRank() != 1)
            {
                return false;
            }
            return this.ValidateType(t.GetElementType());
        }
    }
}

