namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    internal struct AuthIdentityEx
    {
        internal int Version;
        internal int Length;
        internal string UserName;
        internal int UserNameLength;
        internal string Domain;
        internal int DomainLength;
        internal string Password;
        internal int PasswordLength;
        internal int Flags;
        internal string PackageList;
        internal int PackageListLength;
        private static readonly int WinNTAuthIdentityVersion;
        internal AuthIdentityEx(string userName, string password, string domain, params string[] additionalPackages)
        {
            this.Version = WinNTAuthIdentityVersion;
            this.Length = Marshal.SizeOf(typeof(AuthIdentityEx));
            this.UserName = userName;
            this.UserNameLength = (userName == null) ? 0 : userName.Length;
            this.Password = password;
            this.PasswordLength = (password == null) ? 0 : password.Length;
            this.Domain = domain;
            this.DomainLength = (domain == null) ? 0 : domain.Length;
            this.Flags = 2;
            if (additionalPackages == null)
            {
                this.PackageList = null;
                this.PackageListLength = 0;
            }
            else
            {
                this.PackageList = string.Join(",", additionalPackages);
                this.PackageListLength = this.PackageList.Length;
            }
        }

        static AuthIdentityEx()
        {
            WinNTAuthIdentityVersion = 0x200;
        }
    }
}

