namespace System.IO.Pipes
{
    using System;

    [Serializable]
    internal enum PipeState
    {
        WaitingToConnect,
        Connected,
        Broken,
        Disconnected,
        Closed
    }
}

