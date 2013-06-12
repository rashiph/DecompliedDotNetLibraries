namespace System.Net
{
    using System;

    internal abstract class RequestContextBase : IDisposable
    {
        private byte[] m_BackingBuffer;
        private unsafe UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* m_MemoryBlob;
        private unsafe UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* m_OriginalBlobAddress;

        protected RequestContextBase()
        {
        }

        protected unsafe void BaseConstruction(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* requestBlob)
        {
            if (requestBlob == null)
            {
                GC.SuppressFinalize(this);
            }
            else
            {
                this.m_MemoryBlob = requestBlob;
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~RequestContextBase()
        {
            this.Dispose(false);
        }

        protected abstract void OnReleasePins();
        internal unsafe void ReleasePins()
        {
            this.m_OriginalBlobAddress = this.m_MemoryBlob;
            this.UnsetBlob();
            this.OnReleasePins();
        }

        protected unsafe void SetBlob(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* requestBlob)
        {
            if (requestBlob == null)
            {
                this.UnsetBlob();
            }
            else
            {
                if (this.m_MemoryBlob == null)
                {
                    GC.ReRegisterForFinalize(this);
                }
                this.m_MemoryBlob = requestBlob;
            }
        }

        protected void SetBuffer(int size)
        {
            this.m_BackingBuffer = (size == 0) ? null : new byte[size];
        }

        protected unsafe void UnsetBlob()
        {
            if (this.m_MemoryBlob != null)
            {
                GC.SuppressFinalize(this);
            }
            this.m_MemoryBlob = null;
        }

        internal IntPtr OriginalBlobAddress
        {
            get
            {
                UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* memoryBlob = this.m_MemoryBlob;
                return ((memoryBlob == null) ? ((IntPtr) this.m_OriginalBlobAddress) : ((IntPtr) memoryBlob));
            }
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* RequestBlob
        {
            get
            {
                return this.m_MemoryBlob;
            }
        }

        internal byte[] RequestBuffer
        {
            get
            {
                return this.m_BackingBuffer;
            }
        }

        internal uint Size
        {
            get
            {
                return (uint) this.m_BackingBuffer.Length;
            }
        }
    }
}

