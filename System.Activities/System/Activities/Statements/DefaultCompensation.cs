namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal sealed class DefaultCompensation : NativeActivity
    {
        private Activity body;
        private CompletionCallback onChildCompensated;
        private Variable<CompensationToken> toCompensateToken = new Variable<CompensationToken>();

        public DefaultCompensation()
        {
            InternalCompensate compensate = new InternalCompensate {
                Target = new InArgument<CompensationToken>(this.toCompensateToken)
            };
            this.body = compensate;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.toCompensateToken });
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
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CompensateWithoutCompensableActivity(base.DisplayName)));
            }
            CompensationToken token = this.Target.Get(context);
            CompensationTokenData data = (token == null) ? null : extension.Get(token.CompensationId);
            if (data.ExecutionTracker.Count > 0)
            {
                if (this.onChildCompensated == null)
                {
                    this.onChildCompensated = new CompletionCallback(this.InternalExecute);
                }
                this.toCompensateToken.Set(context, new CompensationToken(data.ExecutionTracker.Get()));
                context.ScheduleActivity(this.Body, this.onChildCompensated);
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

