namespace System.Net
{
    using System;
    using System.ComponentModel;

    public class UploadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        private long m_BytesReceived;
        private long m_BytesSent;
        private long m_TotalBytesToReceive;
        private long m_TotalBytesToSend;

        internal UploadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesSent, long totalBytesToSend, long bytesReceived, long totalBytesToReceive) : base(progressPercentage, userToken)
        {
            this.m_BytesReceived = bytesReceived;
            this.m_TotalBytesToReceive = totalBytesToReceive;
            this.m_BytesSent = bytesSent;
            this.m_TotalBytesToSend = totalBytesToSend;
        }

        public long BytesReceived
        {
            get
            {
                return this.m_BytesReceived;
            }
        }

        public long BytesSent
        {
            get
            {
                return this.m_BytesSent;
            }
        }

        public long TotalBytesToReceive
        {
            get
            {
                return this.m_TotalBytesToReceive;
            }
        }

        public long TotalBytesToSend
        {
            get
            {
                return this.m_TotalBytesToSend;
            }
        }
    }
}

