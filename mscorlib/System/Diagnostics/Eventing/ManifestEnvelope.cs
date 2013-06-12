namespace System.Diagnostics.Eventing
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ManifestEnvelope
    {
        public const int MaxChunkSize = 0xff00;
        public ManifestFormats Format;
        public byte MajorVersion;
        public byte MinorVersion;
        public byte Magic;
        public ushort TotalChunks;
        public ushort ChunkNumber;
        public enum ManifestFormats : byte
        {
            SimpleXmlFormat = 1
        }
    }
}

