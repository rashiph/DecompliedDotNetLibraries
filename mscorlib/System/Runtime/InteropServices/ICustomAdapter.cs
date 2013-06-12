namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true)]
    public interface ICustomAdapter
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object GetUnderlyingObject();
    }
}

