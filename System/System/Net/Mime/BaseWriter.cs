namespace System.Net.Mime
{
    using System;
    using System.Collections.Specialized;
    using System.IO;

    internal abstract class BaseWriter
    {
        protected BaseWriter()
        {
        }

        internal abstract IAsyncResult BeginGetContentStream(AsyncCallback callback, object state);
        internal abstract void Close();
        internal abstract Stream EndGetContentStream(IAsyncResult result);
        internal abstract Stream GetContentStream();
        internal abstract void WriteHeader(string name, string value);
        internal abstract void WriteHeaders(NameValueCollection headers);
    }
}

