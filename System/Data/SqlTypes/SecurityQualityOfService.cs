namespace System.Data.SqlTypes
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class SecurityQualityOfService : SafeHandleZeroOrMinusOneIsInvalid
    {
        private GCHandle m_hQos;
        private UnsafeNativeMethods.SECURITY_QUALITY_OF_SERVICE m_qos;

        public SecurityQualityOfService(UnsafeNativeMethods.SecurityImpersonationLevel impersonationLevel, bool effectiveOnly, bool dynamicTrackingMode) : base(true)
        {
            this.Initialize(impersonationLevel, effectiveOnly, dynamicTrackingMode);
        }

        internal void Initialize(UnsafeNativeMethods.SecurityImpersonationLevel impersonationLevel, bool effectiveOnly, bool dynamicTrackingMode)
        {
            this.m_qos.length = (uint) Marshal.SizeOf(typeof(UnsafeNativeMethods.SECURITY_QUALITY_OF_SERVICE));
            this.m_qos.impersonationLevel = (int) impersonationLevel;
            this.m_qos.effectiveOnly = effectiveOnly ? ((byte) 1) : ((byte) 0);
            this.m_qos.contextDynamicTrackingMode = dynamicTrackingMode ? ((byte) 1) : ((byte) 0);
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.m_hQos = GCHandle.Alloc(this.m_qos, GCHandleType.Pinned);
                zero = this.m_hQos.AddrOfPinnedObject();
                if (zero != IntPtr.Zero)
                {
                    base.SetHandle(zero);
                }
            }
        }

        protected override bool ReleaseHandle()
        {
            if (this.m_hQos.IsAllocated)
            {
                this.m_hQos.Free();
            }
            base.handle = IntPtr.Zero;
            return true;
        }
    }
}

