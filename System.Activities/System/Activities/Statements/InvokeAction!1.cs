namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Action")]
    public sealed class InvokeAction<T> : NativeActivity
    {
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(this.Action);
            RuntimeArgument argument = new RuntimeArgument("Argument", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Argument, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
        }

        protected override void Execute(NativeActivityContext context)
        {
            if ((this.Action != null) && (this.Action.Handler != null))
            {
                context.ScheduleAction<T>(this.Action, this.Argument.Get(context), null, null);
            }
        }

        [DefaultValue((string) null)]
        public ActivityAction<T> Action { get; set; }

        [RequiredArgument]
        public InArgument<T> Argument { get; set; }
    }
}

