namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    internal struct AuthIdentity
    {
        internal string UserName;
        internal int UserNameLength;
        internal string Domain;
        internal int DomainLength;
        internal string Password;
        internal int PasswordLength;
        internal int Flags;
        internal AuthIdentity(string userName, string password, string domain)
        {
            this.UserName = userName;
            this.UserNameLength = (userName == null) ? 0 : userName.Length;
            this.Password = password;
            this.PasswordLength = (password == null) ? 0 : password.Length;
            this.Domain = domain;
            this.DomainLength = (domain == null) ? 0 : domain.Length;
            this.Flags = ComNetOS.IsWin9x ? 1 : 2;
        }

        public override string ToString()
        {
            return (ValidationHelper.ToString(this.Domain) + @"\" + ValidationHelper.ToString(this.UserName));
        }
    }
}

