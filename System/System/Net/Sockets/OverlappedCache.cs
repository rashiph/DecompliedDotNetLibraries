namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Threading;

    internal class OverlappedCache
    {
        internal SafeNativeOverlapped m_NativeOverlapped;
        internal System.Threading.Overlapped m_Overlapped;
        internal object m_PinnedObjects;
        internal object[] m_PinnedObjectsArray;

        internal OverlappedCache(System.Threading.Overlapped overlapped, object[] pinnedObjectsArray, IOCompletionCallback callback)
        {
            this.m_Overlapped = overlapped;
            this.m_PinnedObjects = pinnedObjectsArray;
            this.m_PinnedObjectsArray = pinnedObjectsArray;
            this.m_NativeOverlapped = new SafeNativeOverlapped(overlapped.UnsafePack(callback, pinnedObjectsArray));
        }

        internal OverlappedCache(System.Threading.Overlapped overlapped, object pinnedObjects, IOCompletionCallback callback, bool alreadyTriedCast)
        {
            this.m_Overlapped = overlapped;
            this.m_PinnedObjects = pinnedObjects;
            this.m_PinnedObjectsArray = alreadyTriedCast ? null : NclConstants.EmptyObjectArray;
            this.m_NativeOverlapped = new SafeNativeOverlapped(overlapped.UnsafePack(callback, pinnedObjects));
        }

        ~OverlappedCache()
        {
            if (!NclUtilities.HasShutdownStarted)
            {
                this.InternalFree();
            }
        }

        internal void Free()
        {
            this.InternalFree();
            GC.SuppressFinalize(this);
        }

        internal static void InterlockedFree(ref OverlappedCache overlappedCache)
        {
            OverlappedCache cache = (overlappedCache == null) ? null : Interlocked.Exchange<OverlappedCache>(ref overlappedCache, null);
            if (cache != null)
            {
                cache.Free();
            }
        }

        private void InternalFree()
        {
            this.m_Overlapped = null;
            this.m_PinnedObjects = null;
            if (this.m_NativeOverlapped != null)
            {
                if (!this.m_NativeOverlapped.IsInvalid)
                {
                    this.m_NativeOverlapped.Dispose();
                }
                this.m_NativeOverlapped = null;
            }
        }

        internal SafeNativeOverlapped NativeOverlapped
        {
            get
            {
                return this.m_NativeOverlapped;
            }
        }

        internal System.Threading.Overlapped Overlapped
        {
            get
            {
                return this.m_Overlapped;
            }
        }

        internal object PinnedObjects
        {
            get
            {
                return this.m_PinnedObjects;
            }
        }

        internal object[] PinnedObjectsArray
        {
            get
            {
                object[] pinnedObjectsArray = this.m_PinnedObjectsArray;
                if ((pinnedObjectsArray != null) && (pinnedObjectsArray.Length == 0))
                {
                    pinnedObjectsArray = this.m_PinnedObjects as object[];
                    if ((pinnedObjectsArray != null) && (pinnedObjectsArray.Length == 0))
                    {
                        this.m_PinnedObjectsArray = null;
                    }
                    else
                    {
                        this.m_PinnedObjectsArray = pinnedObjectsArray;
                    }
                }
                return this.m_PinnedObjectsArray;
            }
        }
    }
}

