namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public delegate bool MemberFilter(MemberInfo m, object filterCriteria);
}

