namespace System
{
    using System.Reflection;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Field, Inherited=false), ComVisible(true)]
    public sealed class NonSerializedAttribute : Attribute
    {
        internal static Attribute GetCustomAttribute(RuntimeFieldInfo field)
        {
            if ((field.Attributes & FieldAttributes.NotSerialized) == FieldAttributes.PrivateScope)
            {
                return null;
            }
            return new NonSerializedAttribute();
        }

        internal static bool IsDefined(RuntimeFieldInfo field)
        {
            return ((field.Attributes & FieldAttributes.NotSerialized) != FieldAttributes.PrivateScope);
        }
    }
}

