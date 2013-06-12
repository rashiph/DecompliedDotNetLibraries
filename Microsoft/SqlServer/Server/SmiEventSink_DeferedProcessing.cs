namespace Microsoft.SqlServer.Server
{
    using System;

    internal class SmiEventSink_DeferedProcessing : SmiEventSink_Default
    {
        internal SmiEventSink_DeferedProcessing(SmiEventSink parent) : base(parent)
        {
        }

        protected override void DispatchMessages(bool ignoreNonFatalMessages)
        {
        }
    }
}

