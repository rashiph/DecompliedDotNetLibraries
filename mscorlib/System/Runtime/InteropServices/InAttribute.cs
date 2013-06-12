namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [ComVisible(true), AttributeUsage(AttributeTargets.Parameter, Inherited=false)]
    public sealed class InAttribute : Attribute
    {
        internal static Attribute GetCustomAttribute(RuntimeParameterInfo parameter)
        {
            if (!parameter.IsIn)
            {
                return null;
            }
            return new InAttribute();
        }

        internal static bool IsDefined(RuntimeParameterInfo parameter)
        {
            return parameter.IsIn;
        }
    }
}

