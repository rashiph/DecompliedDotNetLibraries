namespace System.Data.ProviderBase
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Services;

    internal class WrappedIUnknown : SafeHandle
    {
        internal WrappedIUnknown() : base(IntPtr.Zero, true)
        {
        }

        internal WrappedIUnknown(object unknown) : this()
        {
            if (unknown != null)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    base.handle = Marshal.GetIUnknownForObject(unknown);
                }
            }
        }

        internal object ComWrapper()
        {
            object obj2 = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                obj2 = EnterpriseServicesHelper.WrapIUnknownWithComObject(base.DangerousGetHandle());
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return obj2;
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                Marshal.Release(handle);
            }
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }
    }
}

