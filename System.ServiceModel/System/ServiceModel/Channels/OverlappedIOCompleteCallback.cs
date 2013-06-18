namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void OverlappedIOCompleteCallback(bool haveResult, int error, int bytesRead);
}

