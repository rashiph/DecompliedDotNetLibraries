namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class BufferManagerOutputStream : BufferedOutputStream
    {
        private string quotaExceededString;

        public BufferManagerOutputStream(string quotaExceededString)
        {
            this.quotaExceededString = quotaExceededString;
        }

        public BufferManagerOutputStream(string quotaExceededString, int maxSize) : base(maxSize)
        {
            this.quotaExceededString = quotaExceededString;
        }

        public BufferManagerOutputStream(string quotaExceededString, int initialSize, int maxSize, BufferManager bufferManager) : base(initialSize, maxSize, BufferManager.GetInternalBufferManager(bufferManager))
        {
            this.quotaExceededString = quotaExceededString;
        }

        protected override Exception CreateQuotaExceededException(int maxSizeQuota)
        {
            return new QuotaExceededException(System.ServiceModel.SR.GetString(this.quotaExceededString, new object[] { maxSizeQuota }));
        }

        public void Init(int initialSize, int maxSizeQuota, BufferManager bufferManager)
        {
            this.Init(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
        }

        public void Init(int initialSize, int maxSizeQuota, int effectiveMaxSize, BufferManager bufferManager)
        {
            base.Reinitialize(initialSize, maxSizeQuota, effectiveMaxSize, BufferManager.GetInternalBufferManager(bufferManager));
        }
    }
}

