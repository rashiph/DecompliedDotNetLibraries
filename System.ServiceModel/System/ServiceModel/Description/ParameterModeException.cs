namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime.Serialization;

    internal class ParameterModeException : Exception
    {
        private System.ServiceModel.Description.MessageContractType messageContractType;

        public ParameterModeException()
        {
            this.messageContractType = System.ServiceModel.Description.MessageContractType.WrappedMessageContract;
        }

        public ParameterModeException(string message) : base(message)
        {
            this.messageContractType = System.ServiceModel.Description.MessageContractType.WrappedMessageContract;
        }

        public ParameterModeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.messageContractType = System.ServiceModel.Description.MessageContractType.WrappedMessageContract;
        }

        public System.ServiceModel.Description.MessageContractType MessageContractType
        {
            get
            {
                return this.messageContractType;
            }
            set
            {
                this.messageContractType = value;
            }
        }
    }
}

