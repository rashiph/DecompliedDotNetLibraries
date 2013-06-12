namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface ITransportHeaders
    {
        [SecurityCritical]
        IEnumerator GetEnumerator();

        object this[object key] { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

