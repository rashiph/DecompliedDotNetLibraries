namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate bool SyncUpdateCallback(SyncFromAllServersEvent eventType, string targetServer, string sourceServer, SyncFromAllServersOperationException exception);
}

