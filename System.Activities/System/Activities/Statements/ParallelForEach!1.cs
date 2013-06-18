namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class ParallelForEach<T> : NativeActivity
    {
        private Variable<bool> hasCompleted;
        private CompletionCallback<bool> onConditionComplete;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Values", typeof(IEnumerable<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Values, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            if (this.CompletionCondition != null)
            {
                metadata.SetChildrenCollection(new Collection<Activity> { this.CompletionCondition });
            }
            if (this.CompletionCondition != null)
            {
                if (this.hasCompleted == null)
                {
                    this.hasCompleted = new Variable<bool>();
                }
                metadata.AddImplementationVariable(this.hasCompleted);
            }
            metadata.AddDelegate(this.Body);
        }

        protected override void Execute(NativeActivityContext context)
        {
            IEnumerable<T> enumerable = this.Values.Get(context);
            if (enumerable == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ParallelForEachRequiresNonNullValues(base.DisplayName)));
            }
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            CompletionCallback onCompleted = new CompletionCallback(this.OnBodyComplete);
            while (enumerator.MoveNext())
            {
                if (this.Body != null)
                {
                    context.ScheduleAction<T>(this.Body, enumerator.Current, onCompleted, null);
                }
            }
            enumerator.Dispose();
        }

        private void OnBodyComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            if ((this.CompletionCondition != null) && !this.hasCompleted.Get(context))
            {
                if ((completedInstance.State != ActivityInstanceState.Closed) && context.IsCancellationRequested)
                {
                    context.MarkCanceled();
                    this.hasCompleted.Set(context, true);
                }
                else
                {
                    if (this.onConditionComplete == null)
                    {
                        this.onConditionComplete = new CompletionCallback<bool>(this.OnConditionComplete);
                    }
                    context.ScheduleActivity<bool>(this.CompletionCondition, this.onConditionComplete, null);
                }
            }
        }

        private void OnConditionComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, bool result)
        {
            if (result)
            {
                context.CancelChildren();
                this.hasCompleted.Set(context, true);
            }
        }

        [DefaultValue((string) null)]
        public ActivityAction<T> Body { get; set; }

        [DefaultValue((string) null)]
        public Activity<bool> CompletionCondition { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<IEnumerable<T>> Values { get; set; }
    }
}

