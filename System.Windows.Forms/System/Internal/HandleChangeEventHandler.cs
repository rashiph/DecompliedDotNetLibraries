namespace System.Internal
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void HandleChangeEventHandler(string handleType, IntPtr handleValue, int currentHandleCount);
}

