namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;

    [ComVisible(true)]
    public sealed class ExtensibleClassFactory
    {
        private ExtensibleClassFactory()
        {
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern void RegisterObjectCreationCallback(ObjectCreationDelegate callback);
    }
}

