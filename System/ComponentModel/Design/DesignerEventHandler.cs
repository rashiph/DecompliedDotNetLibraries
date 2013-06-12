namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public delegate void DesignerEventHandler(object sender, DesignerEventArgs e);
}

