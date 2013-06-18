namespace System.Windows.Forms
{
    using System;
    using System.IO;

    public interface IFileReaderService
    {
        Stream OpenFileFromSource(string relativePath);
    }
}

