namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class StrongNameUtils
    {
        internal static StrongNameLevel GetAssemblyStrongNameLevel(string assemblyPath)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(assemblyPath, "assemblyPath");
            StrongNameLevel unknown = StrongNameLevel.Unknown;
            IntPtr invalidIntPtr = Microsoft.Build.Tasks.NativeMethods.InvalidIntPtr;
            try
            {
                invalidIntPtr = Microsoft.Build.Tasks.NativeMethods.CreateFile(assemblyPath, 0x80000000, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                if (invalidIntPtr == Microsoft.Build.Tasks.NativeMethods.InvalidIntPtr)
                {
                    return unknown;
                }
                if (Microsoft.Build.Tasks.NativeMethods.GetFileType(invalidIntPtr) != 1)
                {
                    return unknown;
                }
                IntPtr zero = IntPtr.Zero;
                try
                {
                    zero = Microsoft.Build.Tasks.NativeMethods.CreateFileMapping(invalidIntPtr, IntPtr.Zero, 2, 0, 0, null);
                    if (zero != IntPtr.Zero)
                    {
                        IntPtr imageBase = IntPtr.Zero;
                        try
                        {
                            imageBase = Microsoft.Build.Tasks.NativeMethods.MapViewOfFile(zero, 4, 0, 0, IntPtr.Zero);
                            if (imageBase == IntPtr.Zero)
                            {
                                return unknown;
                            }
                            IntPtr ntHeadersPtr = Microsoft.Build.Tasks.NativeMethods.ImageNtHeader(imageBase);
                            if (ntHeadersPtr == IntPtr.Zero)
                            {
                                return unknown;
                            }
                            uint rva = GetCor20HeaderRva(ntHeadersPtr);
                            if (rva == 0)
                            {
                                return unknown;
                            }
                            IntPtr lastRvaSection = IntPtr.Zero;
                            IntPtr ptr = Microsoft.Build.Tasks.NativeMethods.ImageRvaToVa(ntHeadersPtr, imageBase, rva, out lastRvaSection);
                            if (ptr == IntPtr.Zero)
                            {
                                return unknown;
                            }
                            Microsoft.Build.Tasks.NativeMethods.IMAGE_COR20_HEADER image_cor_header = (Microsoft.Build.Tasks.NativeMethods.IMAGE_COR20_HEADER) Marshal.PtrToStructure(ptr, typeof(Microsoft.Build.Tasks.NativeMethods.IMAGE_COR20_HEADER));
                            if ((image_cor_header.StrongNameSignature.VirtualAddress == 0) || (image_cor_header.StrongNameSignature.Size == 0))
                            {
                                return StrongNameLevel.None;
                            }
                            if ((image_cor_header.Flags & 8) != 0)
                            {
                                return StrongNameLevel.FullySigned;
                            }
                            return StrongNameLevel.DelaySigned;
                        }
                        finally
                        {
                            if (imageBase != IntPtr.Zero)
                            {
                                Microsoft.Build.Tasks.NativeMethods.UnmapViewOfFile(imageBase);
                                imageBase = IntPtr.Zero;
                            }
                        }
                    }
                    return unknown;
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Microsoft.Build.Tasks.NativeMethods.CloseHandle(zero);
                        zero = IntPtr.Zero;
                    }
                }
            }
            finally
            {
                if (invalidIntPtr != Microsoft.Build.Tasks.NativeMethods.InvalidIntPtr)
                {
                    Microsoft.Build.Tasks.NativeMethods.CloseHandle(invalidIntPtr);
                    invalidIntPtr = Microsoft.Build.Tasks.NativeMethods.InvalidIntPtr;
                }
            }
            return unknown;
        }

        private static uint GetCor20HeaderRva(IntPtr ntHeadersPtr)
        {
            ushort num = (ushort) Marshal.ReadInt16(ntHeadersPtr, Marshal.SizeOf(typeof(uint)) + Marshal.SizeOf(typeof(Microsoft.Build.Tasks.NativeMethods.IMAGE_FILE_HEADER)));
            ulong num2 = 0L;
            switch (num)
            {
                case 0x10b:
                {
                    Microsoft.Build.Tasks.NativeMethods.IMAGE_NT_HEADERS32 image_nt_headers = (Microsoft.Build.Tasks.NativeMethods.IMAGE_NT_HEADERS32) Marshal.PtrToStructure(ntHeadersPtr, typeof(Microsoft.Build.Tasks.NativeMethods.IMAGE_NT_HEADERS32));
                    num2 = image_nt_headers.optionalHeader.DataDirectory[14];
                    break;
                }
                case 0x20b:
                {
                    Microsoft.Build.Tasks.NativeMethods.IMAGE_NT_HEADERS64 image_nt_headers2 = (Microsoft.Build.Tasks.NativeMethods.IMAGE_NT_HEADERS64) Marshal.PtrToStructure(ntHeadersPtr, typeof(Microsoft.Build.Tasks.NativeMethods.IMAGE_NT_HEADERS64));
                    num2 = image_nt_headers2.optionalHeader.DataDirectory[14];
                    break;
                }
                default:
                    return 0;
            }
            return (uint) (num2 & 0xffffffffL);
        }

        internal static void GetStrongNameKey(TaskLoggingHelper log, string keyFile, string keyContainer, out StrongNameKeyPair keyPair, out byte[] publicKey)
        {
            keyPair = null;
            publicKey = null;
            if ((keyContainer != null) && (keyContainer.Length != 0))
            {
                try
                {
                    keyPair = new StrongNameKeyPair(keyContainer);
                    publicKey = keyPair.PublicKey;
                    return;
                }
                catch (SecurityException exception)
                {
                    log.LogErrorWithCodeFromResources("StrongNameUtils.BadKeyContainer", new object[] { keyContainer });
                    log.LogErrorFromException(exception);
                    throw new StrongNameException(exception);
                }
                catch (ArgumentException exception2)
                {
                    log.LogErrorWithCodeFromResources("StrongNameUtils.BadKeyContainer", new object[] { keyContainer });
                    log.LogErrorFromException(exception2);
                    throw new StrongNameException(exception2);
                }
            }
            if ((keyFile != null) && (keyFile.Length != 0))
            {
                ReadKeyFile(log, keyFile, out keyPair, out publicKey);
            }
        }

        internal static void ReadKeyFile(TaskLoggingHelper log, string keyFile, out StrongNameKeyPair keyPair, out byte[] publicKey)
        {
            byte[] buffer;
            keyPair = null;
            publicKey = null;
            try
            {
                using (FileStream stream = new FileStream(keyFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    buffer = new byte[(int) stream.Length];
                    stream.Read(buffer, 0, (int) stream.Length);
                }
            }
            catch (ArgumentException exception)
            {
                log.LogErrorWithCodeFromResources("StrongNameUtils.KeyFileReadFailure", new object[] { keyFile });
                log.LogErrorFromException(exception);
                throw new StrongNameException(exception);
            }
            catch (IOException exception2)
            {
                log.LogErrorWithCodeFromResources("StrongNameUtils.KeyFileReadFailure", new object[] { keyFile });
                log.LogErrorFromException(exception2);
                throw new StrongNameException(exception2);
            }
            catch (SecurityException exception3)
            {
                log.LogErrorWithCodeFromResources("StrongNameUtils.KeyFileReadFailure", new object[] { keyFile });
                log.LogErrorFromException(exception3);
                throw new StrongNameException(exception3);
            }
            StrongNameKeyPair pair = new StrongNameKeyPair(buffer);
            try
            {
                publicKey = pair.PublicKey;
                keyPair = pair;
            }
            catch (ArgumentException)
            {
                publicKey = buffer;
            }
        }
    }
}

