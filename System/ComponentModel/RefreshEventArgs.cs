namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class RefreshEventArgs : EventArgs
    {
        private object componentChanged;
        private Type typeChanged;

        public RefreshEventArgs(object componentChanged)
        {
            this.componentChanged = componentChanged;
            this.typeChanged = componentChanged.GetType();
        }

        public RefreshEventArgs(Type typeChanged)
        {
            this.typeChanged = typeChanged;
        }

        public object ComponentChanged
        {
            get
            {
                return this.componentChanged;
            }
        }

        public Type TypeChanged
        {
            get
            {
                return this.typeChanged;
            }
        }
    }
}

