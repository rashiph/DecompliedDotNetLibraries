namespace System.Xml
{
    using System;
    using System.IO;

    internal class XmlCachedStream : MemoryStream
    {
        private const int MoveBufferSize = 0x1000;
        private Uri uri;

        internal XmlCachedStream(Uri uri, Stream stream)
        {
            this.uri = uri;
            try
            {
                byte[] buffer = new byte[0x1000];
                int count = 0;
                while ((count = stream.Read(buffer, 0, 0x1000)) > 0)
                {
                    this.Write(buffer, 0, count);
                }
                base.Position = 0L;
            }
            finally
            {
                stream.Close();
            }
        }
    }
}

