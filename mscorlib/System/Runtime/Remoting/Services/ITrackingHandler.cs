namespace System.Runtime.Remoting.Services
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;

    [ComVisible(true)]
    public interface ITrackingHandler
    {
        [SecurityCritical]
        void DisconnectedObject(object obj);
        [SecurityCritical]
        void MarshaledObject(object obj, ObjRef or);
        [SecurityCritical]
        void UnmarshaledObject(object obj, ObjRef or);
    }
}

