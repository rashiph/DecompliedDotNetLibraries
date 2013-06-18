namespace System.Activities
{
    using System;

    internal interface IInstanceNotificationListener
    {
        void AbortInstance(Exception reason, bool isWorkflowThread);
        void OnIdle();
        bool OnUnhandledException(Exception exception, Activity exceptionSource);
    }
}

