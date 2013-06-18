namespace System.DirectoryServices.Protocols
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    internal class RequestState
    {
        internal bool abortCalled;
        public byte[] bufferRead = new byte[0x400];
        public const int bufferSize = 0x400;
        public DsmlAsyncResult dsmlAsync;
        public UTF8Encoding encoder = new UTF8Encoding();
        internal Exception exception;
        public HttpWebRequest request;
        public Stream requestStream;
        public string requestString;
        public Stream responseStream;
        public StringBuilder responseString = new StringBuilder(0x400);
    }
}

