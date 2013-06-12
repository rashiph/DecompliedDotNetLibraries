namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WriteHeadersCallbackState
    {
        internal HttpWebRequest request;
        internal ConnectStream stream;
        internal WriteHeadersCallbackState(HttpWebRequest request, ConnectStream stream)
        {
            this.request = request;
            this.stream = stream;
        }
    }
}

