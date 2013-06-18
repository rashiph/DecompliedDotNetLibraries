namespace Microsoft.Workflow.Compiler
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class PEHeader
    {
        private const ushort IMAGE_DOS_SIGNATURE = 0x5a4d;
        private const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
        private const ushort IMAGE_FILE_MACHINE_IA64 = 0x200;
        private const uint IMAGE_NT_SIGNATURE = 0x4550;

        public static bool Is64BitRequiredExecutable(string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IMAGE_DOS_HEADER image_dos_header = ReadHelper.ReadFromStream<IMAGE_DOS_HEADER>(stream);
                if (image_dos_header.e_magic != 0x5a4d)
                {
                    throw new InvalidDataException(WrapperSR.GetString("InvalidAssemblyHeader", new object[] { path }));
                }
                stream.Position = image_dos_header.e_lfanew;
                IMAGE_NT_HEADERS image_nt_headers = ReadHelper.ReadFromStream<IMAGE_NT_HEADERS>(stream);
                if (image_nt_headers.Signature != 0x4550)
                {
                    throw new InvalidDataException(WrapperSR.GetString("InvalidAssemblyHeader", new object[] { path }));
                }
                switch (image_nt_headers.FileHeader.Machine)
                {
                    case 0x200:
                    case 0x8664:
                        return true;
                }
                return false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGE_DATA_DIRECTORY
        {
            public uint VirtualAddress;
            public uint Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class IMAGE_DOS_HEADER
        {
            public ushort e_magic;
            public ushort e_cblp;
            public ushort e_cp;
            public ushort e_crlc;
            public ushort e_cparhdr;
            public ushort e_minalloc;
            public ushort e_maxalloc;
            public ushort e_ss;
            public ushort e_sp;
            public ushort e_csum;
            public ushort e_ip;
            public ushort e_cs;
            public ushort e_lfarlc;
            public ushort e_ovno;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public ushort[] e_res1;
            public ushort e_oemid;
            public ushort e_oeminfo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=10)]
            public ushort[] e_res2;
            public int e_lfanew;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class IMAGE_NT_HEADERS
        {
            public uint Signature;
            public PEHeader.IMAGE_FILE_HEADER FileHeader;
            public PEHeader.IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGE_OPTIONAL_HEADER32
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;
            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
            public PEHeader.IMAGE_DATA_DIRECTORY[] DataDirectory;
        }

        private static class ReadHelper
        {
            private static byte[] ReadBufferFromStream(Stream source, int bufferSize)
            {
                byte[] buffer = new byte[bufferSize];
                source.Read(buffer, 0, bufferSize);
                return buffer;
            }

            public static unsafe T ReadFromStream<T>(Stream source) where T: class, new()
            {
                fixed (byte* numRef = ReadBufferFromStream(source, Marshal.SizeOf(typeof(T))))
                {
                    return (T) Marshal.PtrToStructure((IntPtr) numRef, typeof(T));
                }
            }
        }
    }
}

