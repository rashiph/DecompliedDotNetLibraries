namespace System.Threading
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO.Ports;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Permissions;

    [ComVisible(false), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public sealed class Semaphore : WaitHandle
    {
        private static int MAX_PATH = 260;

        private Semaphore(SafeWaitHandle handle)
        {
            base.SafeWaitHandle = handle;
        }

        public Semaphore(int initialCount, int maximumCount) : this(initialCount, maximumCount, null)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Semaphore(int initialCount, int maximumCount, string name)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (maximumCount < 1)
            {
                throw new ArgumentOutOfRangeException("maximumCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (initialCount > maximumCount)
            {
                throw new ArgumentException(SR.GetString("Argument_SemaphoreInitialMaximum"));
            }
            if ((name != null) && (MAX_PATH < name.Length))
            {
                throw new ArgumentException(SR.GetString("Argument_WaitHandleNameTooLong"));
            }
            SafeWaitHandle handle = Microsoft.Win32.SafeNativeMethods.CreateSemaphore(null, initialCount, maximumCount, name);
            if (handle.IsInvalid)
            {
                int num = Marshal.GetLastWin32Error();
                if (((name != null) && (name.Length != 0)) && (6 == num))
                {
                    throw new WaitHandleCannotBeOpenedException(SR.GetString("WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
                }
                InternalResources.WinIOError();
            }
            base.SafeWaitHandle = handle;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Semaphore(int initialCount, int maximumCount, string name, out bool createdNew) : this(initialCount, maximumCount, name, out createdNew, null)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public unsafe Semaphore(int initialCount, int maximumCount, string name, out bool createdNew, SemaphoreSecurity semaphoreSecurity)
        {
            SafeWaitHandle handle;
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (maximumCount < 1)
            {
                throw new ArgumentOutOfRangeException("maximumCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (initialCount > maximumCount)
            {
                throw new ArgumentException(SR.GetString("Argument_SemaphoreInitialMaximum"));
            }
            if ((name != null) && (MAX_PATH < name.Length))
            {
                throw new ArgumentException(SR.GetString("Argument_WaitHandleNameTooLong"));
            }
            if (semaphoreSecurity != null)
            {
                Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES structure = null;
                structure = new Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                fixed (byte* numRef = semaphoreSecurity.GetSecurityDescriptorBinaryForm())
                {
                    structure.lpSecurityDescriptor = new SafeLocalMemHandle((IntPtr) numRef, false);
                    handle = Microsoft.Win32.SafeNativeMethods.CreateSemaphore(structure, initialCount, maximumCount, name);
                }
            }
            else
            {
                handle = Microsoft.Win32.SafeNativeMethods.CreateSemaphore(null, initialCount, maximumCount, name);
            }
            int num = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                if (((name != null) && (name.Length != 0)) && (6 == num))
                {
                    throw new WaitHandleCannotBeOpenedException(SR.GetString("WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
                }
                InternalResources.WinIOError();
            }
            createdNew = num != 0xb7;
            base.SafeWaitHandle = handle;
        }

        public SemaphoreSecurity GetAccessControl()
        {
            return new SemaphoreSecurity(base.SafeWaitHandle, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static Semaphore OpenExisting(string name)
        {
            return OpenExisting(name, SemaphoreRights.Synchronize | SemaphoreRights.Modify);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static Semaphore OpenExisting(string name, SemaphoreRights rights)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", new object[] { "name" }), "name");
            }
            if ((name != null) && (MAX_PATH < name.Length))
            {
                throw new ArgumentException(SR.GetString("Argument_WaitHandleNameTooLong"));
            }
            SafeWaitHandle handle = Microsoft.Win32.SafeNativeMethods.OpenSemaphore((int) rights, false, name);
            if (handle.IsInvalid)
            {
                int num = Marshal.GetLastWin32Error();
                if ((2 == num) || (0x7b == num))
                {
                    throw new WaitHandleCannotBeOpenedException();
                }
                if (((name != null) && (name.Length != 0)) && (6 == num))
                {
                    throw new WaitHandleCannotBeOpenedException(SR.GetString("WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
                }
                InternalResources.WinIOError();
            }
            return new Semaphore(handle);
        }

        [PrePrepareMethod, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public int Release()
        {
            return this.Release(1);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public int Release(int releaseCount)
        {
            int num;
            if (releaseCount < 1)
            {
                throw new ArgumentOutOfRangeException("releaseCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (!Microsoft.Win32.SafeNativeMethods.ReleaseSemaphore(base.SafeWaitHandle, releaseCount, out num))
            {
                throw new SemaphoreFullException();
            }
            return num;
        }

        public void SetAccessControl(SemaphoreSecurity semaphoreSecurity)
        {
            if (semaphoreSecurity == null)
            {
                throw new ArgumentNullException("semaphoreSecurity");
            }
            semaphoreSecurity.Persist(base.SafeWaitHandle);
        }
    }
}

