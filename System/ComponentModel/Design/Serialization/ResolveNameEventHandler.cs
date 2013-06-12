namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public delegate void ResolveNameEventHandler(object sender, ResolveNameEventArgs e);
}

