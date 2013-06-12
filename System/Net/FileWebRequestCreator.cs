namespace System.Net
{
    using System;

    internal class FileWebRequestCreator : IWebRequestCreate
    {
        internal FileWebRequestCreator()
        {
        }

        public WebRequest Create(Uri uri)
        {
            return new FileWebRequest(uri);
        }
    }
}

