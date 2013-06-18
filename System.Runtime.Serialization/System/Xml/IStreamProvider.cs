namespace System.Xml
{
    using System;
    using System.IO;

    public interface IStreamProvider
    {
        Stream GetStream();
        void ReleaseStream(Stream stream);
    }
}

