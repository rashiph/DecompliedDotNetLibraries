namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IComparer
    {
        int Compare(object x, object y);
    }
}

