namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    internal class SyncRequestContext : RequestContextBase
    {
        private GCHandle m_PinnedHandle;

        internal SyncRequestContext(int size)
        {
            base.BaseConstruction(this.Allocate(size));
        }

        private unsafe UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* Allocate(int size)
        {
            if (this.m_PinnedHandle.IsAllocated)
            {
                if (base.RequestBuffer.Length == size)
                {
                    return base.RequestBlob;
                }
                this.m_PinnedHandle.Free();
            }
            base.SetBuffer(size);
            if (base.RequestBuffer == null)
            {
                return null;
            }
            this.m_PinnedHandle = GCHandle.Alloc(base.RequestBuffer, GCHandleType.Pinned);
            return (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*) Marshal.UnsafeAddrOfPinnedArrayElement(base.RequestBuffer, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.m_PinnedHandle.IsAllocated && (!NclUtilities.HasShutdownStarted || disposing))
            {
                this.m_PinnedHandle.Free();
            }
            base.Dispose(disposing);
        }

        protected override void OnReleasePins()
        {
            if (this.m_PinnedHandle.IsAllocated)
            {
                this.m_PinnedHandle.Free();
            }
        }

        internal void Reset(int size)
        {
            base.SetBlob(this.Allocate(size));
        }
    }
}

