namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public class MsmqPoisonMessageException : PoisonMessageException
    {
        private long messageLookupId;

        public MsmqPoisonMessageException()
        {
        }

        public MsmqPoisonMessageException(long messageLookupId) : this(messageLookupId, null)
        {
        }

        public MsmqPoisonMessageException(string message) : base(message)
        {
        }

        public MsmqPoisonMessageException(long messageLookupId, Exception innerException) : base(System.ServiceModel.SR.GetString("MsmqPoisonMessage"), innerException)
        {
            this.messageLookupId = messageLookupId;
        }

        protected MsmqPoisonMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.messageLookupId = (long) info.GetValue("messageLookupId", typeof(long));
        }

        public MsmqPoisonMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [SecurityCritical, SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("messageLookupId", this.messageLookupId);
        }

        public long MessageLookupId
        {
            get
            {
                return this.messageLookupId;
            }
        }
    }
}

