namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TunnelStateObject
    {
        internal System.Net.Connection Connection;
        internal HttpWebRequest OriginalRequest;
        internal TunnelStateObject(HttpWebRequest r, System.Net.Connection c)
        {
            this.Connection = c;
            this.OriginalRequest = r;
        }
    }
}

