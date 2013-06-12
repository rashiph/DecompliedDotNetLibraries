namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Parameter, Inherited=false), ComVisible(true)]
    public sealed class OptionalAttribute : Attribute
    {
        internal static Attribute GetCustomAttribute(RuntimeParameterInfo parameter)
        {
            if (!parameter.IsOptional)
            {
                return null;
            }
            return new OptionalAttribute();
        }

        internal static bool IsDefined(RuntimeParameterInfo parameter)
        {
            return parameter.IsOptional;
        }
    }
}

