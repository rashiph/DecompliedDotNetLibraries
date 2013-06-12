namespace System.Windows.Forms
{
    using System;
    using System.Security.Permissions;

    public interface IMessageFilter
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        bool PreFilterMessage(ref Message m);
    }
}

