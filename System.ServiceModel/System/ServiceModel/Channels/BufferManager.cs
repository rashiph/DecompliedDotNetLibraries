namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    public abstract class BufferManager
    {
        protected BufferManager()
        {
        }

        public abstract void Clear();
        public static BufferManager CreateBufferManager(long maxBufferPoolSize, int maxBufferSize)
        {
            if (maxBufferPoolSize < 0L)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferPoolSize", maxBufferPoolSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if (maxBufferSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            return new WrappingBufferManager(InternalBufferManager.Create(maxBufferPoolSize, maxBufferSize));
        }

        internal static InternalBufferManager GetInternalBufferManager(BufferManager bufferManager)
        {
            if (bufferManager is WrappingBufferManager)
            {
                return ((WrappingBufferManager) bufferManager).InternalBufferManager;
            }
            return new WrappingInternalBufferManager(bufferManager);
        }

        public abstract void ReturnBuffer(byte[] buffer);
        public abstract byte[] TakeBuffer(int bufferSize);

        private class WrappingBufferManager : BufferManager
        {
            private System.Runtime.InternalBufferManager innerBufferManager;

            public WrappingBufferManager(System.Runtime.InternalBufferManager innerBufferManager)
            {
                this.innerBufferManager = innerBufferManager;
            }

            public override void Clear()
            {
                this.innerBufferManager.Clear();
            }

            public override void ReturnBuffer(byte[] buffer)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
                }
                this.innerBufferManager.ReturnBuffer(buffer);
            }

            public override byte[] TakeBuffer(int bufferSize)
            {
                if (bufferSize < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("bufferSize", bufferSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                return this.innerBufferManager.TakeBuffer(bufferSize);
            }

            public System.Runtime.InternalBufferManager InternalBufferManager
            {
                get
                {
                    return this.innerBufferManager;
                }
            }
        }

        private class WrappingInternalBufferManager : InternalBufferManager
        {
            private BufferManager innerBufferManager;

            public WrappingInternalBufferManager(BufferManager innerBufferManager)
            {
                this.innerBufferManager = innerBufferManager;
            }

            public override void Clear()
            {
                this.innerBufferManager.Clear();
            }

            public override void ReturnBuffer(byte[] buffer)
            {
                this.innerBufferManager.ReturnBuffer(buffer);
            }

            public override byte[] TakeBuffer(int bufferSize)
            {
                return this.innerBufferManager.TakeBuffer(bufferSize);
            }
        }
    }
}

