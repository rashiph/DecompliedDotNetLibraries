namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Duration")]
    public sealed class Delay : NativeActivity
    {
        private static Func<TimerExtension> getDefaultTimerExtension = new Func<TimerExtension>(Delay.GetDefaultTimerExtension);
        private Variable<Bookmark> timerBookmark = new Variable<Bookmark>();

        protected override void Abort(NativeActivityAbortContext context)
        {
            Bookmark bookmark = this.timerBookmark.Get(context);
            if (bookmark != null)
            {
                this.GetTimerExtension(context).CancelTimer(bookmark);
            }
            base.Abort(context);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Duration", typeof(TimeSpan), ArgumentDirection.In, true);
            metadata.Bind(this.Duration, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            metadata.AddImplementationVariable(this.timerBookmark);
            metadata.AddDefaultExtensionProvider<TimerExtension>(getDefaultTimerExtension);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            Bookmark bookmark = this.timerBookmark.Get(context);
            this.GetTimerExtension(context).CancelTimer(bookmark);
            context.RemoveBookmark(bookmark);
            context.MarkCanceled();
        }

        protected override void Execute(NativeActivityContext context)
        {
            TimeSpan actualValue = this.Duration.Get(context);
            if (actualValue < TimeSpan.Zero)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("Duration", actualValue, System.Activities.SR.DurationIsNegative(base.DisplayName));
            }
            if (actualValue != TimeSpan.Zero)
            {
                TimerExtension timerExtension = this.GetTimerExtension(context);
                Bookmark bookmark = context.CreateBookmark();
                timerExtension.RegisterTimer(actualValue, bookmark);
                this.timerBookmark.Set(context, bookmark);
            }
        }

        private static TimerExtension GetDefaultTimerExtension()
        {
            return new DurableTimerExtension();
        }

        private TimerExtension GetTimerExtension(ActivityContext context)
        {
            return context.GetExtension<TimerExtension>();
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        [DefaultValue((string) null), RequiredArgument]
        public InArgument<TimeSpan> Duration { get; set; }
    }
}

