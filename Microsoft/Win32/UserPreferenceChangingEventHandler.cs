namespace Microsoft.Win32
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public delegate void UserPreferenceChangingEventHandler(object sender, UserPreferenceChangingEventArgs e);
}

