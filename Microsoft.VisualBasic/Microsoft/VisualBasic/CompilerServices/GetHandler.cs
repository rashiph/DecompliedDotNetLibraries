namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Reflection;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class GetHandler : IRecordEnum
    {
        private VB6File m_oFile;

        public GetHandler(VB6File oFile)
        {
            this.m_oFile = oFile;
        }

        public bool Callback(FieldInfo field_info, ref object vValue)
        {
            bool flag;
            Type fieldType = field_info.FieldType;
            if (fieldType == null)
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "Empty" })), 5);
            }
            if (fieldType.IsArray)
            {
                object[] customAttributes = field_info.GetCustomAttributes(typeof(VBFixedArrayAttribute), false);
                Array arr = null;
                int fixedStringLength = -1;
                object[] objArray2 = field_info.GetCustomAttributes(typeof(VBFixedStringAttribute), false);
                if ((objArray2 != null) && (objArray2.Length > 0))
                {
                    VBFixedStringAttribute attribute = (VBFixedStringAttribute) objArray2[0];
                    if (attribute.Length > 0)
                    {
                        fixedStringLength = attribute.Length;
                    }
                }
                if ((customAttributes == null) || (customAttributes.Length == 0))
                {
                    this.m_oFile.GetDynamicArray(ref arr, fieldType.GetElementType(), fixedStringLength);
                }
                else
                {
                    VBFixedArrayAttribute attribute2 = (VBFixedArrayAttribute) customAttributes[0];
                    int firstBound = attribute2.FirstBound;
                    int secondBound = attribute2.SecondBound;
                    arr = (Array) vValue;
                    this.m_oFile.GetFixedArray(0L, ref arr, fieldType.GetElementType(), firstBound, secondBound, fixedStringLength);
                }
                vValue = arr;
                return flag;
            }
            switch (Type.GetTypeCode(fieldType))
            {
                case TypeCode.DBNull:
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", new string[] { field_info.Name, "DBNull" })), 5);

                case TypeCode.Boolean:
                    vValue = this.m_oFile.GetBoolean(0L);
                    return flag;

                case TypeCode.Char:
                    vValue = this.m_oFile.GetChar(0L);
                    return flag;

                case TypeCode.Byte:
                    vValue = this.m_oFile.GetByte(0L);
                    return flag;

                case TypeCode.Int16:
                    vValue = this.m_oFile.GetShort(0L);
                    return flag;

                case TypeCode.Int32:
                    vValue = this.m_oFile.GetInteger(0L);
                    return flag;

                case TypeCode.Int64:
                    vValue = this.m_oFile.GetLong(0L);
                    return flag;

                case TypeCode.Single:
                    vValue = this.m_oFile.GetSingle(0L);
                    return flag;

                case TypeCode.Double:
                    vValue = this.m_oFile.GetDouble(0L);
                    return flag;

                case TypeCode.Decimal:
                    vValue = this.m_oFile.GetDecimal(0L);
                    return flag;

                case TypeCode.DateTime:
                    vValue = this.m_oFile.GetDate(0L);
                    return flag;

                case TypeCode.String:
                {
                    object[] objArray3 = field_info.GetCustomAttributes(typeof(VBFixedStringAttribute), false);
                    if ((objArray3 != null) && (objArray3.Length != 0))
                    {
                        VBFixedStringAttribute attribute3 = (VBFixedStringAttribute) objArray3[0];
                        int length = attribute3.Length;
                        if (length == 0)
                        {
                            length = -1;
                        }
                        vValue = this.m_oFile.GetFixedLengthString(0L, length);
                        return flag;
                    }
                    vValue = this.m_oFile.GetLengthPrefixedString(0L);
                    return flag;
                }
            }
            if (fieldType == typeof(object))
            {
                this.m_oFile.GetObject(ref vValue, 0L, true);
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
    }
}

