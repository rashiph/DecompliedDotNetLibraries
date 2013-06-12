namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum ServerProcessing
    {
        Complete,
        OneWay,
        Async
    }
}

