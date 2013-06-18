namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal class SEC_WINNT_AUTH_IDENTITY_EX
    {
        public uint Version;
        public uint Length;
        public string User;
        public uint UserLength;
        public string Domain;
        public uint DomainLength;
        public string Password;
        public uint PasswordLength;
        public uint Flags;
        public string PackageList;
        public uint PackageListLength;
    }
}

