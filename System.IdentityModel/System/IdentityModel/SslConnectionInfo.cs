namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class SslConnectionInfo
    {
        public readonly int Protocol;
        public readonly int DataCipherAlg;
        public readonly int DataKeySize;
        public readonly int DataHashAlg;
        public readonly int DataHashKeySize;
        public readonly int KeyExchangeAlg;
        public readonly int KeyExchKeySize;
        internal unsafe SslConnectionInfo(byte[] nativeBuffer)
        {
            byte[] buffer;
            if (((buffer = nativeBuffer) != null) && (buffer.Length != 0))
            {
                goto Label_0015;
            }
            fixed (IntPtr* ptrRef = null)
            {
                IntPtr ptr;
                goto Label_001D;
            Label_0015:
                ptrRef = buffer;
            Label_001D:
                ptr = new IntPtr((void*) ptrRef);
                this.Protocol = Marshal.ReadInt32(ptr);
                this.DataCipherAlg = Marshal.ReadInt32(ptr, 4);
                this.DataKeySize = Marshal.ReadInt32(ptr, 8);
                this.DataHashAlg = Marshal.ReadInt32(ptr, 12);
                this.DataHashKeySize = Marshal.ReadInt32(ptr, 0x10);
                this.KeyExchangeAlg = Marshal.ReadInt32(ptr, 20);
                this.KeyExchKeySize = Marshal.ReadInt32(ptr, 0x18);
            }
        }
    }
}

