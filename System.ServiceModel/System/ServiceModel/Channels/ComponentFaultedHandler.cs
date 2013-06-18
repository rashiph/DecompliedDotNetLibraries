namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void ComponentFaultedHandler(Exception faultException, WsrmFault fault);
}

