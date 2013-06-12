namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical(SecurityCriticalScope.Everything), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true), SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode=true)]
    public abstract class SafeNCryptHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeNCryptHandle m_holder;
        private OwnershipState m_ownershipState;

        protected SafeNCryptHandle() : base(true)
        {
        }

        internal T Duplicate<T>() where T: SafeNCryptHandle, new()
        {
            if (this.m_ownershipState == OwnershipState.Owner)
            {
                return this.DuplicateOwnerHandle<T>();
            }
            return this.DuplicateDuplicatedHandle<T>();
        }

        private T DuplicateDuplicatedHandle<T>() where T: SafeNCryptHandle, new()
        {
            bool success = false;
            T local = Activator.CreateInstance<T>();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.Holder.DangerousAddRef(ref success);
                local.SetHandle(this.Holder.DangerousGetHandle());
                local.Holder = this.Holder;
            }
            return local;
        }

        private T DuplicateOwnerHandle<T>() where T: SafeNCryptHandle, new()
        {
            bool success = false;
            T local = Activator.CreateInstance<T>();
            T local2 = Activator.CreateInstance<T>();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                local.m_ownershipState = OwnershipState.Holder;
                local.SetHandle(base.DangerousGetHandle());
                GC.SuppressFinalize(local);
                this.Holder = local;
                local.DangerousAddRef(ref success);
                local2.SetHandle(local.DangerousGetHandle());
                local2.Holder = local;
            }
            return local2;
        }

        protected override bool ReleaseHandle()
        {
            if (this.m_ownershipState == OwnershipState.Duplicate)
            {
                this.Holder.DangerousRelease();
                return true;
            }
            return this.ReleaseNativeHandle();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected abstract bool ReleaseNativeHandle();

        private SafeNCryptHandle Holder
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this.m_holder;
            }
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            set
            {
                this.m_holder = value;
                this.m_ownershipState = OwnershipState.Duplicate;
            }
        }

        private enum OwnershipState
        {
            Owner,
            Duplicate,
            Holder
        }
    }
}

