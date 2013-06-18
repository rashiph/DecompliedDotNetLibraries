namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Channels;

    public class ReliableSession
    {
        private ReliableSessionBindingElement element;

        public ReliableSession()
        {
            this.element = new ReliableSessionBindingElement();
        }

        public ReliableSession(ReliableSessionBindingElement reliableSessionBindingElement)
        {
            if (reliableSessionBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reliableSessionBindingElement");
            }
            this.element = reliableSessionBindingElement;
        }

        internal void CopySettings(ReliableSession copyFrom)
        {
            this.Ordered = copyFrom.Ordered;
            this.InactivityTimeout = copyFrom.InactivityTimeout;
        }

        public TimeSpan InactivityTimeout
        {
            get
            {
                return this.element.InactivityTimeout;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.element.InactivityTimeout = value;
            }
        }

        [DefaultValue(true)]
        public bool Ordered
        {
            get
            {
                return this.element.Ordered;
            }
            set
            {
                this.element.Ordered = value;
            }
        }
    }
}

