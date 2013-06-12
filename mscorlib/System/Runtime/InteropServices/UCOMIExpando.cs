namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [Guid("AFBF15E6-C37C-11d2-B88E-00A0C9B471B8"), Obsolete("Use System.Runtime.InteropServices.ComTypes.IExpando instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    internal interface UCOMIExpando : UCOMIReflect
    {
        FieldInfo AddField(string name);
        PropertyInfo AddProperty(string name);
        MethodInfo AddMethod(string name, Delegate method);
        void RemoveMember(MemberInfo m);
    }
}

