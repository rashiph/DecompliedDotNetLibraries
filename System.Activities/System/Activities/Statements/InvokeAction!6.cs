namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6> : NativeActivity
    {
        protected override void Execute(NativeActivityContext context)
        {
            if ((this.Action != null) && (this.Action.Handler != null))
            {
                context.ScheduleAction<T1, T2, T3, T4, T5, T6>(this.Action, this.Argument1.Get(context), this.Argument2.Get(context), this.Argument3.Get(context), this.Argument4.Get(context), this.Argument5.Get(context), this.Argument6.Get(context), null, null);
            }
        }

        [DefaultValue((string) null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6> Action { get; set; }

        [RequiredArgument]
        public InArgument<T1> Argument1 { get; set; }

        [RequiredArgument]
        public InArgument<T2> Argument2 { get; set; }

        [RequiredArgument]
        public InArgument<T3> Argument3 { get; set; }

        [RequiredArgument]
        public InArgument<T4> Argument4 { get; set; }

        [RequiredArgument]
        public InArgument<T5> Argument5 { get; set; }

        [RequiredArgument]
        public InArgument<T6> Argument6 { get; set; }
    }
}

