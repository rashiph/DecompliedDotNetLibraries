namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal class NativeMethods
    {
        public enum AuthenticationModes
        {
            FastBind = 0x20,
            NoAuthentication = 0x10,
            ReadonlyServer = 4,
            SecureAuthentication = 1,
            UseDelegation = 0x100,
            UseEncryption = 2,
            UseSealing = 0x80,
            UseServerBinding = 0x200,
            UseSigning = 0x40,
            UseSSL = 2
        }
    }
}

