namespace Microsoft.SqlServer.Server
{
    using System;

    internal abstract class SmiLink
    {
        internal const ulong InterfaceVersion = 210L;

        protected SmiLink()
        {
        }

        internal abstract object GetCurrentContext(SmiEventSink eventSink);
        internal abstract ulong NegotiateVersion(ulong requestedVersion);
    }
}

