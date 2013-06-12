namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited=false), ComVisible(true)]
    public sealed class ComImportAttribute : Attribute
    {
        internal static Attribute GetCustomAttribute(RuntimeType type)
        {
            if ((type.Attributes & TypeAttributes.Import) == TypeAttributes.AnsiClass)
            {
                return null;
            }
            return new ComImportAttribute();
        }

        internal static bool IsDefined(RuntimeType type)
        {
            return ((type.Attributes & TypeAttributes.Import) != TypeAttributes.AnsiClass);
        }
    }
}

