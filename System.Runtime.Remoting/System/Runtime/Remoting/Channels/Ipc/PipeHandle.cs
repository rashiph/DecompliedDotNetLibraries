namespace System.Runtime.Remoting.Channels.Ipc
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime;

    internal class PipeHandle : CriticalHandleMinusOneIsInvalid
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PipeHandle()
        {
        }

        internal PipeHandle(IntPtr handle)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return (NativePipe.CloseHandle(base.handle) != 0);
        }

        public IntPtr Handle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return base.handle;
            }
        }
    }
}

