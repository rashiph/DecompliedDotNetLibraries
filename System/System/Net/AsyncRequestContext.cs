namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class AsyncRequestContext : RequestContextBase
    {
        private unsafe System.Threading.NativeOverlapped* m_NativeOverlapped;
        private ListenerAsyncResult m_Result;

        internal AsyncRequestContext(ListenerAsyncResult result)
        {
            this.m_Result = result;
            base.BaseConstruction(this.Allocate(0));
        }

        private unsafe UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* Allocate(uint size)
        {
            uint num = (size != 0) ? size : ((base.RequestBuffer == null) ? 0x1000 : base.Size);
            if ((this.m_NativeOverlapped != null) && (num != base.RequestBuffer.Length))
            {
                System.Threading.NativeOverlapped* nativeOverlapped = this.m_NativeOverlapped;
                this.m_NativeOverlapped = null;
                Overlapped.Free(nativeOverlapped);
            }
            if (this.m_NativeOverlapped == null)
            {
                base.SetBuffer((int) num);
                this.m_NativeOverlapped = new Overlapped { AsyncResult = this.m_Result }.Pack(ListenerAsyncResult.IOCallback, base.RequestBuffer);
                return (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*) Marshal.UnsafeAddrOfPinnedArrayElement(base.RequestBuffer, 0);
            }
            return base.RequestBlob;
        }

        protected override unsafe void Dispose(bool disposing)
        {
            if ((this.m_NativeOverlapped != null) && (!NclUtilities.HasShutdownStarted || disposing))
            {
                Overlapped.Free(this.m_NativeOverlapped);
            }
            base.Dispose(disposing);
        }

        protected override unsafe void OnReleasePins()
        {
            if (this.m_NativeOverlapped != null)
            {
                System.Threading.NativeOverlapped* nativeOverlapped = this.m_NativeOverlapped;
                this.m_NativeOverlapped = null;
                Overlapped.Free(nativeOverlapped);
            }
        }

        internal unsafe void Reset(ulong requestId, uint size)
        {
            base.SetBlob(this.Allocate(size));
            base.RequestBlob.RequestId = requestId;
        }

        internal System.Threading.NativeOverlapped* NativeOverlapped
        {
            get
            {
                return this.m_NativeOverlapped;
            }
        }
    }
}

