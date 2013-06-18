namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void ConnectionAvailableCallback(IConnection connection, Action connectionDequeuedCallback);
}

