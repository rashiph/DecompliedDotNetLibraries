namespace System.Runtime.InteropServices
{
    using System;
    using System.Security;

    [ComVisible(false)]
    public interface ICustomQueryInterface
    {
        [SecurityCritical]
        CustomQueryInterfaceResult GetInterface([In] ref Guid iid, out IntPtr ppv);
    }
}

