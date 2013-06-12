namespace System.Net
{
    using System;

    internal class HttpRequestCreator : IWebRequestCreate
    {
        public WebRequest Create(Uri Uri)
        {
            return new HttpWebRequest(Uri, null);
        }
    }
}

