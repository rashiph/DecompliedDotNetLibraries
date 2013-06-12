namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public delegate bool TypeFilter(Type m, object filterCriteria);
}

