namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcTokenRequestDetail
    {
        public int uriLength;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string recipientUri;
        public int cbRecipientToken;
        public byte[] recipientToken;
        public int cchPolicy;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string policy;
    }
}

