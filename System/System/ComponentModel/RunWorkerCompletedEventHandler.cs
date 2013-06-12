namespace System.ComponentModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
}

