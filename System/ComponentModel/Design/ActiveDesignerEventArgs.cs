namespace System.ComponentModel.Design
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ActiveDesignerEventArgs : EventArgs
    {
        private readonly IDesignerHost newDesigner;
        private readonly IDesignerHost oldDesigner;

        public ActiveDesignerEventArgs(IDesignerHost oldDesigner, IDesignerHost newDesigner)
        {
            this.oldDesigner = oldDesigner;
            this.newDesigner = newDesigner;
        }

        public IDesignerHost NewDesigner
        {
            get
            {
                return this.newDesigner;
            }
        }

        public IDesignerHost OldDesigner
        {
            get
            {
                return this.oldDesigner;
            }
        }
    }
}

