namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum MethodCodeType
    {
        IL,
        Native,
        OPTIL,
        Runtime
    }
}

