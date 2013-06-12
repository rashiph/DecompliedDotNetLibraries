namespace System.Net
{
    using System;
    using System.ComponentModel;

    public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        private long m_BytesReceived;
        private long m_TotalBytesToReceive;

        internal DownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive) : base(progressPercentage, userToken)
        {
            this.m_BytesReceived = bytesReceived;
            this.m_TotalBytesToReceive = totalBytesToReceive;
        }

        public long BytesReceived
        {
            get
            {
                return this.m_BytesReceived;
            }
        }

        public long TotalBytesToReceive
        {
            get
            {
                return this.m_TotalBytesToReceive;
            }
        }
    }
}

