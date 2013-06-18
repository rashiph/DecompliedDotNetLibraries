namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MessageAttemptInfo
    {
        private readonly System.ServiceModel.Channels.Message message;
        private readonly int retryCount;
        private readonly long sequenceNumber;
        private readonly object state;
        public MessageAttemptInfo(System.ServiceModel.Channels.Message message, long sequenceNumber, int retryCount, object state)
        {
            this.message = message;
            this.sequenceNumber = sequenceNumber;
            this.retryCount = retryCount;
            this.state = state;
        }

        public System.ServiceModel.Channels.Message Message
        {
            get
            {
                return this.message;
            }
        }
        public int RetryCount
        {
            get
            {
                return this.retryCount;
            }
        }
        public object State
        {
            get
            {
                return this.state;
            }
        }
        public long GetSequenceNumber()
        {
            if (this.sequenceNumber <= 0L)
            {
                throw Fx.AssertAndThrow("The caller is not allowed to get an invalid SequenceNumber.");
            }
            return this.sequenceNumber;
        }
    }
}

