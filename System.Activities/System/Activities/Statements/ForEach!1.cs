namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class ForEach<T> : NativeActivity
    {
        private CompletionCallback onChildComplete;
        private Variable<IEnumerator<T>> valueEnumerator;

        public ForEach()
        {
            this.valueEnumerator = new Variable<IEnumerator<T>>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Values", typeof(IEnumerable<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Values, argument);
            metadata.AddArgument(argument);
            metadata.AddDelegate(this.Body);
            metadata.AddImplementationVariable(this.valueEnumerator);
        }

        protected override void Execute(NativeActivityContext context)
        {
            IEnumerable<T> enumerable = this.Values.Get(context);
            if (enumerable == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ForEachRequiresNonNullValues(base.DisplayName)));
            }
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            this.valueEnumerator.Set(context, enumerator);
            if ((this.Body == null) || (this.Body.Handler == null))
            {
                while (enumerator.MoveNext())
                {
                }
                enumerator.Dispose();
            }
            else
            {
                this.InternalExecute(context, null, enumerator);
            }
        }

        private void GetStateAndExecute(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            IEnumerator<T> valueEnumerator = this.valueEnumerator.Get(context);
            this.InternalExecute(context, completedInstance, valueEnumerator);
        }

        private void InternalExecute(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, IEnumerator<T> valueEnumerator)
        {
            if (!valueEnumerator.MoveNext())
            {
                if ((completedInstance != null) && ((completedInstance.State == ActivityInstanceState.Canceled) || (context.IsCancellationRequested && (completedInstance.State == ActivityInstanceState.Faulted))))
                {
                    context.MarkCanceled();
                }
                valueEnumerator.Dispose();
            }
            else if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
                valueEnumerator.Dispose();
            }
            else
            {
                context.ScheduleAction<T>(this.Body, valueEnumerator.Current, this.OnChildComplete, null);
            }
        }

        [DefaultValue((string) null)]
        public ActivityAction<T> Body { get; set; }

        private CompletionCallback OnChildComplete
        {
            get
            {
                if (this.onChildComplete == null)
                {
                    this.onChildComplete = new CompletionCallback(this.GetStateAndExecute);
                }
                return this.onChildComplete;
            }
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<IEnumerable<T>> Values { get; set; }
    }
}

