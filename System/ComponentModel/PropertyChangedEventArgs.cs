namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class PropertyChangedEventArgs : EventArgs
    {
        private readonly string propertyName;

        public PropertyChangedEventArgs(string propertyName)
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

