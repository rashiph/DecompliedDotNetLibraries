namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct GCHandle
    {
        private const GCHandleType MaxHandleType = GCHandleType.Pinned;
        private IntPtr m_handle;
        private static GCHandleCookieTable s_cookieTable;
        private static bool s_probeIsActive;
        [SecuritySafeCritical]
        static GCHandle()
        {
            s_probeIsActive = Mda.IsInvalidGCHandleCookieProbeEnabled();
            if (s_probeIsActive)
            {
                s_cookieTable = new GCHandleCookieTable();
            }
        }

        [SecurityCritical]
        internal GCHandle(object value, GCHandleType type)
        {
            if (type > GCHandleType.Pinned)
            {
                throw new ArgumentOutOfRangeException("type", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            this.m_handle = InternalAlloc(value, type);
            if (type == GCHandleType.Pinned)
            {
                this.SetIsPinned();
            }
        }

        [SecurityCritical]
        internal GCHandle(IntPtr handle)
        {
            InternalCheckDomain(handle);
            this.m_handle = handle;
        }

        [SecurityCritical]
        public static GCHandle Alloc(object value)
        {
            return new GCHandle(value, GCHandleType.Normal);
        }

        [SecurityCritical]
        public static GCHandle Alloc(object value, GCHandleType type)
        {
            return new GCHandle(value, type);
        }

        [SecurityCritical]
        public void Free()
        {
            IntPtr handle = this.m_handle;
            if (!(handle != IntPtr.Zero) || !(Interlocked.CompareExchange(ref this.m_handle, IntPtr.Zero, handle) == handle))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
            }
            if (s_probeIsActive)
            {
                s_cookieTable.RemoveHandleIfPresent(handle);
            }
            InternalFree((IntPtr) (((int) handle) & -2));
        }

        public object Target
        {
            [SecurityCritical]
            get
            {
                if (this.m_handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
                }
                return InternalGet(this.GetHandleValue());
            }
            [SecurityCritical]
            set
            {
                if (this.m_handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
                }
                InternalSet(this.GetHandleValue(), value, this.IsPinned());
            }
        }
        [SecurityCritical]
        public IntPtr AddrOfPinnedObject()
        {
            if (this.IsPinned())
            {
                return InternalAddrOfPinnedObject(this.GetHandleValue());
            }
            if (this.m_handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotPinned"));
        }

        public bool IsAllocated
        {
            get
            {
                return (this.m_handle != IntPtr.Zero);
            }
        }
        [SecurityCritical]
        public static explicit operator GCHandle(IntPtr value)
        {
            return FromIntPtr(value);
        }

        [SecurityCritical]
        public static GCHandle FromIntPtr(IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
            }
            IntPtr handle = value;
            if (s_probeIsActive)
            {
                handle = s_cookieTable.GetHandle(value);
                if (IntPtr.Zero == handle)
                {
                    Mda.FireInvalidGCHandleCookieProbe(value);
                    return new GCHandle(IntPtr.Zero);
                }
            }
            return new GCHandle(handle);
        }

        public static explicit operator IntPtr(GCHandle value)
        {
            return ToIntPtr(value);
        }

        public static IntPtr ToIntPtr(GCHandle value)
        {
            if (s_probeIsActive)
            {
                return s_cookieTable.FindOrAddHandle(value.m_handle);
            }
            return value.m_handle;
        }

        public override int GetHashCode()
        {
            return this.m_handle.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is GCHandle))
            {
                return false;
            }
            GCHandle handle = (GCHandle) o;
            return (this.m_handle == handle.m_handle);
        }

        public static bool operator ==(GCHandle a, GCHandle b)
        {
            return (a.m_handle == b.m_handle);
        }

        public static bool operator !=(GCHandle a, GCHandle b)
        {
            return (a.m_handle != b.m_handle);
        }

        internal IntPtr GetHandleValue()
        {
            return new IntPtr(((int) this.m_handle) & -2);
        }

        internal bool IsPinned()
        {
            return ((((int) this.m_handle) & 1) != 0);
        }

        internal void SetIsPinned()
        {
            this.m_handle = new IntPtr(((int) this.m_handle) | 1);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern IntPtr InternalAlloc(object value, GCHandleType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void InternalFree(IntPtr handle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object InternalGet(IntPtr handle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void InternalSet(IntPtr handle, object value, bool isPinned);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object InternalCompareExchange(IntPtr handle, object value, object oldValue, bool isPinned);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern IntPtr InternalAddrOfPinnedObject(IntPtr handle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void InternalCheckDomain(IntPtr handle);
    }
}

