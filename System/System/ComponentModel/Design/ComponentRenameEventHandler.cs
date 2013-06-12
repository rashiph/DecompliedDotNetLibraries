namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public delegate void ComponentRenameEventHandler(object sender, ComponentRenameEventArgs e);
}

