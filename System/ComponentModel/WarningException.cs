namespace System.ComponentModel
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class WarningException : SystemException
    {
        private readonly string helpTopic;
        private readonly string helpUrl;

        public WarningException() : this(null, null, null)
        {
        }

        public WarningException(string message) : this(message, null, null)
        {
        }

        protected WarningException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.helpUrl = (string) info.GetValue("helpUrl", typeof(string));
            this.helpTopic = (string) info.GetValue("helpTopic", typeof(string));
        }

        public WarningException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WarningException(string message, string helpUrl) : this(message, helpUrl, null)
        {
        }

        public WarningException(string message, string helpUrl, string helpTopic) : base(message)
        {
            this.helpUrl = helpUrl;
            this.helpTopic = helpTopic;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("helpUrl", this.helpUrl);
            info.AddValue("helpTopic", this.helpTopic);
            base.GetObjectData(info, context);
        }

        public string HelpTopic
        {
            get
            {
                return this.helpTopic;
            }
        }

        public string HelpUrl
        {
            get
            {
                return this.helpUrl;
            }
        }
    }
}

