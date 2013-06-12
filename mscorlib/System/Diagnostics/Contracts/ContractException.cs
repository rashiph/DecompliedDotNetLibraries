namespace System.Diagnostics.Contracts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal sealed class ContractException : Exception
    {
        private readonly string _Condition;
        private readonly ContractFailureKind _Kind;
        private readonly string _UserMessage;

        public ContractException()
        {
        }

        private ContractException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._Kind = (ContractFailureKind) info.GetInt32("Kind");
            this._UserMessage = info.GetString("UserMessage");
            this._Condition = info.GetString("Condition");
        }

        public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innerException) : base(failure, innerException)
        {
            this._Kind = kind;
            this._UserMessage = userMessage;
            this._Condition = condition;
        }

        [ComVisible(false), SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Kind", this._Kind);
            info.AddValue("UserMessage", this._UserMessage);
            info.AddValue("Condition", this._Condition);
        }

        public string Condition
        {
            get
            {
                return this._Condition;
            }
        }

        public string Failure
        {
            get
            {
                return this.Message;
            }
        }

        public ContractFailureKind Kind
        {
            get
            {
                return this._Kind;
            }
        }

        public string UserMessage
        {
            get
            {
                return this._UserMessage;
            }
        }
    }
}

