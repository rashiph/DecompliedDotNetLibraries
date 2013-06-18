namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;

    [Serializable]
    public class FaultException<TDetail> : FaultException
    {
        private TDetail detail;

        public FaultException(TDetail detail)
        {
            this.detail = detail;
        }

        protected FaultException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.detail = (TDetail) info.GetValue("detail", typeof(TDetail));
        }

        public FaultException(TDetail detail, FaultReason reason) : base(reason)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, string reason) : base(reason)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, FaultReason reason, FaultCode code) : base(reason, code)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, string reason, FaultCode code) : base(reason, code)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, FaultReason reason, FaultCode code, string action) : base(reason, code, action)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, string reason, FaultCode code, string action) : base(reason, code, action)
        {
            this.detail = detail;
        }

        public override MessageFault CreateMessageFault()
        {
            return MessageFault.CreateFault(base.Code, base.Reason, this.detail);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("detail", this.detail);
        }

        public override string ToString()
        {
            return System.ServiceModel.SR.GetString("SFxFaultExceptionToString3", new object[] { base.GetType(), this.Message, this.detail.ToString() });
        }

        public TDetail Detail
        {
            get
            {
                return this.detail;
            }
        }
    }
}

