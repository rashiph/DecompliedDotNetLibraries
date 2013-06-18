namespace System.Workflow.Runtime.Tracking
{
    using System;

    public interface IProfileNotification
    {
        event EventHandler<ProfileRemovedEventArgs> ProfileRemoved;

        event EventHandler<ProfileUpdatedEventArgs> ProfileUpdated;
    }
}

