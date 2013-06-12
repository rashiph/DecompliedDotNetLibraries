namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class PropertyChangingEventArgs : EventArgs
    {
        private readonly string propertyName;

        public PropertyChangingEventArgs(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public virtual string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }
    }
}

