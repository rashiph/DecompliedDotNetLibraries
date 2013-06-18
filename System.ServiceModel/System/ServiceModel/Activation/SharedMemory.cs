namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal class SharedMemory : IDisposable
    {
        private SafeFileMappingHandle fileMapping;

        private SharedMemory(SafeFileMappingHandle fileMapping)
        {
            this.fileMapping = fileMapping;
        }

        public static unsafe SharedMemory Create(string name, Guid content, List<SecurityIdentifier> allowedSids)
        {
            SafeFileMappingHandle handle;
            SafeViewOfFileHandle handle2;
            SharedMemory memory2;
            int error = 0;
            byte[] buffer = SecurityDescriptorHelper.FromSecurityIdentifiers(allowedSids, -2147483648);
            UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttributes = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
            fixed (byte* numRef = buffer)
            {
                securityAttributes.lpSecurityDescriptor = (IntPtr) numRef;
                handle = UnsafeNativeMethods.CreateFileMapping((IntPtr) (-1), securityAttributes, 4, 0, sizeof(SharedMemoryContents), name);
                error = Marshal.GetLastWin32Error();
            }
            if (handle.IsInvalid)
            {
                handle.SetHandleAsInvalid();
                handle.Close();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            SharedMemory memory = new SharedMemory(handle);
            GetView(handle, true, out handle2);
            try
            {
                SharedMemoryContents* contentsPtr = (SharedMemoryContents*) handle2.DangerousGetHandle();
                contentsPtr->pipeGuid = content;
                Thread.MemoryBarrier();
                contentsPtr->isInitialized = true;
                memory2 = memory;
            }
            finally
            {
                handle2.Close();
            }
            return memory2;
        }

        public void Dispose()
        {
            if (this.fileMapping != null)
            {
                this.fileMapping.Close();
                this.fileMapping = null;
            }
        }

        private static bool GetView(SafeFileMappingHandle fileMapping, bool writable, out SafeViewOfFileHandle handle)
        {
            handle = UnsafeNativeMethods.MapViewOfFile(fileMapping, writable ? 2 : 4, 0, 0, (IntPtr) sizeof(SharedMemoryContents));
            int error = Marshal.GetLastWin32Error();
            if (!handle.IsInvalid)
            {
                return true;
            }
            handle.SetHandleAsInvalid();
            fileMapping.Close();
            if (writable || (error != 2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            return false;
        }

        public static string Read(string name)
        {
            string str;
            if (!Read(name, out str))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(2));
            }
            return str;
        }

        public static unsafe bool Read(string name, out string content)
        {
            bool flag;
            content = null;
            SafeFileMappingHandle fileMapping = UnsafeNativeMethods.OpenFileMapping(4, false, @"Global\" + name);
            int error = Marshal.GetLastWin32Error();
            if (fileMapping.IsInvalid)
            {
                fileMapping.SetHandleAsInvalid();
                fileMapping.Close();
                if (error != 2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
                return false;
            }
            try
            {
                SafeViewOfFileHandle handle2;
                if (!GetView(fileMapping, false, out handle2))
                {
                    flag = false;
                }
                else
                {
                    try
                    {
                        SharedMemoryContents* handle = (SharedMemoryContents*) handle2.DangerousGetHandle();
                        content = handle->isInitialized ? handle->pipeGuid.ToString() : null;
                        flag = true;
                    }
                    finally
                    {
                        handle2.Close();
                    }
                }
            }
            finally
            {
                fileMapping.Close();
            }
            return flag;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SharedMemoryContents
        {
            public bool isInitialized;
            public Guid pipeGuid;
        }
    }
}

