namespace System.ComponentModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
}

