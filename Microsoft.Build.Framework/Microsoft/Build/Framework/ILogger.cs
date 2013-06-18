namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ILogger
    {
        void Initialize(IEventSource eventSource);
        void Shutdown();

        string Parameters { get; set; }

        LoggerVerbosity Verbosity { get; set; }
    }
}

