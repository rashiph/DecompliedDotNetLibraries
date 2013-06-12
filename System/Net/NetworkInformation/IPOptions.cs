namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IPOptions
    {
        internal byte ttl;
        internal byte tos;
        internal byte flags;
        internal byte optionsSize;
        internal IntPtr optionsData;
        internal IPOptions(PingOptions options)
        {
            this.ttl = 0x80;
            this.tos = 0;
            this.flags = 0;
            this.optionsSize = 0;
            this.optionsData = IntPtr.Zero;
            if (options != null)
            {
                this.ttl = (byte) options.Ttl;
                if (options.DontFragment)
                {
                    this.flags = 2;
                }
            }
        }
    }
}

