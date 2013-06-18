namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface INodeLogger : ILogger
    {
        void Initialize(IEventSource eventSource, int nodeCount);
    }
}

