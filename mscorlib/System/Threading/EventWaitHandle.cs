namespace System.Threading
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class EventWaitHandle : WaitHandle
    {
        [SecurityCritical]
        private EventWaitHandle(SafeWaitHandle handle)
        {
            base.SetHandleInternal(handle);
        }

        [SecuritySafeCritical]
        public EventWaitHandle(bool initialState, EventResetMode mode) : this(initialState, mode, null)
        {
        }

        [SecurityCritical]
        public EventWaitHandle(bool initialState, EventResetMode mode, string name)
        {
            if ((name != null) && (260 < name.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", new object[] { name }));
            }
            SafeWaitHandle handle = null;
            switch (mode)
            {
                case EventResetMode.AutoReset:
                    handle = Win32Native.CreateEvent(null, false, initialState, name);
                    break;

                case EventResetMode.ManualReset:
                    handle = Win32Native.CreateEvent(null, true, initialState, name);
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag", new object[] { name }));
            }
            if (handle.IsInvalid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                handle.SetHandleAsInvalid();
                if (((name != null) && (name.Length != 0)) && (6 == errorCode))
                {
                    throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
                }
                __Error.WinIOError(errorCode, name);
            }
            base.SetHandleInternal(handle);
        }

        [SecurityCritical]
        public EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew) : this(initialState, mode, name, out createdNew, null)
        {
        }

        [SecurityCritical]
        public unsafe EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew, EventWaitHandleSecurity eventSecurity)
        {
            bool flag;
            if ((name != null) && (260 < name.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", new object[] { name }));
            }
            Win32Native.SECURITY_ATTRIBUTES structure = null;
            if (eventSecurity != null)
            {
                structure = new Win32Native.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                byte[] securityDescriptorBinaryForm = eventSecurity.GetSecurityDescriptorBinaryForm();
                byte* pDest = stackalloc byte[(IntPtr) securityDescriptorBinaryForm.Length];
                Buffer.memcpy(securityDescriptorBinaryForm, 0, pDest, 0, securityDescriptorBinaryForm.Length);
                structure.pSecurityDescriptor = pDest;
            }
            SafeWaitHandle handle = null;
            switch (mode)
            {
                case EventResetMode.AutoReset:
                    flag = false;
                    break;

                case EventResetMode.ManualReset:
                    flag = true;
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag", new object[] { name }));
            }
            handle = Win32Native.CreateEvent(structure, flag, initialState, name);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                handle.SetHandleAsInvalid();
                if (((name != null) && (name.Length != 0)) && (6 == errorCode))
                {
                    throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
                }
                __Error.WinIOError(errorCode, name);
            }
            createdNew = errorCode != 0xb7;
            base.SetHandleInternal(handle);
        }

        [SecuritySafeCritical]
        public EventWaitHandleSecurity GetAccessControl()
        {
            return new EventWaitHandleSecurity(base.safeWaitHandle, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        [SecurityCritical]
        public static EventWaitHandle OpenExisting(string name)
        {
            return OpenExisting(name, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify);
        }

        [SecurityCritical]
        public static EventWaitHandle OpenExisting(string name, EventWaitHandleRights rights)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", Environment.GetResourceString("ArgumentNull_WithParamName"));
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if ((name != null) && (260 < name.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", new object[] { name }));
            }
            SafeWaitHandle handle = Win32Native.OpenEvent((int) rights, false, name);
            if (handle.IsInvalid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if ((2 == errorCode) || (0x7b == errorCode))
                {
                    throw new WaitHandleCannotBeOpenedException();
                }
                if (((name != null) && (name.Length != 0)) && (6 == errorCode))
                {
                    throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
                }
                __Error.WinIOError(errorCode, "");
            }
            return new EventWaitHandle(handle);
        }

        [SecuritySafeCritical]
        public bool Reset()
        {
            bool flag = Win32Native.ResetEvent(base.safeWaitHandle);
            if (!flag)
            {
                __Error.WinIOError();
            }
            return flag;
        }

        [SecuritySafeCritical]
        public bool Set()
        {
            bool flag = Win32Native.SetEvent(base.safeWaitHandle);
            if (!flag)
            {
                __Error.WinIOError();
            }
            return flag;
        }

        [SecuritySafeCritical]
        public void SetAccessControl(EventWaitHandleSecurity eventSecurity)
        {
            if (eventSecurity == null)
            {
                throw new ArgumentNullException("eventSecurity");
            }
            eventSecurity.Persist(base.safeWaitHandle);
        }
    }
}

