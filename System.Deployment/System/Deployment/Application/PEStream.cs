namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class PEStream : Stream
    {
        protected bool _canRead;
        protected bool _canSeek;
        protected ArrayList _dataDirectories;
        protected DosHeader _dosHeader;
        protected DosStub _dosStub;
        protected FileHeader _fileHeader;
        protected const ushort _id1ManifestId = 1;
        protected const ushort _id1ManifestLanguageId = 0x409;
        protected long _length;
        protected NtSignature _ntSignature;
        protected OptionalHeader _optionalHeader;
        protected bool _partialConstruct;
        protected FileStream _peFile;
        protected long _position;
        protected ResourceSection _resourceSection;
        protected ArrayList _sectionHeaders;
        protected ArrayList _sections;
        protected StreamComponentList _streamComponents;
        protected const int ErrorBadFormat = 11;
        protected const uint IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7;
        protected const uint IMAGE_DIRECTORY_ENTRY_BASERELOC = 5;
        protected const uint IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11;
        protected const uint IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14;
        protected const uint IMAGE_DIRECTORY_ENTRY_DEBUG = 6;
        protected const uint IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13;
        protected const uint IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3;
        protected const uint IMAGE_DIRECTORY_ENTRY_EXPORT = 0;
        protected const uint IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8;
        protected const uint IMAGE_DIRECTORY_ENTRY_IAT = 12;
        protected const uint IMAGE_DIRECTORY_ENTRY_IMPORT = 1;
        protected const uint IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10;
        protected const uint IMAGE_DIRECTORY_ENTRY_RESOURCE = 2;
        protected const uint IMAGE_DIRECTORY_ENTRY_SECURITY = 4;
        protected const uint IMAGE_DIRECTORY_ENTRY_TLS = 9;
        internal const ushort IMAGE_DOS_SIGNATURE = 0x5a4d;
        internal const uint IMAGE_FILE_DLL = 0x2000;
        internal const uint IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b;
        internal const uint IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b;
        internal const uint IMAGE_NT_SIGNATURE = 0x4550;
        internal const uint IMAGE_NUMBEROF_DIRECTORY_ENTRIES = 0x10;
        protected const uint IMAGE_RESOURCE_DATA_IS_DIRECTORY = 0x80000000;
        protected const uint IMAGE_RESOURCE_NAME_IS_STRING = 0x80000000;
        protected const ushort ManifestDirId = 0x18;

        public PEStream(string filePath)
        {
            this._streamComponents = new StreamComponentList();
            this._dataDirectories = new ArrayList();
            this._sectionHeaders = new ArrayList();
            this._sections = new ArrayList();
            this.ConstructFromFile(filePath, true);
        }

        public PEStream(string filePath, bool partialConstruct)
        {
            this._streamComponents = new StreamComponentList();
            this._dataDirectories = new ArrayList();
            this._sectionHeaders = new ArrayList();
            this._sections = new ArrayList();
            this.ConstructFromFile(filePath, partialConstruct);
        }

        private void ConstructFromFile(string filePath, bool partialConstruct)
        {
            string fileName = Path.GetFileName(filePath);
            bool flag = false;
            try
            {
                this._peFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                this.ConstructPEImage(this._peFile, partialConstruct);
                flag = true;
            }
            catch (IOException exception)
            {
                throw new IOException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidPEImage"), new object[] { fileName }), exception);
            }
            catch (Win32Exception exception2)
            {
                throw new IOException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidPEImage"), new object[] { fileName }), exception2);
            }
            catch (NotSupportedException exception3)
            {
                throw new IOException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidPEImage"), new object[] { fileName }), exception3);
            }
            finally
            {
                if (!flag && (this._peFile != null))
                {
                    this._peFile.Close();
                }
            }
        }

        protected void ConstructPEImage(FileStream file, bool partialConstruct)
        {
            this._partialConstruct = partialConstruct;
            this._dosHeader = new DosHeader(file);
            long size = this._dosHeader.NtHeaderPosition - (this._dosHeader.Address + this._dosHeader.Size);
            if (size < 0L)
            {
                throw new Win32Exception(11, Resources.GetString("Ex_InvalidPEFormat"));
            }
            this._dosStub = new DosStub(file, this._dosHeader.Address + this._dosHeader.Size, size);
            this._ntSignature = new NtSignature(file, (long) this._dosHeader.NtHeaderPosition);
            this._fileHeader = new FileHeader(file, this._ntSignature.Address + this._ntSignature.Size);
            this._optionalHeader = new OptionalHeader(file, this._fileHeader.Address + this._fileHeader.Size);
            long address = this._optionalHeader.Address + this._optionalHeader.Size;
            int num3 = 0;
            for (num3 = 0; num3 < this._optionalHeader.NumberOfRvaAndSizes; num3++)
            {
                DataDirectory directory = new DataDirectory(file, address);
                address += directory.Size;
                this._dataDirectories.Add(directory);
            }
            if (this._fileHeader.SizeOfOptionalHeader < (((ulong) this._optionalHeader.Size) + (this._optionalHeader.NumberOfRvaAndSizes * Marshal.SizeOf(typeof(IMAGE_DATA_DIRECTORY)))))
            {
                throw new Win32Exception(11, Resources.GetString("Ex_InvalidPEFormat"));
            }
            bool flag = false;
            uint virtualAddress = 0;
            if (this._optionalHeader.NumberOfRvaAndSizes > 2)
            {
                virtualAddress = ((DataDirectory) this._dataDirectories[2]).VirtualAddress;
                flag = true;
            }
            long num5 = this._optionalHeader.Address + this._fileHeader.SizeOfOptionalHeader;
            for (num3 = 0; num3 < this._fileHeader.NumberOfSections; num3++)
            {
                SectionHeader sectionHeader = new SectionHeader(file, num5);
                Section section = null;
                if (flag && (sectionHeader.VirtualAddress == virtualAddress))
                {
                    section = this._resourceSection = new ResourceSection(file, sectionHeader, partialConstruct);
                }
                else
                {
                    section = new Section(file, sectionHeader);
                }
                sectionHeader.Section = section;
                this._sectionHeaders.Add(sectionHeader);
                this._sections.Add(section);
                num5 += sectionHeader.Size;
            }
            this.ConstructStream();
            ArrayList c = new ArrayList();
            long num6 = 0L;
            foreach (PEComponent component in this._streamComponents)
            {
                if (component.Address < num6)
                {
                    throw new Win32Exception(11, Resources.GetString("Ex_InvalidPEFormat"));
                }
                if (component.Address > num6)
                {
                    PEComponent component2 = new PEComponent(file, num6, component.Address - num6);
                    c.Add(component2);
                }
                num6 = component.Address + component.Size;
            }
            if (num6 < file.Length)
            {
                PEComponent component3 = new PEComponent(file, num6, file.Length - num6);
                c.Add(component3);
            }
            this._streamComponents.AddRange(c);
            this._streamComponents.Sort(new PEComponentComparer());
            this._canRead = true;
            this._canSeek = true;
            this._length = file.Length;
            this._position = 0L;
        }

        protected void ConstructStream()
        {
            this._streamComponents.Clear();
            this._streamComponents.Add(this._dosHeader);
            this._streamComponents.Add(this._dosStub);
            this._streamComponents.Add(this._ntSignature);
            this._streamComponents.Add(this._fileHeader);
            this._streamComponents.Add(this._optionalHeader);
            foreach (DataDirectory directory in this._dataDirectories)
            {
                this._streamComponents.Add(directory);
            }
            foreach (SectionHeader header in this._sectionHeaders)
            {
                this._streamComponents.Add(header);
            }
            foreach (Section section in this._sections)
            {
                section.AddComponentsToStream(this._streamComponents);
            }
            this._streamComponents.Sort(new PEComponentComparer());
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this._peFile != null))
                {
                    this._peFile.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public byte[] GetDefaultId1ManifestResource()
        {
            ResourceData data = this.GetId1ManifestResource();
            if (data != null)
            {
                return data.Data;
            }
            return null;
        }

        protected ResourceData GetId1ManifestResource()
        {
            object[] keys = new object[] { (ushort) 0x18, Id1ManifestId };
            ResourceComponent component = this.RetrieveResource(keys);
            if ((component != null) && (component is ResourceDirectory))
            {
                ResourceDirectory directory = (ResourceDirectory) component;
                if (directory.ResourceComponentCount > 1)
                {
                    throw new Win32Exception(11, Resources.GetString("Ex_MultipleId1Manifest"));
                }
                if (directory.ResourceComponentCount == 1)
                {
                    ResourceComponent resourceComponent = directory.GetResourceComponent(0);
                    if ((resourceComponent != null) && (resourceComponent is ResourceData))
                    {
                        return (ResourceData) resourceComponent;
                    }
                }
            }
            return null;
        }

        public byte[] GetManifestResource(ushort manifestId, ushort languageId)
        {
            object[] keys = new object[] { (ushort) 0x18, manifestId, languageId };
            ResourceComponent component = this.RetrieveResource(keys);
            if ((component != null) && (component is ResourceData))
            {
                return ((ResourceData) component).Data;
            }
            return null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            bool flag = false;
            long num = 0L;
            int num2 = 0;
            int num3 = count;
            long sourceOffset = 0L;
            int bufferOffset = offset;
            foreach (PEComponent component in this._streamComponents)
            {
                if (!flag)
                {
                    num = (component.Address + component.Size) - 1L;
                    if (this._position <= num)
                    {
                        sourceOffset = this._position - component.Address;
                        if (sourceOffset < 0L)
                        {
                            throw new Win32Exception(11, Resources.GetString("Ex_InvalidPEImage"));
                        }
                        flag = true;
                    }
                }
                if (flag)
                {
                    num2 = component.Read(buffer, bufferOffset, sourceOffset, num3);
                    bufferOffset += num2;
                    this._position += num2;
                    num3 -= num2;
                    sourceOffset = 0L;
                }
                if (num3 <= 0)
                {
                    break;
                }
            }
            return (count - num3);
        }

        protected ResourceComponent RetrieveResource(object[] keys)
        {
            if (this._resourceSection == null)
            {
                return null;
            }
            ResourceDirectory rootResourceDirectory = this._resourceSection.RootResourceDirectory;
            if (rootResourceDirectory == null)
            {
                return null;
            }
            return this.RetrieveResource(rootResourceDirectory, keys, 0);
        }

        protected ResourceComponent RetrieveResource(ResourceDirectory resourcesDirectory, object[] keys, uint keyIndex)
        {
            ResourceComponent component = resourcesDirectory[keys[keyIndex]];
            if (keyIndex == (keys.Length - 1))
            {
                return component;
            }
            if (component is ResourceDirectory)
            {
                return this.RetrieveResource((ResourceDirectory) component, keys, keyIndex + 1);
            }
            return null;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                this._position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                this._position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                this._position = this._length + offset;
            }
            if (this._position < 0L)
            {
                this._position = 0L;
            }
            if (this._position > this._length)
            {
                this._position = this._length;
            }
            return this._position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void ZeroOutDefaultId1ManifestResource()
        {
            ResourceData data = this.GetId1ManifestResource();
            if (data != null)
            {
                data.ZeroData();
            }
        }

        public void ZeroOutManifestResource(ushort manifestId, ushort languageId)
        {
            object[] keys = new object[] { (ushort) 0x18, manifestId, languageId };
            ResourceComponent component = this.RetrieveResource(keys);
            if ((component != null) && (component is ResourceData))
            {
                ((ResourceData) component).ZeroData();
            }
        }

        public void ZeroOutOptionalHeaderCheckSum()
        {
            this._optionalHeader.CheckSum = 0;
        }

        public override bool CanRead
        {
            get
            {
                return this._canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this._canSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public static ushort Id1ManifestId
        {
            get
            {
                return 1;
            }
        }

        public static ushort Id1ManifestLanguageId
        {
            get
            {
                return 0x409;
            }
        }

        public bool IsImageFileDll
        {
            get
            {
                return this._fileHeader.IsImageFileDll;
            }
        }

        public override long Length
        {
            get
            {
                return this._length;
            }
        }

        public override long Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }

        protected class BlankDataBlock : PEStream.DataComponent
        {
            public long _size;

            public BlankDataBlock(long size)
            {
                this._size = size;
            }

            public override int Read(byte[] buffer, int bufferOffset, long sourceOffset, int count)
            {
                int num = 0;
                num = (int) Math.Min((long) count, this._size - sourceOffset);
                if (num < 0)
                {
                    throw new ArgumentException(Resources.GetString("Ex_InvalidCopyRequest"));
                }
                int num2 = 0;
                for (num2 = 0; num2 < num; num2++)
                {
                    buffer[bufferOffset + num2] = 0;
                }
                return num;
            }
        }

        protected abstract class DataComponent
        {
            protected DataComponent()
            {
            }

            public abstract int Read(byte[] buffer, int bufferOffset, long sourceOffset, int count);
        }

        protected class DataDirectory : PEStream.PEComponent
        {
            private PEStream.IMAGE_DATA_DIRECTORY _dataDirectory;

            public DataDirectory(FileStream file, long address)
            {
                this._dataDirectory = (PEStream.IMAGE_DATA_DIRECTORY) PEStream.PEComponent.ReadData(file, address, this._dataDirectory.GetType());
                base._address = address;
                base._size = base.CalculateSize(this._dataDirectory);
                base._data = this._dataDirectory;
            }

            public uint VirtualAddress
            {
                get
                {
                    return this._dataDirectory.VirtualAddress;
                }
            }
        }

        protected class DiskDataBlock : PEStream.DataComponent
        {
            public long _address;
            public FileStream _file;
            public long _size;

            public DiskDataBlock(FileStream file, long address, long size)
            {
                this._address = address;
                this._size = size;
                this._file = file;
            }

            public override int Read(byte[] buffer, int bufferOffset, long sourceOffset, int count)
            {
                int num = 0;
                num = (int) Math.Min((long) count, this._size - sourceOffset);
                if (num < 0)
                {
                    throw new ArgumentException(Resources.GetString("Ex_InvalidCopyRequest"));
                }
                this._file.Seek(this._address + sourceOffset, SeekOrigin.Begin);
                return this._file.Read(buffer, bufferOffset, num);
            }
        }

        protected class DosHeader : PEStream.PEComponent
        {
            protected PEStream.IMAGE_DOS_HEADER _dosHeader;

            public DosHeader(FileStream file)
            {
                file.Seek(0L, SeekOrigin.Begin);
                this._dosHeader = (PEStream.IMAGE_DOS_HEADER) PEStream.PEComponent.ReadData(file, 0L, this._dosHeader.GetType());
                if (this._dosHeader.e_magic != 0x5a4d)
                {
                    throw new Win32Exception(11, Resources.GetString("Ex_InvalidPEImage"));
                }
                base._data = this._dosHeader;
                base._address = 0L;
                base._size = base.CalculateSize(this._dosHeader);
            }

            public uint NtHeaderPosition
            {
                get
                {
                    return this._dosHeader.e_lfanew;
                }
            }
        }

        protected class DosStub : PEStream.PEComponent
        {
            public DosStub(FileStream file, long startAddress, long size)
            {
                base._address = startAddress;
                base._size = size;
                base._data = new PEStream.DiskDataBlock(file, base._address, base._size);
            }
        }

        protected class FileHeader : PEStream.PEComponent
        {
            protected PEStream.IMAGE_FILE_HEADER _fileHeader;

            public FileHeader(FileStream file, long address)
            {
                this._fileHeader = (PEStream.IMAGE_FILE_HEADER) PEStream.PEComponent.ReadData(file, address, this._fileHeader.GetType());
                base._address = address;
                base._size = base.CalculateSize(this._fileHeader);
                base._data = this._fileHeader;
            }

            public bool IsImageFileDll
            {
                get
                {
                    return ((this._fileHeader.Characteristics & 0x2000) != 0);
                }
            }

            public ushort NumberOfSections
            {
                get
                {
                    return this._fileHeader.NumberOfSections;
                }
            }

            public ushort SizeOfOptionalHeader
            {
                get
                {
                    return this._fileHeader.SizeOfOptionalHeader;
                }
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_DATA_DIRECTORY
        {
            public uint VirtualAddress;
            public uint Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_DOS_HEADER
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
            public ushort[] e_res;
            public ushort e_oemid;
            public ushort e_oeminfo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=10)]
            public ushort[] e_res2;
            public uint e_lfanew;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_FILE_HEADER
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
        protected struct IMAGE_OPTIONAL_HEADER32
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
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_OPTIONAL_HEADER64
        {
            internal ushort Magic;
            internal byte MajorLinkerVersion;
            internal byte MinorLinkerVersion;
            internal uint SizeOfCode;
            internal uint SizeOfInitializedData;
            internal uint SizeOfUninitializedData;
            internal uint AddressOfEntryPoint;
            internal uint BaseOfCode;
            internal ulong ImageBase;
            internal uint SectionAlignment;
            internal uint FileAlignment;
            internal ushort MajorOperatingSystemVersion;
            internal ushort MinorOperatingSystemVersion;
            internal ushort MajorImageVersion;
            internal ushort MinorImageVersion;
            internal ushort MajorSubsystemVersion;
            internal ushort MinorSubsystemVersion;
            internal uint Win32VersionValue;
            internal uint SizeOfImage;
            internal uint SizeOfHeaders;
            internal uint CheckSum;
            internal ushort Subsystem;
            internal ushort DllCharacteristics;
            internal ulong SizeOfStackReserve;
            internal ulong SizeOfStackCommit;
            internal ulong SizeOfHeapReserve;
            internal ulong SizeOfHeapCommit;
            internal uint LoaderFlags;
            internal uint NumberOfRvaAndSizes;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_RESOURCE_DATA_ENTRY
        {
            public uint OffsetToData;
            public uint Size;
            public uint CodePage;
            public uint Reserved;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_RESOURCE_DIRECTORY
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public ushort NumberOfNamedEntries;
            public ushort NumberOfIdEntries;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_RESOURCE_DIRECTORY_ENTRY
        {
            public uint Name;
            public uint OffsetToData;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        protected struct IMAGE_SECTION_HEADER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            public byte[] Name;
            public uint VirtualSize;
            public uint VirtualAddress;
            public uint SizeOfRawData;
            public uint PointerToRawData;
            public uint PointerToRelocations;
            public uint PointerToLinenumbers;
            public ushort NumberOfRelocations;
            public ushort NumberOfLinenumbers;
            public uint Characteristics;
        }

        protected class NtSignature : PEStream.PEComponent
        {
            public NtSignature(FileStream file, long address)
            {
                uint data = 0;
                data = (uint) PEStream.PEComponent.ReadData(file, address, data.GetType());
                if (data != 0x4550)
                {
                    throw new Win32Exception(11, Resources.GetString("Ex_InvalidPEFormat"));
                }
                base._address = address;
                base._size = base.CalculateSize(data);
                base._data = data;
            }
        }

        protected class OptionalHeader : PEStream.PEComponent
        {
            protected bool _is64Bit;
            protected PEStream.IMAGE_OPTIONAL_HEADER32 _optionalHeader32;
            protected PEStream.IMAGE_OPTIONAL_HEADER64 _optionalHeader64;

            public OptionalHeader(FileStream file, long address)
            {
                this._optionalHeader32 = (PEStream.IMAGE_OPTIONAL_HEADER32) PEStream.PEComponent.ReadData(file, address, this._optionalHeader32.GetType());
                if (this._optionalHeader32.Magic == 0x20b)
                {
                    this._is64Bit = true;
                    this._optionalHeader64 = (PEStream.IMAGE_OPTIONAL_HEADER64) PEStream.PEComponent.ReadData(file, address, this._optionalHeader64.GetType());
                    base._size = base.CalculateSize(this._optionalHeader64);
                    base._data = this._optionalHeader64;
                }
                else
                {
                    if (this._optionalHeader32.Magic != 0x10b)
                    {
                        throw new NotSupportedException(Resources.GetString("Ex_PEImageTypeNotSupported"));
                    }
                    this._is64Bit = false;
                    base._size = base.CalculateSize(this._optionalHeader32);
                    base._data = this._optionalHeader32;
                }
                base._address = address;
            }

            public uint CheckSum
            {
                set
                {
                    if (this._is64Bit)
                    {
                        this._optionalHeader64.CheckSum = value;
                        base._data = this._optionalHeader64;
                    }
                    else
                    {
                        this._optionalHeader32.CheckSum = value;
                        base._data = this._optionalHeader32;
                    }
                }
            }

            public uint NumberOfRvaAndSizes
            {
                get
                {
                    if (this._is64Bit)
                    {
                        return this._optionalHeader64.NumberOfRvaAndSizes;
                    }
                    return this._optionalHeader32.NumberOfRvaAndSizes;
                }
            }
        }

        protected class PEComponent
        {
            protected long _address;
            protected object _data;
            protected long _size;

            public PEComponent()
            {
                this._address = 0L;
                this._size = 0L;
                this._data = null;
            }

            public PEComponent(FileStream file, long address, long size)
            {
                this._address = address;
                this._size = size;
                this._data = new PEStream.DiskDataBlock(file, address, size);
            }

            protected long CalculateSize(object data)
            {
                return (long) Marshal.SizeOf(data);
            }

            public virtual int Read(byte[] buffer, int bufferOffset, long sourceOffset, int count)
            {
                if (this._data is PEStream.DataComponent)
                {
                    PEStream.DataComponent component = (PEStream.DataComponent) this._data;
                    long num2 = Math.Min((long) count, this._size - sourceOffset);
                    if (num2 < 0L)
                    {
                        throw new ArgumentException(Resources.GetString("Ex_InvalidCopyRequest"));
                    }
                    return component.Read(buffer, bufferOffset, sourceOffset, (int) num2);
                }
                byte[] sourceArray = ToByteArray(this._data);
                long num3 = Math.Min((long) count, sourceArray.Length - sourceOffset);
                if (num3 < 0L)
                {
                    throw new ArgumentException(Resources.GetString("Ex_InvalidCopyRequest"));
                }
                Array.Copy(sourceArray, (int) sourceOffset, buffer, bufferOffset, (int) num3);
                return (int) num3;
            }

            protected static object ReadData(FileStream file, long position, Type dataType)
            {
                int cb = Marshal.SizeOf(dataType);
                byte[] buffer = new byte[cb];
                if (file.Seek(position, SeekOrigin.Begin) != position)
                {
                    throw new IOException(Resources.GetString("Ex_NotEnoughDataInFile"));
                }
                if (file.Read(buffer, 0, buffer.Length) < cb)
                {
                    throw new IOException(Resources.GetString("Ex_NotEnoughDataInFile"));
                }
                IntPtr destination = Marshal.AllocCoTaskMem(cb);
                Marshal.Copy(buffer, 0, destination, cb);
                object obj2 = Marshal.PtrToStructure(destination, dataType);
                Marshal.FreeCoTaskMem(destination);
                return obj2;
            }

            protected static byte[] ToByteArray(object data)
            {
                int cb = Marshal.SizeOf(data);
                IntPtr ptr = Marshal.AllocCoTaskMem(cb);
                Marshal.StructureToPtr(data, ptr, false);
                byte[] destination = new byte[cb];
                Marshal.Copy(ptr, destination, 0, destination.Length);
                Marshal.FreeCoTaskMem(ptr);
                return destination;
            }

            public long Address
            {
                get
                {
                    return this._address;
                }
            }

            public long Size
            {
                get
                {
                    return this._size;
                }
            }
        }

        protected class PEComponentComparer : IComparer
        {
            public int Compare(object a, object b)
            {
                PEStream.PEComponent component = (PEStream.PEComponent) a;
                PEStream.PEComponent component2 = (PEStream.PEComponent) b;
                if (component.Address > component2.Address)
                {
                    return 1;
                }
                if (component.Address < component2.Address)
                {
                    return -1;
                }
                return 0;
            }
        }

        protected class ResourceComponent : PEStream.PEComponent
        {
            public virtual void AddComponentsToStream(PEStream.StreamComponentList stream)
            {
                stream.Add(this);
            }
        }

        protected class ResourceData : PEStream.ResourceComponent
        {
            protected PEStream.IMAGE_RESOURCE_DATA_ENTRY _resourceDataEntry;
            protected PEStream.ResourceRawData _resourceRawData;

            public ResourceData(FileStream file, long rootResourceAddress, long address, long addressDelta)
            {
                this._resourceDataEntry = (PEStream.IMAGE_RESOURCE_DATA_ENTRY) PEStream.PEComponent.ReadData(file, address, this._resourceDataEntry.GetType());
                this._resourceRawData = new PEStream.ResourceRawData(file, this._resourceDataEntry.OffsetToData - addressDelta, (long) this._resourceDataEntry.Size);
                base._address = address;
                base._size = base.CalculateSize(this._resourceDataEntry);
                base._data = this._resourceDataEntry;
            }

            public override void AddComponentsToStream(PEStream.StreamComponentList stream)
            {
                stream.Add(this);
                stream.Add(this._resourceRawData);
            }

            public void ZeroData()
            {
                this._resourceRawData.ZeroData();
            }

            public byte[] Data
            {
                get
                {
                    return this._resourceRawData.Data;
                }
            }
        }

        protected class ResourceDirectory : PEStream.ResourceComponent
        {
            protected PEStream.IMAGE_RESOURCE_DIRECTORY _imageResourceDirectory;
            protected ArrayList _resourceDirectoryEntries = new ArrayList();
            protected Hashtable _resourceDirectoryItems = new Hashtable();

            public ResourceDirectory(PEStream.ResourceSection resourceSection, FileStream file, long rootResourceAddress, long resourceAddress, long addressDelta, bool partialConstruct)
            {
                this._imageResourceDirectory = (PEStream.IMAGE_RESOURCE_DIRECTORY) PEStream.PEComponent.ReadData(file, resourceAddress, this._imageResourceDirectory.GetType());
                base._address = resourceAddress;
                base._size = base.CalculateSize(this._imageResourceDirectory);
                base._data = this._imageResourceDirectory;
                long address = base._address + base._size;
                int num2 = 0;
                for (num2 = 0; num2 < this._imageResourceDirectory.NumberOfIdEntries; num2++)
                {
                    PEStream.ResourceDirectoryEntry entry = new PEStream.ResourceDirectoryEntry(file, address);
                    this._resourceDirectoryEntries.Add(entry);
                    address += entry.Size;
                }
                for (num2 = 0; num2 < this._imageResourceDirectory.NumberOfNamedEntries; num2++)
                {
                    PEStream.ResourceDirectoryEntry entry2 = new PEStream.ResourceDirectoryEntry(file, address);
                    this._resourceDirectoryEntries.Add(entry2);
                    address += entry2.Size;
                }
                foreach (PEStream.ResourceDirectoryEntry entry3 in this._resourceDirectoryEntries)
                {
                    bool flag = false;
                    object key = null;
                    if (entry3.NameIsString)
                    {
                        key = resourceSection.CreateResourceDirectoryString(file, rootResourceAddress + entry3.NameOffset).NameString;
                    }
                    else
                    {
                        key = entry3.Id;
                        if ((rootResourceAddress == resourceAddress) && (entry3.Id == 0x18))
                        {
                            flag = true;
                        }
                    }
                    entry3.Key = key;
                    object obj3 = null;
                    if (entry3.IsDirectory)
                    {
                        if (!partialConstruct || (partialConstruct && flag))
                        {
                            obj3 = new PEStream.ResourceDirectory(resourceSection, file, rootResourceAddress, rootResourceAddress + entry3.OffsetToData, addressDelta, false);
                        }
                    }
                    else
                    {
                        obj3 = new PEStream.ResourceData(file, rootResourceAddress, rootResourceAddress + entry3.OffsetToData, addressDelta);
                    }
                    if (obj3 != null)
                    {
                        this._resourceDirectoryItems.Add(key, obj3);
                    }
                }
            }

            public override void AddComponentsToStream(PEStream.StreamComponentList stream)
            {
                stream.Add(this);
                foreach (PEStream.ResourceDirectoryEntry entry in this._resourceDirectoryEntries)
                {
                    entry.AddComponentsToStream(stream);
                }
                foreach (PEStream.ResourceComponent component in this._resourceDirectoryItems.Values)
                {
                    component.AddComponentsToStream(stream);
                }
            }

            public PEStream.ResourceComponent GetResourceComponent(int index)
            {
                PEStream.ResourceDirectoryEntry entry = (PEStream.ResourceDirectoryEntry) this._resourceDirectoryEntries[index];
                return this[entry.Key];
            }

            public PEStream.ResourceComponent this[object key]
            {
                get
                {
                    if (this._resourceDirectoryItems.Contains(key))
                    {
                        return (PEStream.ResourceComponent) this._resourceDirectoryItems[key];
                    }
                    return null;
                }
            }

            public int ResourceComponentCount
            {
                get
                {
                    return this._resourceDirectoryItems.Count;
                }
            }
        }

        protected class ResourceDirectoryEntry : PEStream.ResourceComponent
        {
            protected PEStream.IMAGE_RESOURCE_DIRECTORY_ENTRY _imageResourceDirectoryEntry;
            protected object _key;

            public ResourceDirectoryEntry(FileStream file, long address)
            {
                this._imageResourceDirectoryEntry = (PEStream.IMAGE_RESOURCE_DIRECTORY_ENTRY) PEStream.PEComponent.ReadData(file, address, this._imageResourceDirectoryEntry.GetType());
                base._address = address;
                base._size = base.CalculateSize(this._imageResourceDirectoryEntry);
                base._data = this._imageResourceDirectoryEntry;
            }

            public ushort Id
            {
                get
                {
                    return (ushort) (this._imageResourceDirectoryEntry.Name & 0xffff);
                }
            }

            public bool IsDirectory
            {
                get
                {
                    return ((this._imageResourceDirectoryEntry.OffsetToData & 0x80000000) != 0);
                }
            }

            public object Key
            {
                get
                {
                    return this._key;
                }
                set
                {
                    this._key = value;
                }
            }

            public bool NameIsString
            {
                get
                {
                    return ((this._imageResourceDirectoryEntry.Name & 0x80000000) != 0);
                }
            }

            public long NameOffset
            {
                get
                {
                    return (long) (this._imageResourceDirectoryEntry.Name & 0x7fffffff);
                }
            }

            public long OffsetToData
            {
                get
                {
                    return (long) (this._imageResourceDirectoryEntry.OffsetToData & 0x7fffffff);
                }
            }
        }

        protected class ResourceDirectoryString : PEStream.ResourceComponent
        {
            protected ushort _length;
            protected string _nameString;
            protected byte[] _nameStringBuffer;

            public ResourceDirectoryString(FileStream file, long offset)
            {
                this._length = (ushort) PEStream.PEComponent.ReadData(file, offset, this._length.GetType());
                if (this._length > 0)
                {
                    long num = this._length * Marshal.SizeOf(typeof(ushort));
                    this._nameStringBuffer = new byte[num];
                    long num2 = offset + base.CalculateSize(this._length);
                    if (file.Seek(num2, SeekOrigin.Begin) != num2)
                    {
                        throw new IOException(Resources.GetString("Ex_NotEnoughDataInFile"));
                    }
                    if (file.Read(this._nameStringBuffer, 0, this._nameStringBuffer.Length) < num)
                    {
                        throw new IOException(Resources.GetString("Ex_NotEnoughDataInFile"));
                    }
                    this._nameString = Encoding.Unicode.GetString(this._nameStringBuffer);
                    base._address = offset;
                    base._size = num + base.CalculateSize(this._length);
                }
                else
                {
                    this._nameStringBuffer = null;
                    this._nameString = null;
                    base._address = offset;
                    base._size = base.CalculateSize(this._length);
                }
                base._data = new PEStream.DiskDataBlock(file, base._address, base._size);
            }

            public string NameString
            {
                get
                {
                    return this._nameString;
                }
            }
        }

        protected class ResourceRawData : PEStream.ResourceComponent
        {
            public ResourceRawData(FileStream file, long address, long size)
            {
                base._address = address;
                base._size = size;
                base._data = new PEStream.DiskDataBlock(file, address, size);
            }

            public void ZeroData()
            {
                base._data = new PEStream.BlankDataBlock(base._size);
            }

            public byte[] Data
            {
                get
                {
                    byte[] buffer = new byte[base._size];
                    if (!(base._data is PEStream.DataComponent))
                    {
                        throw new NotSupportedException();
                    }
                    ((PEStream.DataComponent) base._data).Read(buffer, 0, 0L, buffer.Length);
                    return buffer;
                }
            }
        }

        protected class ResourceSection : PEStream.Section
        {
            protected PEStream.ResourceDirectory _resourceDirectory;
            protected ArrayList _resourceDirectoryStrings;

            public ResourceSection(FileStream file, PEStream.SectionHeader sectionHeader, bool partialConstruct) : base(file, sectionHeader)
            {
                this._resourceDirectoryStrings = new ArrayList();
                this._resourceDirectory = new PEStream.ResourceDirectory(this, file, (long) sectionHeader.PointerToRawData, (long) sectionHeader.PointerToRawData, (long) (sectionHeader.VirtualAddress - sectionHeader.PointerToRawData), partialConstruct);
                base._address = 0L;
                base._size = 0L;
                base._data = null;
            }

            public override void AddComponentsToStream(PEStream.StreamComponentList stream)
            {
                this._resourceDirectory.AddComponentsToStream(stream);
                foreach (PEStream.ResourceDirectoryString str in this._resourceDirectoryStrings)
                {
                    str.AddComponentsToStream(stream);
                }
            }

            public PEStream.ResourceDirectoryString CreateResourceDirectoryString(FileStream file, long offset)
            {
                foreach (PEStream.ResourceDirectoryString str in this._resourceDirectoryStrings)
                {
                    if (str.Address == offset)
                    {
                        return str;
                    }
                }
                PEStream.ResourceDirectoryString str2 = new PEStream.ResourceDirectoryString(file, offset);
                this._resourceDirectoryStrings.Add(str2);
                return str2;
            }

            public PEStream.ResourceDirectory RootResourceDirectory
            {
                get
                {
                    return this._resourceDirectory;
                }
            }
        }

        protected class Section : PEStream.PEComponent
        {
            public PEStream.SectionHeader _sectionHeader;

            public Section(FileStream file, PEStream.SectionHeader sectionHeader)
            {
                base._address = sectionHeader.PointerToRawData;
                base._size = sectionHeader.SizeOfRawData;
                base._data = new PEStream.DiskDataBlock(file, base._address, base._size);
                this._sectionHeader = sectionHeader;
            }

            public virtual void AddComponentsToStream(PEStream.StreamComponentList stream)
            {
                stream.Add(this);
            }
        }

        protected class SectionHeader : PEStream.PEComponent
        {
            protected PEStream.IMAGE_SECTION_HEADER _imageSectionHeader;
            protected System.Deployment.Application.PEStream.Section _section;

            public SectionHeader(FileStream file, long address)
            {
                this._imageSectionHeader = (PEStream.IMAGE_SECTION_HEADER) PEStream.PEComponent.ReadData(file, address, this._imageSectionHeader.GetType());
                base._address = address;
                base._size = base.CalculateSize(this._imageSectionHeader);
                base._data = this._imageSectionHeader;
            }

            public uint PointerToRawData
            {
                get
                {
                    return this._imageSectionHeader.PointerToRawData;
                }
            }

            public System.Deployment.Application.PEStream.Section Section
            {
                set
                {
                    this._section = value;
                }
            }

            public uint SizeOfRawData
            {
                get
                {
                    return this._imageSectionHeader.SizeOfRawData;
                }
            }

            public uint VirtualAddress
            {
                get
                {
                    return this._imageSectionHeader.VirtualAddress;
                }
            }
        }

        protected class StreamComponentList : ArrayList
        {
            public int Add(PEStream.PEComponent peComponent)
            {
                if (peComponent.Size > 0L)
                {
                    return this.Add(peComponent);
                }
                return -1;
            }
        }
    }
}

