namespace System.Runtime.InteropServices
{
    using System;
    using System.Globalization;
    using System.Reflection;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8A7C1442-A9FB-366B-80D8-4939FFA6DBE0"), ComVisible(true), CLSCompliant(false), TypeLibImportClass(typeof(FieldInfo))]
    public interface _FieldInfo
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
        Type FieldType { get; }
        object GetValue(object obj);
        object GetValueDirect(TypedReference obj);
        void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);
        void SetValueDirect(TypedReference obj, object value);
        RuntimeFieldHandle FieldHandle { get; }
        FieldAttributes Attributes { get; }
        void SetValue(object obj, object value);
        bool IsPublic { get; }
        bool IsPrivate { get; }
        bool IsFamily { get; }
        bool IsAssembly { get; }
        bool IsFamilyAndAssembly { get; }
        bool IsFamilyOrAssembly { get; }
        bool IsStatic { get; }
        bool IsInitOnly { get; }
        bool IsLiteral { get; }
        bool IsNotSerialized { get; }
        bool IsSpecialName { get; }
        bool IsPinvokeImpl { get; }
    }
}

