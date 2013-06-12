namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate PooledStream CreateConnectionDelegate(ConnectionPool pool);
}

