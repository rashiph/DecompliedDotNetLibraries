namespace Microsoft.Build.Framework
{
    using System;

    public interface IEventRedirector
    {
        void ForwardEvent(BuildEventArgs buildEvent);
    }
}

