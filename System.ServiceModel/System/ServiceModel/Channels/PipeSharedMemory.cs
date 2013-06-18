namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;

    internal class PipeSharedMemory : IDisposable
    {
        private SafeFileMappingHandle fileMapping;
        private string pipeName;
        private Uri pipeUri;

        private PipeSharedMemory(SafeFileMappingHandle fileMapping, Uri pipeUri) : this(fileMapping, pipeUri, null)
        {
        }

        private PipeSharedMemory(SafeFileMappingHandle fileMapping, Uri pipeUri, string pipeName)
        {
            this.pipeName = pipeName;
            this.fileMapping = fileMapping;
            this.pipeUri = pipeUri;
        }

        private static string BuildPipeName(Guid guid)
        {
            return (@"\\.\pipe\" + guid.ToString());
        }

        public static PipeSharedMemory Create(List<SecurityIdentifier> allowedSids, Uri pipeUri, string sharedMemoryName)
        {
            PipeSharedMemory memory;
            if (!TryCreate(allowedSids, pipeUri, sharedMemoryName, out memory))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameInUseException(5, pipeUri));
            }
            return memory;
        }

        private static Exception CreatePipeNameCannotBeAccessedException(int error, Uri pipeUri)
        {
            return new AddressAccessDeniedException(System.ServiceModel.SR.GetString("PipeNameCanNotBeAccessed2", new object[] { pipeUri.AbsoluteUri }), new PipeException(System.ServiceModel.SR.GetString("PipeNameCanNotBeAccessed", new object[] { PipeError.GetErrorString(error) }), error));
        }

        public static Exception CreatePipeNameInUseException(int error, Uri pipeUri)
        {
            Exception innerException = new PipeException(System.ServiceModel.SR.GetString("PipeNameInUse", new object[] { pipeUri.AbsoluteUri }), error);
            return new AddressAlreadyInUseException(innerException.Message, innerException);
        }

        public void Dispose()
        {
            if (this.fileMapping != null)
            {
                this.fileMapping.Close();
                this.fileMapping = null;
            }
        }

        private SafeViewOfFileHandle GetView(bool writable)
        {
            SafeViewOfFileHandle handle = UnsafeNativeMethods.MapViewOfFile(this.fileMapping, writable ? 2 : 4, 0, 0, (IntPtr) sizeof(SharedMemoryContents));
            if (handle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                handle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameCannotBeAccessedException(error, this.pipeUri));
            }
            return handle;
        }

        private unsafe void InitializeContents(Guid pipeGuid)
        {
            SafeViewOfFileHandle view = this.GetView(true);
            try
            {
                SharedMemoryContents* handle = (SharedMemoryContents*) view.DangerousGetHandle();
                handle->pipeGuid = pipeGuid;
                Thread.MemoryBarrier();
                handle->isInitialized = true;
            }
            finally
            {
                view.Close();
            }
        }

        public static PipeSharedMemory Open(string sharedMemoryName, Uri pipeUri)
        {
            SafeFileMappingHandle fileMapping = UnsafeNativeMethods.OpenFileMapping(4, false, sharedMemoryName);
            if (!fileMapping.IsInvalid)
            {
                return new PipeSharedMemory(fileMapping, pipeUri);
            }
            int error = Marshal.GetLastWin32Error();
            fileMapping.SetHandleAsInvalid();
            if (error != 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameCannotBeAccessedException(error, pipeUri));
            }
            fileMapping = UnsafeNativeMethods.OpenFileMapping(4, false, @"Global\" + sharedMemoryName);
            if (!fileMapping.IsInvalid)
            {
                return new PipeSharedMemory(fileMapping, pipeUri);
            }
            error = Marshal.GetLastWin32Error();
            fileMapping.SetHandleAsInvalid();
            if (error != 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameCannotBeAccessedException(error, pipeUri));
            }
            return null;
        }

        public static unsafe bool TryCreate(List<SecurityIdentifier> allowedSids, Uri pipeUri, string sharedMemoryName, out PipeSharedMemory result)
        {
            byte[] buffer;
            SafeFileMappingHandle handle;
            int num;
            bool flag2;
            Guid guid = Guid.NewGuid();
            string pipeName = BuildPipeName(guid);
            try
            {
                buffer = SecurityDescriptorHelper.FromSecurityIdentifiers(allowedSids, -2147483648);
            }
            catch (Win32Exception exception)
            {
                Exception innerException = new PipeException(exception.Message, exception);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(innerException.Message, innerException));
            }
            result = null;
            fixed (byte* numRef = buffer)
            {
                UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttributes = new UnsafeNativeMethods.SECURITY_ATTRIBUTES {
                    lpSecurityDescriptor = numRef
                };
                handle = UnsafeNativeMethods.CreateFileMapping((IntPtr) (-1), securityAttributes, 4, 0, sizeof(SharedMemoryContents), sharedMemoryName);
                num = Marshal.GetLastWin32Error();
            }
            if (handle.IsInvalid)
            {
                handle.SetHandleAsInvalid();
                if (num == 5)
                {
                    return false;
                }
                Exception exception3 = new PipeException(System.ServiceModel.SR.GetString("PipeNameCantBeReserved", new object[] { pipeUri.AbsoluteUri, PipeError.GetErrorString(num) }), num);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAccessDeniedException(exception3.Message, exception3));
            }
            if (num == 0xb7)
            {
                handle.Close();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameInUseException(num, pipeUri));
            }
            PipeSharedMemory memory = new PipeSharedMemory(handle, pipeUri, pipeName);
            bool flag = true;
            try
            {
                memory.InitializeContents(guid);
                flag = false;
                result = memory;
                flag2 = true;
            }
            finally
            {
                if (flag)
                {
                    memory.Dispose();
                }
            }
            return flag2;
        }

        public string PipeName
        {
            get
            {
                if (this.pipeName == null)
                {
                    SafeViewOfFileHandle view = this.GetView(false);
                    try
                    {
                        SharedMemoryContents* handle = (SharedMemoryContents*) view.DangerousGetHandle();
                        if (handle->isInitialized)
                        {
                            Thread.MemoryBarrier();
                            this.pipeName = BuildPipeName(handle->pipeGuid);
                        }
                    }
                    finally
                    {
                        view.Close();
                    }
                }
                return this.pipeName;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SharedMemoryContents
        {
            public bool isInitialized;
            public Guid pipeGuid;
        }
    }
}

