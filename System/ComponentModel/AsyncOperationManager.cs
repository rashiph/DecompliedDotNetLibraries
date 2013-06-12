namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public static class AsyncOperationManager
    {
        public static AsyncOperation CreateOperation(object userSuppliedState)
        {
            return AsyncOperation.CreateOperation(userSuppliedState, SynchronizationContext);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static System.Threading.SynchronizationContext SynchronizationContext
        {
            get
            {
                if (System.Threading.SynchronizationContext.Current == null)
                {
                    System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Threading.SynchronizationContext());
                }
                return System.Threading.SynchronizationContext.Current;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                System.Threading.SynchronizationContext.SetSynchronizationContext(value);
            }
        }
    }
}

