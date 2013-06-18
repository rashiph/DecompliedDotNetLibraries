namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;

    [Serializable]
    internal class ActionMismatchAddressingException : ProtocolException
    {
        private string httpActionHeader;
        private string soapActionHeader;

        protected ActionMismatchAddressingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ActionMismatchAddressingException(string message, string soapActionHeader, string httpActionHeader) : base(message)
        {
            this.httpActionHeader = httpActionHeader;
            this.soapActionHeader = soapActionHeader;
        }

        internal Message ProvideFault(MessageVersion messageVersion)
        {
            WSAddressing10ProblemHeaderQNameFault fault = new WSAddressing10ProblemHeaderQNameFault(this);
            Message message = Message.CreateMessage(messageVersion, fault, messageVersion.Addressing.FaultAction);
            fault.AddHeaders(message.Headers);
            return message;
        }

        public string HttpActionHeader
        {
            get
            {
                return this.httpActionHeader;
            }
        }

        public string SoapActionHeader
        {
            get
            {
                return this.soapActionHeader;
            }
        }
    }
}

