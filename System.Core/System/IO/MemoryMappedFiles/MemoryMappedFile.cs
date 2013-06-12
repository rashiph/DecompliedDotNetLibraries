namespace System.IO.MemoryMappedFiles
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Threading;

    public class MemoryMappedFile : IDisposable
    {
        private FileStream _fileStream;
        private Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle _handle;
        private bool _leaveOpen;
        internal const int DefaultSize = 0;

        [SecurityCritical]
        private MemoryMappedFile(Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle)
        {
            this._handle = handle;
            this._leaveOpen = true;
        }

        [SecurityCritical]
        private MemoryMappedFile(Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle, FileStream fileStream, bool leaveOpen)
        {
            this._handle = handle;
            this._fileStream = fileStream;
            this._leaveOpen = leaveOpen;
        }

        private static void CleanupFile(FileStream fileStream, bool existed, string path)
        {
            fileStream.Close();
            if (!existed)
            {
                File.Delete(path);
            }
        }

        [SecurityCritical]
        private static Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle CreateCore(SafeFileHandle fileHandle, string mapName, HandleInheritability inheritability, MemoryMappedFileSecurity memoryMappedFileSecurity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, long capacity)
        {
            Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle = null;
            object obj2;
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES lpAttributes = GetSecAttrs(inheritability, memoryMappedFileSecurity, out obj2);
            int dwMaximumSizeLow = (int) (((ulong) capacity) & 0xffffffffL);
            int dwMaximumSizeHigh = (int) (capacity >> 0x20);
            try
            {
                handle = Microsoft.Win32.UnsafeNativeMethods.CreateFileMapping(fileHandle, lpAttributes, GetPageAccess(access) | options, dwMaximumSizeHigh, dwMaximumSizeLow, mapName);
                int errorCode = Marshal.GetLastWin32Error();
                if (!handle.IsInvalid && (errorCode == 0xb7))
                {
                    handle.Dispose();
                    System.IO.__Error.WinIOError(errorCode, string.Empty);
                    return handle;
                }
                if (handle.IsInvalid)
                {
                    System.IO.__Error.WinIOError(errorCode, string.Empty);
                }
            }
            finally
            {
                if (obj2 != null)
                {
                    ((GCHandle) obj2).Free();
                }
            }
            return handle;
        }

        public static MemoryMappedFile CreateFromFile(string path)
        {
            return CreateFromFile(path, FileMode.Open, null, 0L, MemoryMappedFileAccess.ReadWrite);
        }

        public static MemoryMappedFile CreateFromFile(string path, FileMode mode)
        {
            return CreateFromFile(path, mode, null, 0L, MemoryMappedFileAccess.ReadWrite);
        }

        public static MemoryMappedFile CreateFromFile(string path, FileMode mode, string mapName)
        {
            return CreateFromFile(path, mode, mapName, 0L, MemoryMappedFileAccess.ReadWrite);
        }

        public static MemoryMappedFile CreateFromFile(string path, FileMode mode, string mapName, long capacity)
        {
            return CreateFromFile(path, mode, mapName, capacity, MemoryMappedFileAccess.ReadWrite);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateFromFile(string path, FileMode mode, string mapName, long capacity, MemoryMappedFileAccess access)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if ((mapName != null) && (mapName.Length == 0))
            {
                throw new ArgumentException(System.SR.GetString("Argument_MapNameEmptyString"));
            }
            if (capacity < 0L)
            {
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_PositiveOrDefaultCapacityRequired"));
            }
            if ((access < MemoryMappedFileAccess.ReadWrite) || (access > MemoryMappedFileAccess.ReadWriteExecute))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if (mode == FileMode.Append)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NewMMFAppendModeNotAllowed"), "mode");
            }
            if (access == MemoryMappedFileAccess.Write)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NewMMFWriteAccessNotAllowed"), "access");
            }
            bool existed = File.Exists(path);
            FileStream fileStream = new FileStream(path, mode, GetFileStreamFileSystemRights(access), FileShare.None, 0x1000, FileOptions.None);
            if ((capacity == 0L) && (fileStream.Length == 0L))
            {
                CleanupFile(fileStream, existed, path);
                throw new ArgumentException(System.SR.GetString("Argument_EmptyFile"));
            }
            if ((access == MemoryMappedFileAccess.Read) && (capacity > fileStream.Length))
            {
                CleanupFile(fileStream, existed, path);
                throw new ArgumentException(System.SR.GetString("Argument_ReadAccessWithLargeCapacity"));
            }
            if (capacity == 0L)
            {
                capacity = fileStream.Length;
            }
            if (fileStream.Length > capacity)
            {
                CleanupFile(fileStream, existed, path);
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_CapacityGEFileSizeRequired"));
            }
            Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle = null;
            try
            {
                handle = CreateCore(fileStream.SafeFileHandle, mapName, HandleInheritability.None, null, access, MemoryMappedFileOptions.None, capacity);
            }
            catch
            {
                CleanupFile(fileStream, existed, path);
                throw;
            }
            return new MemoryMappedFile(handle, fileStream, false);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateFromFile(FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability, bool leaveOpen)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException("fileStream", System.SR.GetString("ArgumentNull_FileStream"));
            }
            if ((mapName != null) && (mapName.Length == 0))
            {
                throw new ArgumentException(System.SR.GetString("Argument_MapNameEmptyString"));
            }
            if (capacity < 0L)
            {
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_PositiveOrDefaultCapacityRequired"));
            }
            if ((capacity == 0L) && (fileStream.Length == 0L))
            {
                throw new ArgumentException(System.SR.GetString("Argument_EmptyFile"));
            }
            if ((access < MemoryMappedFileAccess.ReadWrite) || (access > MemoryMappedFileAccess.ReadWriteExecute))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if (access == MemoryMappedFileAccess.Write)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NewMMFWriteAccessNotAllowed"), "access");
            }
            if ((access == MemoryMappedFileAccess.Read) && (capacity > fileStream.Length))
            {
                throw new ArgumentException(System.SR.GetString("Argument_ReadAccessWithLargeCapacity"));
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability");
            }
            fileStream.Flush();
            if (capacity == 0L)
            {
                capacity = fileStream.Length;
            }
            if (fileStream.Length > capacity)
            {
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_CapacityGEFileSizeRequired"));
            }
            return new MemoryMappedFile(CreateCore(fileStream.SafeFileHandle, mapName, inheritability, memoryMappedFileSecurity, access, MemoryMappedFileOptions.None, capacity), fileStream, leaveOpen);
        }

        public static MemoryMappedFile CreateNew(string mapName, long capacity)
        {
            return CreateNew(mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, null, HandleInheritability.None);
        }

        public static MemoryMappedFile CreateNew(string mapName, long capacity, MemoryMappedFileAccess access)
        {
            return CreateNew(mapName, capacity, access, MemoryMappedFileOptions.None, null, HandleInheritability.None);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateNew(string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability)
        {
            if ((mapName != null) && (mapName.Length == 0))
            {
                throw new ArgumentException(System.SR.GetString("Argument_MapNameEmptyString"));
            }
            if (capacity <= 0L)
            {
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_NeedPositiveNumber"));
            }
            if ((IntPtr.Size == 4) && (capacity > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed"));
            }
            if ((access < MemoryMappedFileAccess.ReadWrite) || (access > MemoryMappedFileAccess.ReadWriteExecute))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if (access == MemoryMappedFileAccess.Write)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NewMMFWriteAccessNotAllowed"), "access");
            }
            if ((options & ~MemoryMappedFileOptions.DelayAllocatePages) != MemoryMappedFileOptions.None)
            {
                throw new ArgumentOutOfRangeException("options");
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability");
            }
            return new MemoryMappedFile(CreateCore(new SafeFileHandle(new IntPtr(-1), true), mapName, inheritability, memoryMappedFileSecurity, access, options, capacity));
        }

        public static MemoryMappedFile CreateOrOpen(string mapName, long capacity)
        {
            return CreateOrOpen(mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, null, HandleInheritability.None);
        }

        public static MemoryMappedFile CreateOrOpen(string mapName, long capacity, MemoryMappedFileAccess access)
        {
            return CreateOrOpen(mapName, capacity, access, MemoryMappedFileOptions.None, null, HandleInheritability.None);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateOrOpen(string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability)
        {
            Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle;
            if (mapName == null)
            {
                throw new ArgumentNullException("mapName", System.SR.GetString("ArgumentNull_MapName"));
            }
            if (mapName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_MapNameEmptyString"));
            }
            if (capacity <= 0L)
            {
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_NeedPositiveNumber"));
            }
            if ((IntPtr.Size == 4) && (capacity > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("capacity", System.SR.GetString("ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed"));
            }
            if ((access < MemoryMappedFileAccess.ReadWrite) || (access > MemoryMappedFileAccess.ReadWriteExecute))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if ((options & ~MemoryMappedFileOptions.DelayAllocatePages) != MemoryMappedFileOptions.None)
            {
                throw new ArgumentOutOfRangeException("options");
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability");
            }
            if (access == MemoryMappedFileAccess.Write)
            {
                handle = OpenCore(mapName, inheritability, GetFileMapAccess(access), true);
            }
            else
            {
                handle = CreateOrOpenCore(new SafeFileHandle(new IntPtr(-1), true), mapName, inheritability, memoryMappedFileSecurity, access, options, capacity);
            }
            return new MemoryMappedFile(handle);
        }

        [SecurityCritical]
        private static Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle CreateOrOpenCore(SafeFileHandle fileHandle, string mapName, HandleInheritability inheritability, MemoryMappedFileSecurity memoryMappedFileSecurity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, long capacity)
        {
            Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle = null;
            object obj2;
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES lpAttributes = GetSecAttrs(inheritability, memoryMappedFileSecurity, out obj2);
            int dwMaximumSizeLow = (int) (((ulong) capacity) & 0xffffffffL);
            int dwMaximumSizeHigh = (int) (capacity >> 0x20);
            try
            {
                int num3 = 14;
                int millisecondsTimeout = 0;
                while (num3 > 0)
                {
                    handle = Microsoft.Win32.UnsafeNativeMethods.CreateFileMapping(fileHandle, lpAttributes, GetPageAccess(access) | options, dwMaximumSizeHigh, dwMaximumSizeLow, mapName);
                    int errorCode = Marshal.GetLastWin32Error();
                    if (!handle.IsInvalid)
                    {
                        break;
                    }
                    if (errorCode != 5)
                    {
                        System.IO.__Error.WinIOError(errorCode, string.Empty);
                    }
                    handle.SetHandleAsInvalid();
                    handle = Microsoft.Win32.UnsafeNativeMethods.OpenFileMapping(GetFileMapAccess(access), (inheritability & HandleInheritability.Inheritable) != HandleInheritability.None, mapName);
                    int num6 = Marshal.GetLastWin32Error();
                    if (!handle.IsInvalid)
                    {
                        break;
                    }
                    if (num6 != 2)
                    {
                        System.IO.__Error.WinIOError(num6, string.Empty);
                    }
                    num3--;
                    if (millisecondsTimeout == 0)
                    {
                        millisecondsTimeout = 10;
                    }
                    else
                    {
                        Thread.Sleep(millisecondsTimeout);
                        millisecondsTimeout *= 2;
                    }
                }
                if ((handle == null) || handle.IsInvalid)
                {
                    throw new InvalidOperationException(System.SR.GetString("InvalidOperation_CantCreateFileMapping"));
                }
                return handle;
            }
            finally
            {
                if (obj2 != null)
                {
                    ((GCHandle) obj2).Free();
                }
            }
            return handle;
        }

        public MemoryMappedViewAccessor CreateViewAccessor()
        {
            return this.CreateViewAccessor(0L, 0L, MemoryMappedFileAccess.ReadWrite);
        }

        public MemoryMappedViewAccessor CreateViewAccessor(long offset, long size)
        {
            return this.CreateViewAccessor(offset, size, MemoryMappedFileAccess.ReadWrite);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public MemoryMappedViewAccessor CreateViewAccessor(long offset, long size, MemoryMappedFileAccess access)
        {
            if (offset < 0L)
            {
                throw new ArgumentOutOfRangeException("offset", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (size < 0L)
            {
                throw new ArgumentOutOfRangeException("size", System.SR.GetString("ArgumentOutOfRange_PositiveOrDefaultSizeRequired"));
            }
            if ((access < MemoryMappedFileAccess.ReadWrite) || (access > MemoryMappedFileAccess.ReadWriteExecute))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if ((IntPtr.Size == 4) && (size > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("size", System.SR.GetString("ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed"));
            }
            return new MemoryMappedViewAccessor(MemoryMappedView.CreateView(this._handle, access, offset, size));
        }

        public MemoryMappedViewStream CreateViewStream()
        {
            return this.CreateViewStream(0L, 0L, MemoryMappedFileAccess.ReadWrite);
        }

        public MemoryMappedViewStream CreateViewStream(long offset, long size)
        {
            return this.CreateViewStream(offset, size, MemoryMappedFileAccess.ReadWrite);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public MemoryMappedViewStream CreateViewStream(long offset, long size, MemoryMappedFileAccess access)
        {
            if (offset < 0L)
            {
                throw new ArgumentOutOfRangeException("offset", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (size < 0L)
            {
                throw new ArgumentOutOfRangeException("size", System.SR.GetString("ArgumentOutOfRange_PositiveOrDefaultSizeRequired"));
            }
            if ((access < MemoryMappedFileAccess.ReadWrite) || (access > MemoryMappedFileAccess.ReadWriteExecute))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if ((IntPtr.Size == 4) && (size > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("size", System.SR.GetString("ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed"));
            }
            return new MemoryMappedViewStream(MemoryMappedView.CreateView(this._handle, access, offset, size));
        }

        [SecurityCritical]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecurityCritical]
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if ((this._handle != null) && !this._handle.IsClosed)
                {
                    this._handle.Dispose();
                }
            }
            finally
            {
                if ((this._fileStream != null) && !this._leaveOpen)
                {
                    this._fileStream.Dispose();
                }
            }
        }

        [SecurityCritical]
        public MemoryMappedFileSecurity GetAccessControl()
        {
            if (this._handle.IsClosed)
            {
                System.IO.__Error.FileNotOpen();
            }
            return new MemoryMappedFileSecurity(this._handle, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        internal static FileAccess GetFileAccess(MemoryMappedFileAccess access)
        {
            if (access == MemoryMappedFileAccess.Read)
            {
                return FileAccess.Read;
            }
            if (access == MemoryMappedFileAccess.Write)
            {
                return FileAccess.Write;
            }
            if (access != MemoryMappedFileAccess.ReadWrite)
            {
                if (access == MemoryMappedFileAccess.CopyOnWrite)
                {
                    return FileAccess.ReadWrite;
                }
                if (access == MemoryMappedFileAccess.ReadExecute)
                {
                    return FileAccess.Read;
                }
                if (access != MemoryMappedFileAccess.ReadWriteExecute)
                {
                    throw new ArgumentOutOfRangeException("access");
                }
            }
            return FileAccess.ReadWrite;
        }

        internal static int GetFileMapAccess(MemoryMappedFileAccess access)
        {
            if (access == MemoryMappedFileAccess.Read)
            {
                return 4;
            }
            if (access == MemoryMappedFileAccess.Write)
            {
                return 2;
            }
            if (access == MemoryMappedFileAccess.ReadWrite)
            {
                return 6;
            }
            if (access == MemoryMappedFileAccess.CopyOnWrite)
            {
                return 1;
            }
            if (access == MemoryMappedFileAccess.ReadExecute)
            {
                return 0x24;
            }
            if (access != MemoryMappedFileAccess.ReadWriteExecute)
            {
                throw new ArgumentOutOfRangeException("access");
            }
            return 0x26;
        }

        private static FileSystemRights GetFileStreamFileSystemRights(MemoryMappedFileAccess access)
        {
            switch (access)
            {
                case MemoryMappedFileAccess.ReadWrite:
                    return (FileSystemRights.CreateFiles | FileSystemRights.ListDirectory);

                case MemoryMappedFileAccess.Read:
                case MemoryMappedFileAccess.CopyOnWrite:
                    return FileSystemRights.ListDirectory;

                case MemoryMappedFileAccess.Write:
                    return FileSystemRights.CreateFiles;

                case MemoryMappedFileAccess.ReadExecute:
                    return (FileSystemRights.ExecuteFile | FileSystemRights.ListDirectory);

                case MemoryMappedFileAccess.ReadWriteExecute:
                    return (FileSystemRights.ExecuteFile | FileSystemRights.CreateFiles | FileSystemRights.ListDirectory);
            }
            throw new ArgumentOutOfRangeException("access");
        }

        internal static int GetPageAccess(MemoryMappedFileAccess access)
        {
            if (access == MemoryMappedFileAccess.Read)
            {
                return 2;
            }
            if (access == MemoryMappedFileAccess.ReadWrite)
            {
                return 4;
            }
            if (access == MemoryMappedFileAccess.CopyOnWrite)
            {
                return 8;
            }
            if (access == MemoryMappedFileAccess.ReadExecute)
            {
                return 0x20;
            }
            if (access != MemoryMappedFileAccess.ReadWriteExecute)
            {
                throw new ArgumentOutOfRangeException("access");
            }
            return 0x40;
        }

        [SecurityCritical]
        private static unsafe Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability, MemoryMappedFileSecurity memoryMappedFileSecurity, out object pinningHandle)
        {
            pinningHandle = null;
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES structure = null;
            if (((inheritability & HandleInheritability.Inheritable) != HandleInheritability.None) || (memoryMappedFileSecurity != null))
            {
                structure = new Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                if ((inheritability & HandleInheritability.Inheritable) != HandleInheritability.None)
                {
                    structure.bInheritHandle = 1;
                }
                if (memoryMappedFileSecurity == null)
                {
                    return structure;
                }
                byte[] securityDescriptorBinaryForm = memoryMappedFileSecurity.GetSecurityDescriptorBinaryForm();
                pinningHandle = GCHandle.Alloc(securityDescriptorBinaryForm, GCHandleType.Pinned);
                fixed (byte* numRef = securityDescriptorBinaryForm)
                {
                    structure.pSecurityDescriptor = numRef;
                }
            }
            return structure;
        }

        [SecurityCritical]
        internal static int GetSystemPageAllocationGranularity()
        {
            Microsoft.Win32.UnsafeNativeMethods.SYSTEM_INFO lpSystemInfo = new Microsoft.Win32.UnsafeNativeMethods.SYSTEM_INFO();
            Microsoft.Win32.UnsafeNativeMethods.GetSystemInfo(ref lpSystemInfo);
            return lpSystemInfo.dwAllocationGranularity;
        }

        [SecurityCritical]
        private static Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle OpenCore(string mapName, HandleInheritability inheritability, int desiredAccessRights, bool createOrOpen)
        {
            Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle = Microsoft.Win32.UnsafeNativeMethods.OpenFileMapping(desiredAccessRights, (inheritability & HandleInheritability.Inheritable) != HandleInheritability.None, mapName);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                if (createOrOpen && (errorCode == 2))
                {
                    throw new ArgumentException(System.SR.GetString("Argument_NewMMFWriteAccessNotAllowed"), "access");
                }
                System.IO.__Error.WinIOError(errorCode, string.Empty);
            }
            return handle;
        }

        public static MemoryMappedFile OpenExisting(string mapName)
        {
            return OpenExisting(mapName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
        }

        public static MemoryMappedFile OpenExisting(string mapName, MemoryMappedFileRights desiredAccessRights)
        {
            return OpenExisting(mapName, desiredAccessRights, HandleInheritability.None);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile OpenExisting(string mapName, MemoryMappedFileRights desiredAccessRights, HandleInheritability inheritability)
        {
            if (mapName == null)
            {
                throw new ArgumentNullException("mapName", System.SR.GetString("ArgumentNull_MapName"));
            }
            if (mapName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_MapNameEmptyString"));
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability");
            }
            if ((desiredAccessRights & ~(MemoryMappedFileRights.AccessSystemSecurity | MemoryMappedFileRights.FullControl)) != 0)
            {
                throw new ArgumentOutOfRangeException("desiredAccessRights");
            }
            return new MemoryMappedFile(OpenCore(mapName, inheritability, (int) desiredAccessRights, false));
        }

        [SecurityCritical]
        public void SetAccessControl(MemoryMappedFileSecurity memoryMappedFileSecurity)
        {
            if (memoryMappedFileSecurity == null)
            {
                throw new ArgumentNullException("memoryMappedFileSecurity");
            }
            if (this._handle.IsClosed)
            {
                System.IO.__Error.FileNotOpen();
            }
            memoryMappedFileSecurity.PersistHandle(this._handle);
        }

        public Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle SafeMemoryMappedFileHandle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return this._handle;
            }
        }
    }
}

