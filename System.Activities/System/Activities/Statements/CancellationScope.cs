namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class CancellationScope : NativeActivity
    {
        private Variable<bool> suppressCancel = new Variable<bool>();
        private Collection<Variable> variables;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(this.Body);
            metadata.AddChild(this.CancellationHandler);
            metadata.SetVariablesCollection(this.Variables);
            metadata.AddImplementationVariable(this.suppressCancel);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            if (!this.suppressCancel.Get(context))
            {
                context.CancelChildren();
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body, new CompletionCallback(this.OnBodyComplete));
            }
        }

        private void OnBodyComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            if ((completedInstance.State == ActivityInstanceState.Canceled) || (context.IsCancellationRequested && (completedInstance.State == ActivityInstanceState.Faulted)))
            {
                this.suppressCancel.Set(context, true);
                context.MarkCanceled();
                if (this.CancellationHandler != null)
                {
                    FaultCallback onFaulted = new FaultCallback(this.OnExceptionFromCancelHandler);
                    context.ScheduleActivity(this.CancellationHandler, onFaulted);
                }
            }
        }

        private void OnExceptionFromCancelHandler(NativeActivityFaultContext context, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom)
        {
            this.suppressCancel.Set(context, false);
        }

        [DependsOn("Variables"), DefaultValue((string) null)]
        public Activity Body { get; set; }

        [DependsOn("Body"), DefaultValue((string) null)]
        public Activity CancellationHandler { get; set; }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    ValidatingCollection<Variable> validatings = new ValidatingCollection<Variable> {
                        OnAddValidationCallback = delegate (Variable item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.variables = validatings;
                }
                return this.variables;
            }
        }
    }
}

