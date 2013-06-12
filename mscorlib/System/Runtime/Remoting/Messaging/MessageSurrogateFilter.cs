namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public delegate bool MessageSurrogateFilter(string key, object value);
}

