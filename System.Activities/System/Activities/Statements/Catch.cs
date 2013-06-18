namespace System.Activities.Statements
{
    using System;
    using System.Activities;

    public abstract class Catch
    {
        internal Catch()
        {
        }

        internal abstract ActivityDelegate GetAction();
        internal abstract void ScheduleAction(NativeActivityContext context, Exception exception, CompletionCallback completionCallback, FaultCallback faultCallback);

        public abstract Type ExceptionType { get; }
    }
}

