namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class StructUtils
    {
        private StructUtils()
        {
        }

        internal static object EnumerateUDT(ValueType oStruct, IRecordEnum intfRecEnum, bool fGet)
        {
            Type typ = oStruct.GetType();
            if ((Information.VarTypeFromComType(typ) != VariantType.UserDefinedType) || typ.IsPrimitive)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "oStruct" }));
            }
            FieldInfo[] fields = typ.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int num2 = 0;
            int num4 = fields.GetUpperBound(0);
            for (int i = num2; i <= num4; i++)
            {
                FieldInfo fieldInfo = fields[i];
                Type fieldType = fieldInfo.FieldType;
                object obj3 = fieldInfo.GetValue(oStruct);
                if (Information.VarTypeFromComType(fieldType) == VariantType.UserDefinedType)
                {
                    if (fieldType.IsPrimitive)
                    {
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { fieldInfo.Name, fieldType.Name })), 5);
                    }
                    EnumerateUDT((ValueType) obj3, intfRecEnum, fGet);
                }
                else
                {
                    intfRecEnum.Callback(fieldInfo, ref obj3);
                }
                if (fGet)
                {
                    fieldInfo.SetValue(oStruct, obj3);
                }
            }
            return null;
        }

        internal static int GetRecordLength(object o, int PackSize = -1)
        {
            if (o == null)
            {
                return 0;
            }
            StructByteLengthHandler handler = new StructByteLengthHandler(PackSize);
            IRecordEnum intfRecEnum = handler;
            if (intfRecEnum == null)
            {
                throw ExceptionUtils.VbMakeException(5);
            }
            EnumerateUDT((ValueType) o, intfRecEnum, false);
            return handler.Length;
        }

        private sealed class StructByteLengthHandler : IRecordEnum
        {
            private int m_PackSize;
            private int m_StructLength;

            internal StructByteLengthHandler(int PackSize)
            {
                this.m_PackSize = PackSize;
            }

            internal bool Callback(FieldInfo field_info, ref object vValue)
            {
                int num;
                int num2;
                Type fieldType = field_info.FieldType;
                if (fieldType == null)
                {
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Empty" })), 5);
                }
                if (fieldType.IsArray)
                {
                    VBFixedArrayAttribute attribute;
                    int length;
                    int num4;
                    object[] customAttributes = field_info.GetCustomAttributes(typeof(VBFixedArrayAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length != 0))
                    {
                        attribute = (VBFixedArrayAttribute) customAttributes[0];
                    }
                    else
                    {
                        attribute = null;
                    }
                    Type elementType = fieldType.GetElementType();
                    if (attribute == null)
                    {
                        length = 1;
                        num4 = 4;
                    }
                    else
                    {
                        length = attribute.Length;
                        this.GetFieldSize(field_info, elementType, ref num, ref num4);
                    }
                    this.SetAlignment(num);
                    this.m_StructLength += length * num4;
                    return false;
                }
                this.GetFieldSize(field_info, fieldType, ref num, ref num2);
                this.SetAlignment(num);
                this.m_StructLength += num2;
                return false;
            }

            private void GetFieldSize(FieldInfo field_info, Type FieldType, ref int align, ref int size)
            {
                switch (Type.GetTypeCode(FieldType))
                {
                    case TypeCode.DBNull:
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "DBNull" })), 5);

                    case TypeCode.Boolean:
                        align = 2;
                        size = 2;
                        break;

                    case TypeCode.Char:
                        align = 2;
                        size = 2;
                        break;

                    case TypeCode.Byte:
                        align = 1;
                        size = 1;
                        break;

                    case TypeCode.Int16:
                        align = 2;
                        size = 2;
                        break;

                    case TypeCode.Int32:
                        align = 4;
                        size = 4;
                        break;

                    case TypeCode.Int64:
                        align = 8;
                        size = 8;
                        break;

                    case TypeCode.Single:
                        align = 4;
                        size = 4;
                        break;

                    case TypeCode.Double:
                        align = 8;
                        size = 8;
                        break;

                    case TypeCode.Decimal:
                        align = 0x10;
                        size = 0x10;
                        break;

                    case TypeCode.DateTime:
                        align = 8;
                        size = 8;
                        break;

                    case TypeCode.String:
                    {
                        object[] customAttributes = field_info.GetCustomAttributes(typeof(VBFixedStringAttribute), false);
                        if ((customAttributes != null) && (customAttributes.Length != 0))
                        {
                            VBFixedStringAttribute attribute = (VBFixedStringAttribute) customAttributes[0];
                            int length = attribute.Length;
                            if (length == 0)
                            {
                                length = -1;
                            }
                            size = length;
                            break;
                        }
                        align = 4;
                        size = 4;
                        break;
                    }
                }
                if (FieldType == typeof(Exception))
                {
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Exception" })), 5);
                }
                if (FieldType == typeof(Missing))
                {
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Missing" })), 5);
                }
                if (FieldType == typeof(object))
                {
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Object" })), 5);
                }
            }

            internal void SetAlignment(int size)
            {
                if (this.m_PackSize != 1)
                {
                    this.m_StructLength += this.m_StructLength % size;
                }
            }

            internal int Length
            {
                get
                {
                    if (this.m_PackSize == 1)
                    {
                        return this.m_StructLength;
                    }
                    return (this.m_StructLength + (this.m_StructLength % this.m_PackSize));
                }
            }
        }
    }
}

