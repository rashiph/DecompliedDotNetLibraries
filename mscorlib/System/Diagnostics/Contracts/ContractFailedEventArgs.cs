namespace System.Diagnostics.Contracts
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    public sealed class ContractFailedEventArgs : EventArgs
    {
        private string _condition;
        private ContractFailureKind _failureKind;
        private bool _handled;
        private string _message;
        private Exception _originalException;
        private bool _unwind;
        internal Exception thrownDuringHandler;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public ContractFailedEventArgs(ContractFailureKind failureKind, string message, string condition, Exception originalException)
        {
            this._failureKind = failureKind;
            this._message = message;
            this._condition = condition;
            this._originalException = originalException;
        }

        [SecurityCritical]
        public void SetHandled()
        {
            this._handled = true;
        }

        [SecurityCritical]
        public void SetUnwind()
        {
            this._unwind = true;
        }

        public string Condition
        {
            get
            {
                return this._condition;
            }
        }

        public ContractFailureKind FailureKind
        {
            get
            {
                return this._failureKind;
            }
        }

        public bool Handled
        {
            get
            {
                return this._handled;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }

        public Exception OriginalException
        {
            get
            {
                return this._originalException;
            }
        }

        public bool Unwind
        {
            get
            {
                return this._unwind;
            }
        }
    }
}

