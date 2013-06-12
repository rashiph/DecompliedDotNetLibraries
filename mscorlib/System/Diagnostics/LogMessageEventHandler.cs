namespace System.Diagnostics
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    internal delegate void LogMessageEventHandler(LoggingLevels level, LogSwitch category, string message, StackTrace location);
}

