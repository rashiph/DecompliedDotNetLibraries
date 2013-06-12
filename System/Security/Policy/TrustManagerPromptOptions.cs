namespace System.Security.Policy
{
    using System;

    [Flags]
    internal enum TrustManagerPromptOptions
    {
        AddsShortcut = 8,
        InternetSource = 0x40,
        LocalComputerSource = 0x20,
        LocalNetworkSource = 0x10,
        None = 0,
        RequiresPermissions = 2,
        StopApp = 1,
        TrustedSitesSource = 0x80,
        UntrustedSitesSource = 0x100,
        WillHaveFullTrust = 4
    }
}

