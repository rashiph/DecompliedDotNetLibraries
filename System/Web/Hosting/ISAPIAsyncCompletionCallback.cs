namespace System.Web.Hosting
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void ISAPIAsyncCompletionCallback(IntPtr ecb, int byteCount, int error);
}

