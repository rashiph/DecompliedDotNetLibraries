namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Guid("AFBF15E5-C37C-11d2-B88E-00A0C9B471B8")]
    internal interface IReflect
    {
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
    }
}

