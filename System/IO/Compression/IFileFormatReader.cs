namespace System.IO.Compression
{
    using System;

    internal interface IFileFormatReader
    {
        bool ReadFooter(InputBuffer input);
        bool ReadHeader(InputBuffer input);
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
        void Validate();
    }
}

