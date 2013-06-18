namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal sealed class DefaultConfirmation : NativeActivity
    {
        private Activity body;
        private CompletionCallback onChildConfirmed;
        private Variable<CompensationToken> toConfirmToken = new Variable<CompensationToken>();

        public DefaultConfirmation()
        {
            InternalConfirm confirm = new InternalConfirm {
                Target = new InArgument<CompensationToken>(this.toConfirmToken)
            };
            this.body = confirm;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.toConfirmToken });
            metadata.SetImplementationChildrenCollection(new Collection<Activity> { this.Body });
        }

        protected override void Cancel(NativeActivityContext context)
        {
        }

        protected override void Execute(NativeActivityContext context)
        {
            this.InternalExecute(context, null);
        }

        private void InternalExecute(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            if (extension == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ConfirmWithoutCompensableActivity(base.DisplayName)));
            }
            CompensationToken token = this.Target.Get(context);
            CompensationTokenData data = (token == null) ? null : extension.Get(token.CompensationId);
            if (data.ExecutionTracker.Count > 0)
            {
                if (this.onChildConfirmed == null)
                {
                    this.onChildConfirmed = new CompletionCallback(this.InternalExecute);
                }
                this.toConfirmToken.Set(context, new CompensationToken(data.ExecutionTracker.Get()));
                context.ScheduleActivity(this.Body, this.onChildConfirmed);
            }
        }

        private Activity Body
        {
            get
            {
                return this.body;
            }
        }

        public InArgument<CompensationToken> Target { get; set; }
    }
}

