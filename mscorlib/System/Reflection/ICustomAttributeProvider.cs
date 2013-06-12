namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ICustomAttributeProvider
    {
        object[] GetCustomAttributes(bool inherit);
        object[] GetCustomAttributes(Type attributeType, bool inherit);
        bool IsDefined(Type attributeType, bool inherit);
    }
}

