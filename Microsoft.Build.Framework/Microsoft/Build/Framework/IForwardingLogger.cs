namespace Microsoft.Build.Framework
{
    using System;

    public interface IForwardingLogger : INodeLogger, ILogger
    {
        IEventRedirector BuildEventRedirector { get; set; }

        int NodeId { get; set; }
    }
}

