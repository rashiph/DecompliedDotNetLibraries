namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [Obsolete("Please use IEqualityComparer instead."), ComVisible(true)]
    public interface IHashCodeProvider
    {
        int GetHashCode(object obj);
    }
}

