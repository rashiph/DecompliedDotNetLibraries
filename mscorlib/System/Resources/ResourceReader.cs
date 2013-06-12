namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Text;

    [ComVisible(true)]
    public sealed class ResourceReader : IResourceReader, IEnumerable, IDisposable
    {
        private long _dataSectionOffset;
        private int[] _nameHashes;
        private unsafe int* _nameHashesPtr;
        private int[] _namePositions;
        private unsafe int* _namePositionsPtr;
        private long _nameSectionOffset;
        private int _numResources;
        private BinaryFormatter _objFormatter;
        internal Dictionary<string, ResourceLocator> _resCache;
        private bool[] _safeToDeserialize;
        private BinaryReader _store;
        private TypeLimitingDeserializationBinder _typeLimitingBinder;
        private int[] _typeNamePositions;
        private RuntimeType[] _typeTable;
        private UnmanagedMemoryStream _ums;
        private int _version;
        private const int DefaultFileStreamBufferSize = 0x1000;
        private static readonly string[] TypesSafeForDeserialization = new string[] { 
            "System.String[], mscorlib, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.DateTime[], mscorlib, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Bitmap, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Imaging.Metafile, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Point, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.PointF, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Size, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.SizeF, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Font, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Icon, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Color, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Windows.Forms.Cursor, System.Windows.Forms, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Windows.Forms.Padding, System.Windows.Forms, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Windows.Forms.LinkArea, System.Windows.Forms, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Windows.Forms.ImageListStreamer, System.Windows.Forms, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Windows.Forms.ListViewGroup, System.Windows.Forms, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
            "System.Windows.Forms.ListViewItem, System.Windows.Forms, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Windows.Forms.TreeNode, System.Windows.Forms, Culture=neutral, PublicKeyToken=b77a5c561934e089"
         };

        [SecurityCritical]
        public ResourceReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotReadable"));
            }
            this._resCache = new Dictionary<string, ResourceLocator>(FastResourceComparer.Default);
            this._store = new BinaryReader(stream, Encoding.UTF8);
            this._ums = stream as UnmanagedMemoryStream;
            this.ReadResources();
        }

        [SecuritySafeCritical]
        public ResourceReader(string fileName)
        {
            this._resCache = new Dictionary<string, ResourceLocator>(FastResourceComparer.Default);
            this._store = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, FileOptions.RandomAccess), Encoding.UTF8);
            try
            {
                this.ReadResources();
            }
            catch
            {
                this._store.Close();
                throw;
            }
        }

        [SecurityCritical]
        internal ResourceReader(Stream stream, Dictionary<string, ResourceLocator> resCache)
        {
            this._resCache = resCache;
            this._store = new BinaryReader(stream, Encoding.UTF8);
            this._ums = stream as UnmanagedMemoryStream;
            this.ReadResources();
        }

        [SecuritySafeCritical]
        private object _LoadObjectV1(int pos)
        {
            this._store.BaseStream.Seek(this._dataSectionOffset + pos, SeekOrigin.Begin);
            int typeIndex = this._store.Read7BitEncodedInt();
            if (typeIndex == -1)
            {
                return null;
            }
            RuntimeType type = this.FindType(typeIndex);
            if (type == typeof(string))
            {
                return this._store.ReadString();
            }
            if (type == typeof(int))
            {
                return this._store.ReadInt32();
            }
            if (type == typeof(byte))
            {
                return this._store.ReadByte();
            }
            if (type == typeof(sbyte))
            {
                return this._store.ReadSByte();
            }
            if (type == typeof(short))
            {
                return this._store.ReadInt16();
            }
            if (type == typeof(long))
            {
                return this._store.ReadInt64();
            }
            if (type == typeof(ushort))
            {
                return this._store.ReadUInt16();
            }
            if (type == typeof(uint))
            {
                return this._store.ReadUInt32();
            }
            if (type == typeof(ulong))
            {
                return this._store.ReadUInt64();
            }
            if (type == typeof(float))
            {
                return this._store.ReadSingle();
            }
            if (type == typeof(double))
            {
                return this._store.ReadDouble();
            }
            if (type == typeof(DateTime))
            {
                return new DateTime(this._store.ReadInt64());
            }
            if (type == typeof(TimeSpan))
            {
                return new TimeSpan(this._store.ReadInt64());
            }
            if (type != typeof(decimal))
            {
                return this.DeserializeObject(typeIndex);
            }
            int[] bits = new int[4];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = this._store.ReadInt32();
            }
            return new decimal(bits);
        }

        [SecuritySafeCritical]
        private unsafe object _LoadObjectV2(int pos, out ResourceTypeCode typeCode)
        {
            this._store.BaseStream.Seek(this._dataSectionOffset + pos, SeekOrigin.Begin);
            typeCode = (ResourceTypeCode) this._store.Read7BitEncodedInt();
            switch (typeCode)
            {
                case ResourceTypeCode.Null:
                    return null;

                case ResourceTypeCode.String:
                    return this._store.ReadString();

                case ResourceTypeCode.Boolean:
                    return this._store.ReadBoolean();

                case ResourceTypeCode.Char:
                    return (char) this._store.ReadUInt16();

                case ResourceTypeCode.Byte:
                    return this._store.ReadByte();

                case ResourceTypeCode.SByte:
                    return this._store.ReadSByte();

                case ResourceTypeCode.Int16:
                    return this._store.ReadInt16();

                case ResourceTypeCode.UInt16:
                    return this._store.ReadUInt16();

                case ResourceTypeCode.Int32:
                    return this._store.ReadInt32();

                case ResourceTypeCode.UInt32:
                    return this._store.ReadUInt32();

                case ResourceTypeCode.Int64:
                    return this._store.ReadInt64();

                case ResourceTypeCode.UInt64:
                    return this._store.ReadUInt64();

                case ResourceTypeCode.Single:
                    return this._store.ReadSingle();

                case ResourceTypeCode.Double:
                    return this._store.ReadDouble();

                case ResourceTypeCode.Decimal:
                    return this._store.ReadDecimal();

                case ResourceTypeCode.DateTime:
                    return DateTime.FromBinary(this._store.ReadInt64());

                case ResourceTypeCode.TimeSpan:
                    return new TimeSpan(this._store.ReadInt64());

                case ResourceTypeCode.ByteArray:
                {
                    int count = this._store.ReadInt32();
                    if (count < 0)
                    {
                        throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourceDataLengthInvalid", new object[] { count }));
                    }
                    if (this._ums == null)
                    {
                        if (count > this._store.BaseStream.Length)
                        {
                            throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourceDataLengthInvalid", new object[] { count }));
                        }
                        return this._store.ReadBytes(count);
                    }
                    if (count > (this._ums.Length - this._ums.Position))
                    {
                        throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourceDataLengthInvalid", new object[] { count }));
                    }
                    byte[] buffer = new byte[count];
                    this._ums.Read(buffer, 0, count);
                    return buffer;
                }
                case ResourceTypeCode.Stream:
                {
                    int num4 = this._store.ReadInt32();
                    if (num4 < 0)
                    {
                        throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourceDataLengthInvalid", new object[] { num4 }));
                    }
                    if (this._ums == null)
                    {
                        return new PinnedBufferMemoryStream(this._store.ReadBytes(num4));
                    }
                    if (num4 > (this._ums.Length - this._ums.Position))
                    {
                        throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourceDataLengthInvalid", new object[] { num4 }));
                    }
                    return new UnmanagedMemoryStream(this._ums.PositionPointer, (long) num4, (long) num4, FileAccess.Read, true);
                }
            }
            if (typeCode < ResourceTypeCode.StartOfUserTypes)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_TypeMismatch"));
            }
            int typeIndex = ((int) typeCode) - 0x40;
            return this.DeserializeObject(typeIndex);
        }

        [SecurityCritical]
        private unsafe void _ReadResources()
        {
            if (this._store.ReadInt32() != ResourceManager.MagicNumber)
            {
                throw new ArgumentException(Environment.GetResourceString("Resources_StreamNotValid"));
            }
            int num2 = this._store.ReadInt32();
            int num3 = this._store.ReadInt32();
            if ((num3 < 0) || (num2 < 0))
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
            }
            if (num2 > 1)
            {
                this._store.BaseStream.Seek((long) num3, SeekOrigin.Current);
            }
            else
            {
                string str = this._store.ReadString();
                AssemblyName name = new AssemblyName(ResourceManager.MscorlibName);
                if (!ResourceManager.CompareNames(str, ResourceManager.ResReaderTypeName, name))
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_WrongResourceReader_Type", new object[] { str }));
                }
                this.SkipString();
            }
            int num4 = this._store.ReadInt32();
            if ((num4 != 2) && (num4 != 1))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ResourceFileUnsupportedVersion", new object[] { 2, num4 }));
            }
            this._version = num4;
            this._numResources = this._store.ReadInt32();
            if (this._numResources < 0)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
            }
            int num5 = this._store.ReadInt32();
            if (num5 < 0)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
            }
            this._typeTable = new RuntimeType[num5];
            this._typeNamePositions = new int[num5];
            for (int i = 0; i < num5; i++)
            {
                this._typeNamePositions[i] = (int) this._store.BaseStream.Position;
                this.SkipString();
            }
            int num8 = ((int) this._store.BaseStream.Position) & 7;
            if (num8 != 0)
            {
                for (int j = 0; j < (8 - num8); j++)
                {
                    this._store.ReadByte();
                }
            }
            if (this._ums == null)
            {
                this._nameHashes = new int[this._numResources];
                for (int k = 0; k < this._numResources; k++)
                {
                    this._nameHashes[k] = this._store.ReadInt32();
                }
            }
            else
            {
                int num11 = 4 * this._numResources;
                if (num11 < 0)
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
                }
                this._nameHashesPtr = (int*) this._ums.PositionPointer;
                this._ums.Seek((long) num11, SeekOrigin.Current);
                byte* positionPointer = this._ums.PositionPointer;
            }
            if (this._ums == null)
            {
                this._namePositions = new int[this._numResources];
                for (int m = 0; m < this._numResources; m++)
                {
                    int num13 = this._store.ReadInt32();
                    if (num13 < 0)
                    {
                        throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
                    }
                    this._namePositions[m] = num13;
                }
            }
            else
            {
                int num14 = 4 * this._numResources;
                if (num14 < 0)
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
                }
                this._namePositionsPtr = (int*) this._ums.PositionPointer;
                this._ums.Seek((long) num14, SeekOrigin.Current);
                byte* numPtr2 = this._ums.PositionPointer;
            }
            this._dataSectionOffset = this._store.ReadInt32();
            if (this._dataSectionOffset < 0L)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
            }
            this._nameSectionOffset = this._store.BaseStream.Position;
            if (this._dataSectionOffset < this._nameSectionOffset)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"));
            }
        }

        [SecurityCritical]
        private unsafe string AllocateStringForNameIndex(int index, out int dataOffset)
        {
            byte[] buffer;
            int num;
            long namePosition = this.GetNamePosition(index);
            lock (this)
            {
                int num4;
                this._store.BaseStream.Seek(namePosition + this._nameSectionOffset, SeekOrigin.Begin);
                num = this._store.Read7BitEncodedInt();
                if (num < 0)
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_NegativeStringLength"));
                }
                if (this._ums != null)
                {
                    if (this._ums.Position > (this._ums.Length - num))
                    {
                        throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesIndexTooLong", new object[] { index }));
                    }
                    string str = null;
                    str = new string((char*) this._ums.PositionPointer, 0, num / 2);
                    this._ums.Position += num;
                    dataOffset = this._store.ReadInt32();
                    if ((dataOffset < 0) || (((long) dataOffset) >= (this._store.BaseStream.Length - this._dataSectionOffset)))
                    {
                        throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourcesDataInvalidOffset", new object[] { (int) dataOffset }));
                    }
                    return str;
                }
                buffer = new byte[num];
                for (int i = num; i > 0; i -= num4)
                {
                    num4 = this._store.Read(buffer, num - i, i);
                    if (num4 == 0)
                    {
                        throw new EndOfStreamException(Environment.GetResourceString("BadImageFormat_ResourceNameCorrupted_NameIndex", new object[] { index }));
                    }
                }
                dataOffset = this._store.ReadInt32();
                if ((dataOffset < 0) || (((long) dataOffset) >= (this._store.BaseStream.Length - this._dataSectionOffset)))
                {
                    throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourcesDataInvalidOffset", new object[] { (int) dataOffset }));
                }
            }
            return Encoding.Unicode.GetString(buffer, 0, num);
        }

        public void Close()
        {
            this.Dispose(true);
        }

        [SecuritySafeCritical]
        private unsafe bool CompareStringEqualsName(string name)
        {
            int num3;
            int byteLen = this._store.Read7BitEncodedInt();
            if (byteLen < 0)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_NegativeStringLength"));
            }
            if (this._ums != null)
            {
                byte* positionPointer = this._ums.PositionPointer;
                this._ums.Seek((long) byteLen, SeekOrigin.Current);
                if (this._ums.Position > this._ums.Length)
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesNameTooLong"));
                }
                return (FastResourceComparer.CompareOrdinal(positionPointer, byteLen, name) == 0);
            }
            byte[] buffer = new byte[byteLen];
            for (int i = byteLen; i > 0; i -= num3)
            {
                num3 = this._store.Read(buffer, byteLen - i, i);
                if (num3 == 0)
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourceNameCorrupted"));
                }
            }
            return (FastResourceComparer.CompareOrdinal(buffer, byteLen / 2, name) == 0);
        }

        [SecurityCritical]
        private object DeserializeObject(int typeIndex)
        {
            object obj2;
            RuntimeType type = this.FindType(typeIndex);
            if (this._safeToDeserialize == null)
            {
                this.InitSafeToDeserializeArray();
            }
            if (this._safeToDeserialize[typeIndex])
            {
                this._objFormatter.Binder = this._typeLimitingBinder;
                this._typeLimitingBinder.ExpectingToDeserialize(type);
                obj2 = this._objFormatter.UnsafeDeserialize(this._store.BaseStream, null);
            }
            else
            {
                this._objFormatter.Binder = null;
                obj2 = this._objFormatter.Deserialize(this._store.BaseStream);
            }
            if (obj2.GetType() != type)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResType&SerBlobMismatch", new object[] { type.FullName, obj2.GetType().FullName }));
            }
            return obj2;
        }

        public void Dispose()
        {
            this.Close();
        }

        [SecuritySafeCritical]
        private unsafe void Dispose(bool disposing)
        {
            if (this._store != null)
            {
                this._resCache = null;
                if (disposing)
                {
                    BinaryReader reader = this._store;
                    this._store = null;
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                this._store = null;
                this._namePositions = null;
                this._nameHashes = null;
                this._ums = null;
                this._namePositionsPtr = null;
                this._nameHashesPtr = null;
            }
        }

        internal int FindPosForResource(string name)
        {
            int num = FastResourceComparer.HashFunction(name);
            int num2 = 0;
            int num3 = this._numResources - 1;
            int index = -1;
            bool flag = false;
            while (num2 <= num3)
            {
                int num6;
                index = (num2 + num3) >> 1;
                int nameHash = this.GetNameHash(index);
                if (nameHash == num)
                {
                    num6 = 0;
                }
                else if (nameHash < num)
                {
                    num6 = -1;
                }
                else
                {
                    num6 = 1;
                }
                if (num6 == 0)
                {
                    flag = true;
                    break;
                }
                if (num6 < 0)
                {
                    num2 = index + 1;
                }
                else
                {
                    num3 = index - 1;
                }
            }
            if (flag)
            {
                if (num2 != index)
                {
                    num2 = index;
                    while ((num2 > 0) && (this.GetNameHash(num2 - 1) == num))
                    {
                        num2--;
                    }
                }
                if (num3 != index)
                {
                    num3 = index;
                    while ((num3 < (this._numResources - 1)) && (this.GetNameHash(num3 + 1) == num))
                    {
                        num3++;
                    }
                }
                lock (this)
                {
                    for (int i = num2; i <= num3; i++)
                    {
                        this._store.BaseStream.Seek(this._nameSectionOffset + this.GetNamePosition(i), SeekOrigin.Begin);
                        if (this.CompareStringEqualsName(name))
                        {
                            int num8 = this._store.ReadInt32();
                            if ((num8 < 0) || (num8 >= (this._store.BaseStream.Length - this._dataSectionOffset)))
                            {
                                throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourcesDataInvalidOffset", new object[] { num8 }));
                            }
                            return num8;
                        }
                    }
                }
            }
            return -1;
        }

        private RuntimeType FindType(int typeIndex)
        {
            if ((typeIndex < 0) || (typeIndex >= this._typeTable.Length))
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_InvalidType"));
            }
            if (this._typeTable[typeIndex] == null)
            {
                long position = this._store.BaseStream.Position;
                try
                {
                    this._store.BaseStream.Position = this._typeNamePositions[typeIndex];
                    string typeName = this._store.ReadString();
                    this._typeTable[typeIndex] = (RuntimeType) Type.GetType(typeName, true);
                }
                finally
                {
                    this._store.BaseStream.Position = position;
                }
            }
            return this._typeTable[typeIndex];
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            if (this._resCache == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("ResourceReaderIsClosed"));
            }
            return new ResourceEnumerator(this);
        }

        internal ResourceEnumerator GetEnumeratorInternal()
        {
            return new ResourceEnumerator(this);
        }

        [SecuritySafeCritical]
        private unsafe int GetNameHash(int index)
        {
            if (this._ums == null)
            {
                return this._nameHashes[index];
            }
            return ReadUnalignedI4(this._nameHashesPtr + index);
        }

        [SecuritySafeCritical]
        private unsafe int GetNamePosition(int index)
        {
            int num;
            if (this._ums == null)
            {
                num = this._namePositions[index];
            }
            else
            {
                num = ReadUnalignedI4(this._namePositionsPtr + index);
            }
            if ((num < 0) || (num > (this._dataSectionOffset - this._nameSectionOffset)))
            {
                throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourcesNameInvalidOffset", new object[] { num }));
            }
            return num;
        }

        public void GetResourceData(string resourceName, out string resourceType, out byte[] resourceData)
        {
            if (resourceName == null)
            {
                throw new ArgumentNullException("resourceName");
            }
            if (this._resCache == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("ResourceReaderIsClosed"));
            }
            int[] array = new int[this._numResources];
            int num = this.FindPosForResource(resourceName);
            if (num == -1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ResourceNameNotExist", new object[] { resourceName }));
            }
            lock (this)
            {
                for (int i = 0; i < this._numResources; i++)
                {
                    this._store.BaseStream.Position = this._nameSectionOffset + this.GetNamePosition(i);
                    int num3 = this._store.Read7BitEncodedInt();
                    if (num3 < 0)
                    {
                        throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourcesNameInvalidOffset", new object[] { num3 }));
                    }
                    Stream baseStream = this._store.BaseStream;
                    baseStream.Position += num3;
                    int num4 = this._store.ReadInt32();
                    if ((num4 < 0) || (num4 >= (this._store.BaseStream.Length - this._dataSectionOffset)))
                    {
                        throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourcesDataInvalidOffset", new object[] { num4 }));
                    }
                    array[i] = num4;
                }
                Array.Sort<int>(array);
                int num5 = Array.BinarySearch<int>(array, num);
                long num6 = (num5 < (this._numResources - 1)) ? (array[num5 + 1] + this._dataSectionOffset) : this._store.BaseStream.Length;
                int count = (int) (num6 - (num + this._dataSectionOffset));
                this._store.BaseStream.Position = this._dataSectionOffset + num;
                ResourceTypeCode typeCode = (ResourceTypeCode) this._store.Read7BitEncodedInt();
                if ((typeCode < ResourceTypeCode.Null) || (typeCode >= (0x40 + this._typeTable.Length)))
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_InvalidType"));
                }
                resourceType = this.TypeNameFromTypeCode(typeCode);
                count -= (int) (this._store.BaseStream.Position - (this._dataSectionOffset + num));
                byte[] buffer = this._store.ReadBytes(count);
                if (buffer.Length != count)
                {
                    throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourceNameCorrupted"));
                }
                resourceData = buffer;
            }
        }

        [SecuritySafeCritical]
        private object GetValueForNameIndex(int index)
        {
            long namePosition = this.GetNamePosition(index);
            lock (this)
            {
                ResourceTypeCode code;
                this._store.BaseStream.Seek(namePosition + this._nameSectionOffset, SeekOrigin.Begin);
                this.SkipString();
                int pos = this._store.ReadInt32();
                if ((pos < 0) || (pos >= (this._store.BaseStream.Length - this._dataSectionOffset)))
                {
                    throw new FormatException(Environment.GetResourceString("BadImageFormat_ResourcesDataInvalidOffset", new object[] { pos }));
                }
                if (this._version == 1)
                {
                    return this.LoadObjectV1(pos);
                }
                return this.LoadObjectV2(pos, out code);
            }
        }

        [SecurityCritical]
        private void InitSafeToDeserializeArray()
        {
            this._safeToDeserialize = new bool[this._typeTable.Length];
            for (int i = 0; i < this._typeTable.Length; i++)
            {
                string str;
                AssemblyName name;
                string fullName;
                long position = this._store.BaseStream.Position;
                try
                {
                    this._store.BaseStream.Position = this._typeNamePositions[i];
                    str = this._store.ReadString();
                }
                finally
                {
                    this._store.BaseStream.Position = position;
                }
                RuntimeType type = (RuntimeType) Type.GetType(str, false);
                if (type == null)
                {
                    name = null;
                    fullName = str;
                }
                else
                {
                    if (type.BaseType == typeof(Enum))
                    {
                        this._safeToDeserialize[i] = true;
                        continue;
                    }
                    fullName = type.FullName;
                    name = new AssemblyName();
                    RuntimeAssembly assembly = (RuntimeAssembly) type.Assembly;
                    name.Init(assembly.GetSimpleName(), assembly.GetPublicKey(), null, null, assembly.GetLocale(), AssemblyHashAlgorithm.None, AssemblyVersionCompatibility.SameMachine, null, AssemblyNameFlags.PublicKey, null);
                }
                foreach (string str3 in TypesSafeForDeserialization)
                {
                    if (ResourceManager.CompareNames(str3, fullName, name))
                    {
                        this._safeToDeserialize[i] = true;
                    }
                }
            }
        }

        internal object LoadObject(int pos)
        {
            ResourceTypeCode code;
            if (this._version == 1)
            {
                return this.LoadObjectV1(pos);
            }
            return this.LoadObjectV2(pos, out code);
        }

        internal object LoadObject(int pos, out ResourceTypeCode typeCode)
        {
            if (this._version == 1)
            {
                object obj2 = this.LoadObjectV1(pos);
                typeCode = (obj2 is string) ? ResourceTypeCode.String : ResourceTypeCode.StartOfUserTypes;
                return obj2;
            }
            return this.LoadObjectV2(pos, out typeCode);
        }

        internal object LoadObjectV1(int pos)
        {
            object obj2;
            try
            {
                obj2 = this._LoadObjectV1(pos);
            }
            catch (EndOfStreamException exception)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_TypeMismatch"), exception);
            }
            catch (ArgumentOutOfRangeException exception2)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_TypeMismatch"), exception2);
            }
            return obj2;
        }

        internal object LoadObjectV2(int pos, out ResourceTypeCode typeCode)
        {
            object obj2;
            try
            {
                obj2 = this._LoadObjectV2(pos, out typeCode);
            }
            catch (EndOfStreamException exception)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_TypeMismatch"), exception);
            }
            catch (ArgumentOutOfRangeException exception2)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_TypeMismatch"), exception2);
            }
            return obj2;
        }

        [SecuritySafeCritical]
        internal string LoadString(int pos)
        {
            string fullName;
            this._store.BaseStream.Seek(this._dataSectionOffset + pos, SeekOrigin.Begin);
            string str = null;
            int typeIndex = this._store.Read7BitEncodedInt();
            if (this._version == 1)
            {
                if (typeIndex == -1)
                {
                    return null;
                }
                if (this.FindType(typeIndex) != typeof(string))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Type", new object[] { this.FindType(typeIndex).FullName }));
                }
                return this._store.ReadString();
            }
            ResourceTypeCode code = (ResourceTypeCode) typeIndex;
            switch (code)
            {
                case ResourceTypeCode.String:
                case ResourceTypeCode.Null:
                    if (code == ResourceTypeCode.String)
                    {
                        str = this._store.ReadString();
                    }
                    return str;
            }
            if (code < ResourceTypeCode.StartOfUserTypes)
            {
                fullName = code.ToString();
            }
            else
            {
                fullName = this.FindType(((int) code) - 0x40).FullName;
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Type", new object[] { fullName }));
        }

        [SecurityCritical]
        private void ReadResources()
        {
            BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence | StreamingContextStates.File));
            this._typeLimitingBinder = new TypeLimitingDeserializationBinder();
            formatter.Binder = this._typeLimitingBinder;
            this._objFormatter = formatter;
            try
            {
                this._ReadResources();
            }
            catch (EndOfStreamException exception)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"), exception);
            }
            catch (IndexOutOfRangeException exception2)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResourcesHeaderCorrupted"), exception2);
            }
        }

        [SecurityCritical]
        internal static unsafe int ReadUnalignedI4(int* p)
        {
            byte* numPtr = (byte*) p;
            return (((numPtr[0] | (numPtr[1] << 8)) | (numPtr[2] << 0x10)) | (numPtr[3] << 0x18));
        }

        private void SkipInt32()
        {
            this._store.BaseStream.Seek(4L, SeekOrigin.Current);
        }

        private void SkipString()
        {
            int num = this._store.Read7BitEncodedInt();
            if (num < 0)
            {
                throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_NegativeStringLength"));
            }
            this._store.BaseStream.Seek((long) num, SeekOrigin.Current);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private string TypeNameFromTypeCode(ResourceTypeCode typeCode)
        {
            string str;
            if (typeCode < ResourceTypeCode.StartOfUserTypes)
            {
                return ("ResourceTypeCode." + typeCode.ToString());
            }
            int index = ((int) typeCode) - 0x40;
            long position = this._store.BaseStream.Position;
            try
            {
                this._store.BaseStream.Position = this._typeNamePositions[index];
                str = this._store.ReadString();
            }
            finally
            {
                this._store.BaseStream.Position = position;
            }
            return str;
        }

        internal sealed class ResourceEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private bool _currentIsValid;
            private int _currentName = -1;
            private int _dataPosition;
            private ResourceReader _reader;
            private const int ENUM_DONE = -2147483648;
            private const int ENUM_NOT_STARTED = -1;

            internal ResourceEnumerator(ResourceReader reader)
            {
                this._reader = reader;
                this._dataPosition = -2;
            }

            public bool MoveNext()
            {
                if ((this._currentName == (this._reader._numResources - 1)) || (this._currentName == -2147483648))
                {
                    this._currentIsValid = false;
                    this._currentName = -2147483648;
                    return false;
                }
                this._currentIsValid = true;
                this._currentName++;
                return true;
            }

            public void Reset()
            {
                if (this._reader._resCache == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("ResourceReaderIsClosed"));
                }
                this._currentIsValid = false;
                this._currentName = -1;
            }

            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }

            internal int DataPosition
            {
                get
                {
                    return this._dataPosition;
                }
            }

            public DictionaryEntry Entry
            {
                [SecuritySafeCritical]
                get
                {
                    string str;
                    if (this._currentName == -2147483648)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                    }
                    if (!this._currentIsValid)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    if (this._reader._resCache == null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("ResourceReaderIsClosed"));
                    }
                    object valueForNameIndex = null;
                    lock (this._reader._resCache)
                    {
                        ResourceLocator locator;
                        str = this._reader.AllocateStringForNameIndex(this._currentName, out this._dataPosition);
                        if (this._reader._resCache.TryGetValue(str, out locator))
                        {
                            valueForNameIndex = locator.Value;
                        }
                        if (valueForNameIndex == null)
                        {
                            if (this._dataPosition == -1)
                            {
                                valueForNameIndex = this._reader.GetValueForNameIndex(this._currentName);
                            }
                            else
                            {
                                valueForNameIndex = this._reader.LoadObject(this._dataPosition);
                            }
                        }
                    }
                    return new DictionaryEntry(str, valueForNameIndex);
                }
            }

            public object Key
            {
                [SecuritySafeCritical]
                get
                {
                    if (this._currentName == -2147483648)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                    }
                    if (!this._currentIsValid)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    if (this._reader._resCache == null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("ResourceReaderIsClosed"));
                    }
                    return this._reader.AllocateStringForNameIndex(this._currentName, out this._dataPosition);
                }
            }

            public object Value
            {
                get
                {
                    if (this._currentName == -2147483648)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                    }
                    if (!this._currentIsValid)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    if (this._reader._resCache == null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("ResourceReaderIsClosed"));
                    }
                    return this._reader.GetValueForNameIndex(this._currentName);
                }
            }
        }

        internal sealed class TypeLimitingDeserializationBinder : SerializationBinder
        {
            private System.Runtime.Serialization.Formatters.Binary.ObjectReader _objectReader;
            private RuntimeType _typeToDeserialize;

            [SecuritySafeCritical]
            public override Type BindToType(string assemblyName, string typeName)
            {
                AssemblyName name = new AssemblyName(assemblyName);
                bool flag = false;
                foreach (string str in ResourceReader.TypesSafeForDeserialization)
                {
                    if (ResourceManager.CompareNames(str, typeName, name))
                    {
                        flag = true;
                        break;
                    }
                }
                if (this.ObjectReader.FastBindToType(assemblyName, typeName).IsEnum)
                {
                    flag = true;
                }
                if (!flag)
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ResType&SerBlobMismatch", new object[] { this._typeToDeserialize.FullName, typeName }));
                }
                return null;
            }

            internal void ExpectingToDeserialize(RuntimeType type)
            {
                this._typeToDeserialize = type;
            }

            internal System.Runtime.Serialization.Formatters.Binary.ObjectReader ObjectReader
            {
                get
                {
                    return this._objectReader;
                }
                set
                {
                    this._objectReader = value;
                }
            }
        }
    }
}

