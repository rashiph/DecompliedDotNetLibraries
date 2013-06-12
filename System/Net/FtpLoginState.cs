namespace System.Net
{
    using System;

    internal enum FtpLoginState : byte
    {
        LoggedIn = 1,
        LoggedInButNeedsRelogin = 2,
        NotLoggedIn = 0,
        ReloginFailed = 3
    }
}

