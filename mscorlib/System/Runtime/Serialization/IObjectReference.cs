namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IObjectReference
    {
        [SecurityCritical]
        object GetRealObject(StreamingContext context);
    }
}

