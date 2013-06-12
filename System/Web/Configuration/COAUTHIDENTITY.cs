namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack=4)]
    internal class COAUTHIDENTITY
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string user;
        internal int userlen;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string domain;
        internal int domainlen;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string password;
        internal int passwordlen;
        internal int flags = 2;
        internal COAUTHIDENTITY(string usr, string dom, string pwd)
        {
            this.user = usr;
            this.userlen = (this.user == null) ? 0 : this.user.Length;
            this.domain = dom;
            this.domainlen = (this.domain == null) ? 0 : this.domain.Length;
            this.password = pwd;
            this.passwordlen = (this.password == null) ? 0 : this.password.Length;
        }
    }
}

