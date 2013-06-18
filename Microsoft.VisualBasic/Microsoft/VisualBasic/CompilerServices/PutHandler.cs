namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Reflection;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class PutHandler : IRecordEnum
    {
        public VB6File m_oFile;

        public PutHandler(VB6File oFile)
        {
            this.m_oFile = oFile;
        }

        public bool Callback(FieldInfo field_info, ref object vValue)
        {
            bool flag;
            string str;
            Type fieldType = field_info.FieldType;
            if (fieldType == null)
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Empty" })), 5);
            }
            if (fieldType.IsArray)
            {
                VBFixedArrayAttribute attribute;
                int fixedStringLength = -1;
                object[] objArray = field_info.GetCustomAttributes(typeof(VBFixedArrayAttribute), false);
                if ((objArray != null) && (objArray.Length != 0))
                {
                    attribute = (VBFixedArrayAttribute) objArray[0];
                }
                else
                {
                    attribute = null;
                }
                Type elementType = fieldType.GetElementType();
                if (elementType == typeof(string))
                {
                    objArray = field_info.GetCustomAttributes(typeof(VBFixedStringAttribute), false);
                    if ((objArray == null) || (objArray.Length == 0))
                    {
                        fixedStringLength = -1;
                    }
                    else
                    {
                        fixedStringLength = ((VBFixedStringAttribute) objArray[0]).Length;
                    }
                }
                if (attribute == null)
                {
                    this.m_oFile.PutDynamicArray(0L, (Array) vValue, false, fixedStringLength);
                    return flag;
                }
                this.m_oFile.PutFixedArray(0L, (Array) vValue, elementType, fixedStringLength, attribute.FirstBound, attribute.SecondBound);
                return flag;
            }
            switch (Type.GetTypeCode(fieldType))
            {
                case TypeCode.DBNull:
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "DBNull" })), 5);

                case TypeCode.Boolean:
                    this.m_oFile.PutBoolean(0L, BooleanType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Char:
                    this.m_oFile.PutChar(0L, Microsoft.VisualBasic.CompilerServices.CharType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Byte:
                    this.m_oFile.PutByte(0L, ByteType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Int16:
                    this.m_oFile.PutShort(0L, ShortType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Int32:
                    this.m_oFile.PutInteger(0L, IntegerType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Int64:
                    this.m_oFile.PutLong(0L, LongType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Single:
                    this.m_oFile.PutSingle(0L, SingleType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Double:
                    this.m_oFile.PutDouble(0L, DoubleType.FromObject(vValue), false);
                    return flag;

                case TypeCode.Decimal:
                    this.m_oFile.PutDecimal(0L, DecimalType.FromObject(vValue), false);
                    return flag;

                case TypeCode.DateTime:
                    this.m_oFile.PutDate(0L, DateType.FromObject(vValue), false);
                    return flag;

                case TypeCode.String:
                    if (vValue == null)
                    {
                        str = null;
                        break;
                    }
                    str = vValue.ToString();
                    break;

                default:
                    if (fieldType == typeof(object))
                    {
                        this.m_oFile.PutObject(vValue, 0L, true);
                        return flag;
                    }
                    if (fieldType == typeof(Exception))
                    {
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Exception" })), 5);
                    }
                    if (fieldType == typeof(Missing))
                    {
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Missing" })), 5);
                    }
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, fieldType.Name })), 5);
            }
            object[] customAttributes = field_info.GetCustomAttributes(typeof(VBFixedStringAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                this.m_oFile.PutStringWithLength(0L, str);
                return flag;
            }
            VBFixedStringAttribute attribute2 = (VBFixedStringAttribute) customAttributes[0];
            int length = attribute2.Length;
            if (length == 0)
            {
                length = -1;
            }
            this.m_oFile.PutFixedLengthString(0L, str, length);
            return flag;
        }
    }
}

