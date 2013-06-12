namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public class OperationCanceledException : SystemException
    {
        [NonSerialized]
        private System.Threading.CancellationToken _cancellationToken;

        public OperationCanceledException() : base(Environment.GetResourceString("OperationCanceled"))
        {
            base.SetErrorCode(-2146233029);
        }

        public OperationCanceledException(string message) : base(message)
        {
            base.SetErrorCode(-2146233029);
        }

        public OperationCanceledException(System.Threading.CancellationToken token) : this()
        {
            this.CancellationToken = token;
        }

        protected OperationCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public OperationCanceledException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233029);
        }

        public OperationCanceledException(string message, System.Threading.CancellationToken token) : this(message)
        {
            this.CancellationToken = token;
        }

        public OperationCanceledException(string message, Exception innerException, System.Threading.CancellationToken token) : this(message, innerException)
        {
            this.CancellationToken = token;
        }

        public System.Threading.CancellationToken CancellationToken
        {
            get
            {
                return this._cancellationToken;
            }
            private set
            {
                this._cancellationToken = value;
            }
        }
    }
}

