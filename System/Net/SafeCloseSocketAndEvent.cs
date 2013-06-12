namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeCloseSocketAndEvent : SafeCloseSocket
    {
        private AutoResetEvent waitHandle;

        internal SafeCloseSocketAndEvent()
        {
        }

        internal static void CompleteInitialization(SafeCloseSocketAndEvent socketAndEventHandle)
        {
            SafeWaitHandle safeWaitHandle = socketAndEventHandle.waitHandle.SafeWaitHandle;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                safeWaitHandle.DangerousAddRef(ref success);
            }
            catch
            {
                if (success)
                {
                    safeWaitHandle.DangerousRelease();
                    socketAndEventHandle.waitHandle = null;
                    success = false;
                }
            }
            finally
            {
                if (success)
                {
                    safeWaitHandle.Dispose();
                }
            }
        }

        internal static SafeCloseSocketAndEvent CreateWSASocketWithEvent(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, bool autoReset, bool signaled)
        {
            SafeCloseSocketAndEvent target = new SafeCloseSocketAndEvent();
            SafeCloseSocket.CreateSocket(SafeCloseSocket.InnerSafeCloseSocket.CreateWSASocket(addressFamily, socketType, protocolType), target);
            if (target.IsInvalid)
            {
                throw new SocketException();
            }
            target.waitHandle = new AutoResetEvent(false);
            CompleteInitialization(target);
            return target;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void DeleteEvent()
        {
            try
            {
                if (this.waitHandle != null)
                {
                    this.waitHandle.SafeWaitHandle.DangerousRelease();
                }
            }
            catch
            {
            }
        }

        internal WaitHandle GetEventHandle()
        {
            return this.waitHandle;
        }

        protected override bool ReleaseHandle()
        {
            bool flag = base.ReleaseHandle();
            this.DeleteEvent();
            return flag;
        }
    }
}

