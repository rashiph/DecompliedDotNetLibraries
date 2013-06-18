namespace System.Xml
{
    using System;

    internal static class MimeGlobals
    {
        internal static byte[] BoundaryPrefix = new byte[] { 13, 10, 0x2d, 0x2d };
        internal static byte[] COLONSPACE = new byte[] { 0x3a, 0x20 };
        internal static string ContentIDHeader = "Content-ID";
        internal static string ContentIDScheme = "cid:";
        internal static string ContentTransferEncodingHeader = "Content-Transfer-Encoding";
        internal static string ContentTypeHeader = "Content-Type";
        internal static byte[] CRLF = new byte[] { 13, 10 };
        internal static byte[] DASHDASH = new byte[] { 0x2d, 0x2d };
        internal static string DefaultVersion = "1.0";
        internal static string Encoding8bit = "8bit";
        internal static string EncodingBinary = "binary";
        internal static string MimeVersionHeader = "MIME-Version";
    }
}

