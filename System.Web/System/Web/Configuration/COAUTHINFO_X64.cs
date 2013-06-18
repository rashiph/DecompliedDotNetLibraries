namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack=4)]
    internal class COAUTHINFO_X64 : IDisposable
    {
        internal RpcAuthent authnsvc;
        internal RpcAuthor authzsvc;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string serverprincname;
        internal RpcLevel authnlevel;
        internal RpcImpers impersonationlevel;
        internal IntPtr authidentitydata;
        internal int capabilities;
        internal int padding;
        internal COAUTHINFO_X64(RpcAuthent authent, RpcAuthor author, string serverprinc, RpcLevel level, RpcImpers impers, IntPtr ciptr)
        {
            this.authnsvc = authent;
            this.authzsvc = author;
            this.serverprincname = serverprinc;
            this.authnlevel = level;
            this.impersonationlevel = impers;
            this.authidentitydata = ciptr;
        }

        void IDisposable.Dispose()
        {
            this.authidentitydata = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        ~COAUTHINFO_X64()
        {
        }
    }
}

