namespace System.Runtime.Serialization.Diagnostics
{
    using System;

    internal static class TraceCode
    {
        public const int ElementIgnored = 0x30007;
        public const int FactoryTypeNotFound = 0x30011;
        public const int ObjectWithLargeDepth = 0x30012;
        public const int ReadObjectBegin = 0x30005;
        public const int ReadObjectEnd = 0x30006;
        public const int Serialization = 0x30000;
        public const int WriteObjectBegin = 0x30001;
        public const int WriteObjectContentBegin = 0x30003;
        public const int WriteObjectContentEnd = 0x30004;
        public const int WriteObjectEnd = 0x30002;
        public const int XsdExportAnnotationFailed = 0x3000e;
        public const int XsdExportBegin = 0x30008;
        public const int XsdExportDupItems = 0x30010;
        public const int XsdExportEnd = 0x30009;
        public const int XsdExportError = 0x3000c;
        public const int XsdImportAnnotationFailed = 0x3000f;
        public const int XsdImportBegin = 0x3000a;
        public const int XsdImportEnd = 0x3000b;
        public const int XsdImportError = 0x3000d;
    }
}

