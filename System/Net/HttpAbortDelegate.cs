namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate bool HttpAbortDelegate(HttpWebRequest request, WebException webException);
}

