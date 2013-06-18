namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void SendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException);
}

