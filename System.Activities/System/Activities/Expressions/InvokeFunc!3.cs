namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Func")]
    public sealed class InvokeFunc<T1, T2, TResult> : NativeActivity<TResult>
    {
        protected override void Execute(NativeActivityContext context)
        {
            if ((this.Func != null) && (this.Func.Handler != null))
            {
                context.ScheduleFunc<T1, T2, TResult>(this.Func, this.Argument1.Get(context), this.Argument2.Get(context), new CompletionCallback<TResult>(this.OnActivityFuncComplete), null);
            }
        }

        private void OnActivityFuncComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, TResult resultValue)
        {
            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                base.Result.Set(context, resultValue);
            }
        }

        [RequiredArgument]
        public InArgument<T1> Argument1 { get; set; }

        [RequiredArgument]
        public InArgument<T2> Argument2 { get; set; }

        [DefaultValue((string) null)]
        public ActivityFunc<T1, T2, TResult> Func { get; set; }
    }
}

