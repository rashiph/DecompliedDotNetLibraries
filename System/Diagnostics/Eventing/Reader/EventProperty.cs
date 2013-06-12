namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventProperty
    {
        private object value;

        internal EventProperty(object value)
        {
            this.value = value;
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

