namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Action")]
    public sealed class Catch<TException> : Catch where TException: Exception
    {
        internal override ActivityDelegate GetAction()
        {
            return this.Action;
        }

        internal override void ScheduleAction(NativeActivityContext context, Exception exception, CompletionCallback completionCallback, FaultCallback faultCallback)
        {
            context.ScheduleAction<TException>(this.Action, (TException) exception, completionCallback, faultCallback);
        }

        [DefaultValue((string) null)]
        public ActivityAction<TException> Action { get; set; }

        public override Type ExceptionType
        {
            get
            {
                return typeof(TException);
            }
        }
    }
}

