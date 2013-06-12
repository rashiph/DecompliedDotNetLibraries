namespace System.IO.Compression
{
    using System;

    internal interface IFileFormatWriter
    {
        byte[] GetFooter();
        byte[] GetHeader();
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
    }
}

