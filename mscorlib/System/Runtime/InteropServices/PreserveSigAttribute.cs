namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [ComVisible(true), AttributeUsage(AttributeTargets.Method, Inherited=false)]
    public sealed class PreserveSigAttribute : Attribute
    {
        internal static Attribute GetCustomAttribute(RuntimeMethodInfo method)
        {
            if ((method.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) == MethodImplAttributes.IL)
            {
                return null;
            }
            return new PreserveSigAttribute();
        }

        internal static bool IsDefined(RuntimeMethodInfo method)
        {
            return ((method.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != MethodImplAttributes.IL);
        }
    }
}

