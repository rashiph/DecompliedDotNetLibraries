namespace System.Text
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal abstract class BaseCodePageEncoding : EncodingNLS, ISerializable
    {
        [NonSerialized]
        protected char[] arrayBytesBestFit;
        [NonSerialized]
        protected char[] arrayUnicodeBestFit;
        [NonSerialized]
        protected bool bFlagDataTable;
        internal const string CODE_PAGE_DATA_FILE_NAME = "codepages.nlp";
        [NonSerialized]
        protected int dataTableCodePage;
        [NonSerialized]
        protected int iExtraBytes;
        [NonSerialized]
        protected bool m_bUseMlangTypeForSerialization;
        private unsafe static CodePageDataFileHeader* m_pCodePageFileHeader = ((CodePageDataFileHeader*) GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(CharUnicodeInfo).Assembly, "codepages.nlp"));
        [NonSerialized]
        protected unsafe CodePageHeader* pCodePage;
        [NonSerialized, SecurityCritical]
        protected SafeFileMappingHandle safeFileMappingHandle;
        [NonSerialized, SecurityCritical]
        protected SafeViewOfFileHandle safeMemorySectionHandle;

        [SecurityCritical]
        internal BaseCodePageEncoding(int codepage) : this(codepage, codepage)
        {
        }

        [SecurityCritical]
        internal unsafe BaseCodePageEncoding(int codepage, int dataCodePage) : base((codepage == 0) ? Win32Native.GetACP() : codepage)
        {
            this.bFlagDataTable = true;
            this.pCodePage = null;
            this.dataTableCodePage = dataCodePage;
            this.LoadCodePageTables();
        }

        [SecurityCritical]
        internal unsafe BaseCodePageEncoding(SerializationInfo info, StreamingContext context) : base(0)
        {
            this.bFlagDataTable = true;
            this.pCodePage = null;
            throw new ArgumentNullException("this");
        }

        [SecurityCritical]
        internal void CheckMemorySection()
        {
            if ((this.safeMemorySectionHandle != null) && (this.safeMemorySectionHandle.DangerousGetHandle() == IntPtr.Zero))
            {
                this.LoadManagedCodePage();
            }
        }

        [SecurityCritical]
        private static unsafe CodePageHeader* FindCodePage(int codePage)
        {
            for (int i = 0; i < m_pCodePageFileHeader.CodePageCount; i++)
            {
                CodePageIndex* indexPtr = &m_pCodePageFileHeader.CodePages + i;
                if (indexPtr->CodePage == codePage)
                {
                    return (CodePageHeader*) (m_pCodePageFileHeader + indexPtr->Offset);
                }
            }
            return null;
        }

        [SecuritySafeCritical]
        internal override char[] GetBestFitBytesToUnicodeData()
        {
            if (this.arrayBytesBestFit == null)
            {
                this.ReadBestFitTable();
            }
            return this.arrayBytesBestFit;
        }

        [SecuritySafeCritical]
        internal override char[] GetBestFitUnicodeToBytesData()
        {
            if (this.arrayUnicodeBestFit == null)
            {
                this.ReadBestFitTable();
            }
            return this.arrayUnicodeBestFit;
        }

        [SecurityCritical]
        internal static unsafe int GetCodePageByteSize(int codePage)
        {
            CodePageHeader* headerPtr = FindCodePage(codePage);
            if (headerPtr == null)
            {
                return 0;
            }
            return headerPtr->ByteCount;
        }

        [SecurityCritical]
        protected virtual unsafe string GetMemorySectionName()
        {
            int num = this.bFlagDataTable ? this.dataTableCodePage : this.CodePage;
            return string.Format(CultureInfo.InvariantCulture, "NLS_CodePage_{0}_{1}_{2}_{3}_{4}", new object[] { num, this.pCodePage.VersionMajor, this.pCodePage.VersionMinor, this.pCodePage.VersionRevision, this.pCodePage.VersionBuild });
        }

        [SecurityCritical]
        protected unsafe byte* GetSharedMemory(int iSize)
        {
            IntPtr ptr;
            byte* numPtr = EncodingTable.nativeCreateOpenFileMapping(this.GetMemorySectionName(), iSize, out ptr);
            if (numPtr == null)
            {
                throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
            }
            if (ptr != IntPtr.Zero)
            {
                this.safeMemorySectionHandle = new SafeViewOfFileHandle((IntPtr) numPtr, true);
                this.safeFileMappingHandle = new SafeFileMappingHandle(ptr, true);
            }
            return numPtr;
        }

        [SecurityCritical]
        private unsafe void LoadCodePageTables()
        {
            CodePageHeader* headerPtr = FindCodePage(this.dataTableCodePage);
            if (headerPtr == null)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoCodepageData", new object[] { this.CodePage }));
            }
            this.pCodePage = headerPtr;
            this.LoadManagedCodePage();
        }

        [SecurityCritical]
        protected abstract void LoadManagedCodePage();
        [SecurityCritical]
        protected abstract void ReadBestFitTable();
        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.SerializeEncoding(info, context);
            info.AddValue(this.m_bUseMlangTypeForSerialization ? "m_maxByteSize" : "maxCharSize", this.IsSingleByte ? 1 : 2);
            info.SetType(this.m_bUseMlangTypeForSerialization ? typeof(MLangCodePageEncoding) : typeof(CodePageEncoding));
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct CodePageDataFileHeader
        {
            [FieldOffset(40)]
            internal short CodePageCount;
            [FieldOffset(0x2c)]
            internal BaseCodePageEncoding.CodePageIndex CodePages;
            [FieldOffset(0)]
            internal char TableName;
            [FieldOffset(0x2a)]
            internal short unused1;
            [FieldOffset(0x20)]
            internal ushort Version;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct CodePageHeader
        {
            [FieldOffset(0x2a)]
            internal short ByteCount;
            [FieldOffset(0x2e)]
            internal ushort ByteReplace;
            [FieldOffset(40)]
            internal short CodePage;
            [FieldOffset(0)]
            internal char CodePageName;
            [FieldOffset(0x30)]
            internal short FirstDataWord;
            [FieldOffset(0x2c)]
            internal char UnicodeReplace;
            [FieldOffset(0x26)]
            internal ushort VersionBuild;
            [FieldOffset(0x20)]
            internal ushort VersionMajor;
            [FieldOffset(0x22)]
            internal ushort VersionMinor;
            [FieldOffset(0x24)]
            internal ushort VersionRevision;
        }

        [StructLayout(LayoutKind.Explicit, Pack=2)]
        internal struct CodePageIndex
        {
            [FieldOffset(0x22)]
            internal short ByteCount;
            [FieldOffset(0x20)]
            internal short CodePage;
            [FieldOffset(0)]
            internal char CodePageName;
            [FieldOffset(0x24)]
            internal int Offset;
        }
    }
}

