namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class DurableDispatcherAddressingFault : MessageFault
    {
        private FaultCode faultCode = FaultCode.CreateSenderFaultCode("MissingContext", "http://schemas.microsoft.com/ws/2006/05/context");
        private FaultReason faultReason = new FaultReason(new FaultReasonText(System.ServiceModel.Activities.SR.CurrentOperationCannotCreateInstance, CultureInfo.CurrentCulture));

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        public override FaultCode Code
        {
            get
            {
                return this.faultCode;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return false;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return this.faultReason;
            }
        }
    }
}

