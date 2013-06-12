namespace System.ComponentModel.Design
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class DesignerEventArgs : EventArgs
    {
        private readonly IDesignerHost host;

        public DesignerEventArgs(IDesignerHost host)
        {
            this.host = host;
        }

        public IDesignerHost Designer
        {
            get
            {
                return this.host;
            }
        }
    }
}

