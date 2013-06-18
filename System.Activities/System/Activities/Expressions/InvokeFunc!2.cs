namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Func")]
    public sealed class InvokeFunc<T, TResult> : NativeActivity<TResult>
    {
        protected override void Execute(NativeActivityContext context)
        {
            if ((this.Func != null) && (this.Func.Handler != null))
            {
                context.ScheduleFunc<T, TResult>(this.Func, this.Argument.Get(context), new CompletionCallback<TResult>(this.OnActivityFuncComplete), null);
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

        [RequiredArgument]
        public InArgument<T> Argument { get; set; }

        public ActivityFunc<T, TResult> Func { get; set; }
    }
}

