namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeLibImportClass(typeof(EventInfo)), ComVisible(true), Guid("9DE59C64-D889-35A1-B897-587D74469E5B"), CLSCompliant(false)]
    public interface _EventInfo
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
        MethodInfo GetAddMethod(bool nonPublic);
        MethodInfo GetRemoveMethod(bool nonPublic);
        MethodInfo GetRaiseMethod(bool nonPublic);
        EventAttributes Attributes { get; }
        MethodInfo GetAddMethod();
        MethodInfo GetRemoveMethod();
        MethodInfo GetRaiseMethod();
        void AddEventHandler(object target, Delegate handler);
        void RemoveEventHandler(object target, Delegate handler);
        Type EventHandlerType { get; }
        bool IsSpecialName { get; }
        bool IsMulticast { get; }
    }
}

