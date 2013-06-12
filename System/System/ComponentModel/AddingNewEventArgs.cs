namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class AddingNewEventArgs : EventArgs
    {
        private object newObject;

        public AddingNewEventArgs()
        {
        }

        public AddingNewEventArgs(object newObject)
        {
            this.newObject = newObject;
        }

        public object NewObject
        {
            get
            {
                return this.newObject;
            }
            set
            {
                this.newObject = value;
            }
        }
    }
}

