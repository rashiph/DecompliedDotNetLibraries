namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SSL_EXTRA_CERT_CHAIN_POLICY_PARA
    {
        internal U u;
        internal int dwAuthType;
        internal uint fdwChecks;
        internal unsafe char* pwszServerName;
        private static readonly uint StructSize;
        internal unsafe SSL_EXTRA_CERT_CHAIN_POLICY_PARA(bool amIServer)
        {
            this.u.cbStruct = StructSize;
            this.u.cbSize = StructSize;
            this.dwAuthType = amIServer ? 1 : 2;
            this.fdwChecks = 0;
            this.pwszServerName = null;
        }

        static SSL_EXTRA_CERT_CHAIN_POLICY_PARA()
        {
            StructSize = (uint) Marshal.SizeOf(typeof(SSL_EXTRA_CERT_CHAIN_POLICY_PARA));
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct U
        {
            [FieldOffset(0)]
            internal uint cbSize;
            [FieldOffset(0)]
            internal uint cbStruct;
        }
    }
}

