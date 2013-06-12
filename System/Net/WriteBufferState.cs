namespace System.Net
{
    using System;

    internal enum WriteBufferState
    {
        Disabled,
        Headers,
        Buffer,
        Playback
    }
}

