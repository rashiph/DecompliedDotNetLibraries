namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack=4)]
    internal class COSERVERINFO : IDisposable
    {
        internal int reserved1;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string servername;
        internal IntPtr authinfo;
        internal int reserved2;
        internal COSERVERINFO(string srvname, IntPtr authinf)
        {
            this.servername = srvname;
            this.authinfo = authinf;
        }

        void IDisposable.Dispose()
        {
            this.authinfo = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        ~COSERVERINFO()
        {
        }
    }
}

