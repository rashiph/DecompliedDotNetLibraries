namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void CompletionDelegate(byte[] responseBytes, Exception exception, object State);
}

