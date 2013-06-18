namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("AA51516D-C0F2-49fe-9D38-61D20456904C")]
    public interface IDebugConvert
    {
        object ToPrimitive(object value, TypeCode typeCode, bool truncationPermitted);
        string ByteToString(byte value, int radix);
        string SByteToString(sbyte value, int radix);
        string Int16ToString(short value, int radix);
        string UInt16ToString(ushort value, int radix);
        string Int32ToString(int value, int radix);
        string UInt32ToString(uint value, int radix);
        string Int64ToString(long value, int radix);
        string UInt64ToString(ulong value, int radix);
        string SingleToString(float value);
        string DoubleToString(double value);
        string BooleanToString(bool value);
        string DoubleToDateString(double value);
        string RegexpToString(string source, bool ignoreCase, bool global, bool multiline);
        string StringToPrintable(string source);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetManagedObject(object value);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetManagedInt64Object(long i);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetManagedUInt64Object(ulong i);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetManagedCharObject(ushort i);
        string GetErrorMessageForHR(int hr, IJSVsaEngine engine);
    }
}

