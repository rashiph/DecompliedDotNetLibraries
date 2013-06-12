namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IMessageCtrl
    {
        [SecurityCritical]
        void Cancel(int msToCancel);
    }
}

