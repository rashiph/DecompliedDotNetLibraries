namespace System.Runtime.InteropServices
{
    using System;
    using System.Globalization;
    using System.Reflection;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("BCA8B44D-AAD6-3A86-8AB7-03349F4F2DA2"), CLSCompliant(false), TypeLibImportClass(typeof(Type))]
    public interface _Type
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
        Guid GUID { get; }
        System.Reflection.Module Module { get; }
        System.Reflection.Assembly Assembly { get; }
        RuntimeTypeHandle TypeHandle { get; }
        string FullName { get; }
        string Namespace { get; }
        string AssemblyQualifiedName { get; }
        int GetArrayRank();
        Type BaseType { get; }
        ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);
        Type GetInterface(string name, bool ignoreCase);
        Type[] GetInterfaces();
        Type[] FindInterfaces(TypeFilter filter, object filterCriteria);
        EventInfo GetEvent(string name, BindingFlags bindingAttr);
        EventInfo[] GetEvents();
        EventInfo[] GetEvents(BindingFlags bindingAttr);
        Type[] GetNestedTypes(BindingFlags bindingAttr);
        Type GetNestedType(string name, BindingFlags bindingAttr);
        MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr);
        MemberInfo[] GetDefaultMembers();
        MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria);
        Type GetElementType();
        bool IsSubclassOf(Type c);
        bool IsInstanceOfType(object o);
        bool IsAssignableFrom(Type c);
        InterfaceMapping GetInterfaceMap(Type interfaceType);
        MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers);
        MethodInfo GetMethod(string name, BindingFlags bindingAttr);
        MethodInfo[] GetMethods(BindingFlags bindingAttr);
        FieldInfo GetField(string name, BindingFlags bindingAttr);
        FieldInfo[] GetFields(BindingFlags bindingAttr);
        PropertyInfo GetProperty(string name, BindingFlags bindingAttr);
        PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers);
        PropertyInfo[] GetProperties(BindingFlags bindingAttr);
        MemberInfo[] GetMember(string name, BindingFlags bindingAttr);
        MemberInfo[] GetMembers(BindingFlags bindingAttr);
        object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters);
        Type UnderlyingSystemType { get; }
        object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, CultureInfo culture);
        object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args);
        ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);
        ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers);
        ConstructorInfo GetConstructor(Type[] types);
        ConstructorInfo[] GetConstructors();
        ConstructorInfo TypeInitializer { get; }
        MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);
        MethodInfo GetMethod(string name, Type[] types, ParameterModifier[] modifiers);
        MethodInfo GetMethod(string name, Type[] types);
        MethodInfo GetMethod(string name);
        MethodInfo[] GetMethods();
        FieldInfo GetField(string name);
        FieldInfo[] GetFields();
        Type GetInterface(string name);
        EventInfo GetEvent(string name);
        PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers);
        PropertyInfo GetProperty(string name, Type returnType, Type[] types);
        PropertyInfo GetProperty(string name, Type[] types);
        PropertyInfo GetProperty(string name, Type returnType);
        PropertyInfo GetProperty(string name);
        PropertyInfo[] GetProperties();
        Type[] GetNestedTypes();
        Type GetNestedType(string name);
        MemberInfo[] GetMember(string name);
        MemberInfo[] GetMembers();
        TypeAttributes Attributes { get; }
        bool IsNotPublic { get; }
        bool IsPublic { get; }
        bool IsNestedPublic { get; }
        bool IsNestedPrivate { get; }
        bool IsNestedFamily { get; }
        bool IsNestedAssembly { get; }
        bool IsNestedFamANDAssem { get; }
        bool IsNestedFamORAssem { get; }
        bool IsAutoLayout { get; }
        bool IsLayoutSequential { get; }
        bool IsExplicitLayout { get; }
        bool IsClass { get; }
        bool IsInterface { get; }
        bool IsValueType { get; }
        bool IsAbstract { get; }
        bool IsSealed { get; }
        bool IsEnum { get; }
        bool IsSpecialName { get; }
        bool IsImport { get; }
        bool IsSerializable { get; }
        bool IsAnsiClass { get; }
        bool IsUnicodeClass { get; }
        bool IsAutoClass { get; }
        bool IsArray { get; }
        bool IsByRef { get; }
        bool IsPointer { get; }
        bool IsPrimitive { get; }
        bool IsCOMObject { get; }
        bool HasElementType { get; }
        bool IsContextful { get; }
        bool IsMarshalByRef { get; }
        bool Equals(Type o);
    }
}

