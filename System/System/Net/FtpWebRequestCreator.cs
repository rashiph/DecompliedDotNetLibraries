namespace System.Net
{
    using System;

    internal class FtpWebRequestCreator : IWebRequestCreate
    {
        internal FtpWebRequestCreator()
        {
        }

        public WebRequest Create(Uri uri)
        {
            return new FtpWebRequest(uri);
        }
    }
}

