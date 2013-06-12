namespace System.Runtime.InteropServices
{
    using System;
    using System.Globalization;
    using System.Reflection;

    [Guid("F59ED4E4-E68F-3218-BD77-061AA82824BF"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false), TypeLibImportClass(typeof(PropertyInfo))]
    public interface _PropertyInfo
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
        string ToString();
        bool Equals(object other);
        int GetHashCode();
        Type GetType();
        MemberTypes MemberType { get; }
        string Name { get; }
        Type DeclaringType { get; }
        Type ReflectedType { get; }
        object[] GetCustomAttributes(Type attributeType, bool inherit);
        object[] GetCustomAttributes(bool inherit);
        bool IsDefined(Type attributeType, bool inherit);
        Type PropertyType { get; }
        object GetValue(object obj, object[] index);
        object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
        void SetValue(object obj, object value, object[] index);
        void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
        MethodInfo[] GetAccessors(bool nonPublic);
        MethodInfo GetGetMethod(bool nonPublic);
        MethodInfo GetSetMethod(bool nonPublic);
        ParameterInfo[] GetIndexParameters();
        PropertyAttributes Attributes { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        MethodInfo[] GetAccessors();
        MethodInfo GetGetMethod();
        MethodInfo GetSetMethod();
        bool IsSpecialName { get; }
    }
}

