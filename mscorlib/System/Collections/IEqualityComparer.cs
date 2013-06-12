namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IEqualityComparer
    {
        bool Equals(object x, object y);
        int GetHashCode(object obj);
    }
}

