namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class HandledEventArgs : EventArgs
    {
        private bool handled;

        public HandledEventArgs() : this(false)
        {
        }

        public HandledEventArgs(bool defaultHandledValue)
        {
            this.handled = defaultHandledValue;
        }

        public bool Handled
        {
            get
            {
                return this.handled;
            }
            set
            {
                this.handled = value;
            }
        }
    }
}

