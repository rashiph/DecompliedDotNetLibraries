namespace Microsoft.Build.Framework
{
    using System;

    public interface IEventSource
    {
        event AnyEventHandler AnyEventRaised;

        event BuildFinishedEventHandler BuildFinished;

        event BuildStartedEventHandler BuildStarted;

        event CustomBuildEventHandler CustomEventRaised;

        event BuildErrorEventHandler ErrorRaised;

        event BuildMessageEventHandler MessageRaised;

        event ProjectFinishedEventHandler ProjectFinished;

        event ProjectStartedEventHandler ProjectStarted;

        event BuildStatusEventHandler StatusEventRaised;

        event TargetFinishedEventHandler TargetFinished;

        event TargetStartedEventHandler TargetStarted;

        event TaskFinishedEventHandler TaskFinished;

        event TaskStartedEventHandler TaskStarted;

        event BuildWarningEventHandler WarningRaised;
    }
}

