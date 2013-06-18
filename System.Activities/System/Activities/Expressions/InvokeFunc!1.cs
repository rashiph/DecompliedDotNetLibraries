namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Func")]
    public sealed class InvokeFunc<TResult> : NativeActivity<TResult>
    {
        protected override void Execute(NativeActivityContext context)
        {
            if ((this.Func != null) && (this.Func.Handler != null))
            {
                context.ScheduleFunc<TResult>(this.Func, new CompletionCallback<TResult>(this.OnActivityFuncComplete), null);
            }
        }

        private void OnActivityFuncComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, TResult resultValue)
        {
            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                base.Result.Set(context, resultValue);
            }
            else
            {
                base.Result.Set(context, default(TResult));
            }
        }

        [DefaultValue((string) null)]
        public ActivityFunc<TResult> Func { get; set; }
    }
}

