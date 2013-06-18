namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public class OptionalReliableSession : ReliableSession
    {
        private bool enabled;

        public OptionalReliableSession()
        {
        }

        public OptionalReliableSession(ReliableSessionBindingElement reliableSessionBindingElement) : base(reliableSessionBindingElement)
        {
        }

        internal void CopySettings(OptionalReliableSession copyFrom)
        {
            base.CopySettings(copyFrom);
            this.Enabled = copyFrom.Enabled;
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }
    }
}

