namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class OverlappedData : CriticalFinalizerObject
    {
        private int m_AppDomainId;
        internal IAsyncResult m_asyncResult;
        internal OverlappedDataCacheLine m_cacheLine;
        [SecurityCritical]
        internal IOCompletionCallback m_iocb;
        internal _IOCompletionCallback m_iocbHelper;
        private byte m_isArray;
        internal NativeOverlapped m_nativeOverlapped;
        internal Overlapped m_overlapped;
        private IntPtr m_pinSelf;
        internal short m_slot;
        private byte m_toBeCleaned;
        private object m_userObject;
        private IntPtr m_userObjectInternal;

        internal OverlappedData(OverlappedDataCacheLine cacheLine)
        {
            this.m_cacheLine = cacheLine;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern unsafe NativeOverlapped* AllocateNativeOverlapped();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe void CheckVMForIOPacket(out NativeOverlapped* pOVERLAP, out uint errorCode, out uint numBytes);
        [SecuritySafeCritical]
        ~OverlappedData()
        {
            if (((this.m_cacheLine != null) && !this.m_cacheLine.Removed) && (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload()))
            {
                OverlappedDataCache.CacheOverlappedData(this);
                GC.ReRegisterForFinalize(this);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe void FreeNativeOverlapped(NativeOverlapped* nativeOverlappedPtr);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe OverlappedData GetOverlappedFromNative(NativeOverlapped* nativeOverlappedPtr);
        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        internal unsafe NativeOverlapped* Pack(IOCompletionCallback iocb, object userData)
        {
            if (!this.m_pinSelf.IsNull())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_Overlapped_Pack"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            if (iocb != null)
            {
                this.m_iocbHelper = new _IOCompletionCallback(iocb, ref lookForMyCaller);
                this.m_iocb = iocb;
            }
            else
            {
                this.m_iocbHelper = null;
                this.m_iocb = null;
            }
            this.m_userObject = userData;
            if (this.m_userObject != null)
            {
                if (this.m_userObject.GetType() == typeof(object[]))
                {
                    this.m_isArray = 1;
                }
                else
                {
                    this.m_isArray = 0;
                }
            }
            return this.AllocateNativeOverlapped();
        }

        [SecurityCritical]
        internal void ReInitialize()
        {
            this.m_asyncResult = null;
            this.m_iocb = null;
            this.m_iocbHelper = null;
            this.m_overlapped = null;
            this.m_userObject = null;
            this.m_pinSelf = IntPtr.Zero;
            this.m_userObjectInternal = IntPtr.Zero;
            this.m_AppDomainId = 0;
            this.m_nativeOverlapped.EventHandle = IntPtr.Zero;
            this.m_isArray = 0;
            this.m_nativeOverlapped.InternalHigh = IntPtr.Zero;
        }

        [SecurityCritical]
        internal unsafe NativeOverlapped* UnsafePack(IOCompletionCallback iocb, object userData)
        {
            if (!this.m_pinSelf.IsNull())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_Overlapped_Pack"));
            }
            this.m_userObject = userData;
            if (this.m_userObject != null)
            {
                if (this.m_userObject.GetType() == typeof(object[]))
                {
                    this.m_isArray = 1;
                }
                else
                {
                    this.m_isArray = 0;
                }
            }
            this.m_iocb = iocb;
            this.m_iocbHelper = null;
            return this.AllocateNativeOverlapped();
        }

        [ComVisible(false)]
        internal IntPtr UserHandle
        {
            get
            {
                return this.m_nativeOverlapped.EventHandle;
            }
            set
            {
                this.m_nativeOverlapped.EventHandle = value;
            }
        }
    }
}

