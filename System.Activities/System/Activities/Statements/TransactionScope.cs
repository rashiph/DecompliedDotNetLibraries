namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Transactions;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class TransactionScope : NativeActivity
    {
        private bool abortInstanceFlagWasExplicitlySet;
        private bool abortInstanceOnTransactionFailure = true;
        private const string AbortInstanceOnTransactionFailurePropertyName = "AbortInstanceOnTransactionFailure";
        private const string BodyPropertyName = "Body";
        private const System.Transactions.IsolationLevel defaultIsolationLevel = System.Transactions.IsolationLevel.Serializable;
        private Variable<bool> delayWasScheduled = new Variable<bool>();
        private const string IsolationLevelPropertyName = "IsolationLevel";
        private bool isTimeoutSetExplicitly;
        private Variable<TimeSpan> nestedScopeTimeout = new Variable<TimeSpan>();
        private Variable<System.Activities.ActivityInstance> nestedScopeTimeoutActivityInstance = new Variable<System.Activities.ActivityInstance>();
        private Delay nestedScopeTimeoutWorkflow;
        private Variable<RuntimeTransactionHandle> runtimeTransactionHandle = new Variable<RuntimeTransactionHandle>();
        private static string runtimeTransactionHandlePropertyName = typeof(RuntimeTransactionHandle).FullName;
        private InArgument<TimeSpan> timeout = new InArgument<TimeSpan>(TimeSpan.FromMinutes(1.0));

        public TransactionScope()
        {
            base.Constraints.Add(this.ProcessParentChainConstraints());
            base.Constraints.Add(this.ProcessChildSubtreeConstraints());
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Timeout", typeof(TimeSpan), ArgumentDirection.In, false);
            metadata.Bind(this.Timeout, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            metadata.AddImplementationChild(this.NestedScopeTimeoutWorkflow);
            if (this.Body != null)
            {
                metadata.AddChild(this.Body);
            }
            metadata.AddImplementationVariable(this.runtimeTransactionHandle);
            metadata.AddImplementationVariable(this.nestedScopeTimeout);
            metadata.AddImplementationVariable(this.delayWasScheduled);
            metadata.AddImplementationVariable(this.nestedScopeTimeoutActivityInstance);
        }

        protected override void Execute(NativeActivityContext context)
        {
            RuntimeTransactionHandle property = this.runtimeTransactionHandle.Get(context);
            RuntimeTransactionHandle handle2 = context.Properties.Find(runtimeTransactionHandlePropertyName) as RuntimeTransactionHandle;
            if (handle2 == null)
            {
                property.AbortInstanceOnTransactionFailure = this.AbortInstanceOnTransactionFailure;
                context.Properties.Add(property.ExecutionPropertyName, property);
            }
            else
            {
                if ((!handle2.IsRuntimeOwnedTransaction && this.abortInstanceFlagWasExplicitlySet) && (handle2.AbortInstanceOnTransactionFailure != this.AbortInstanceOnTransactionFailure))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.AbortInstanceOnTransactionFailureDoesNotMatch));
                }
                if (handle2.SuppressTransaction)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotNestTransactionScopeWhenAmbientHandleIsSuppressed(base.DisplayName)));
                }
                property = handle2;
            }
            Transaction currentTransaction = property.GetCurrentTransaction(context);
            if (currentTransaction == null)
            {
                property.RequestTransactionContext(context, new Action<NativeActivityTransactionContext, object>(this.OnContextAcquired), null);
            }
            else
            {
                if (currentTransaction.IsolationLevel != this.IsolationLevel)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.IsolationLevelValidation));
                }
                if (this.isTimeoutSetExplicitly)
                {
                    TimeSpan span = this.Timeout.Get(context);
                    this.delayWasScheduled.Set(context, true);
                    this.nestedScopeTimeout.Set(context, span);
                    this.nestedScopeTimeoutActivityInstance.Set(context, context.ScheduleActivity(this.NestedScopeTimeoutWorkflow, new CompletionCallback(this.OnDelayCompletion)));
                }
                this.ScheduleBody(context);
            }
        }

        private void OnCompletion(NativeActivityContext context, System.Activities.ActivityInstance instance)
        {
            RuntimeTransactionHandle handle = this.runtimeTransactionHandle.Get(context);
            if (this.delayWasScheduled.Get(context))
            {
                handle.CompleteTransaction(context, new BookmarkCallback(this.OnTransactionComplete));
            }
            else
            {
                handle.CompleteTransaction(context);
            }
        }

        private void OnContextAcquired(NativeActivityTransactionContext context, object state)
        {
            TimeSpan span = this.Timeout.Get(context);
            TransactionOptions options = new TransactionOptions {
                IsolationLevel = this.IsolationLevel,
                Timeout = span
            };
            context.SetRuntimeTransaction(new CommittableTransaction(options));
            this.ScheduleBody(context);
        }

        private void OnDelayCompletion(NativeActivityContext context, System.Activities.ActivityInstance instance)
        {
            if (instance.State == ActivityInstanceState.Closed)
            {
                RuntimeTransactionHandle handle = context.Properties.Find(runtimeTransactionHandlePropertyName) as RuntimeTransactionHandle;
                handle.GetCurrentTransaction(context).Rollback();
            }
        }

        private void OnTransactionComplete(NativeActivityContext context, Bookmark bookmark, object state)
        {
            System.Activities.ActivityInstance activityInstance = this.nestedScopeTimeoutActivityInstance.Get(context);
            if (activityInstance != null)
            {
                context.CancelChild(activityInstance);
            }
        }

        private Constraint ProcessChildSubtreeConstraints()
        {
            DelegateInArgument<System.Activities.Statements.TransactionScope> argument = new DelegateInArgument<System.Activities.Statements.TransactionScope> {
                Name = "element"
            };
            DelegateInArgument<ValidationContext> argument2 = new DelegateInArgument<ValidationContext> {
                Name = "validationContext"
            };
            DelegateInArgument<Activity> delegateArgument = new DelegateInArgument<Activity> {
                Name = "child"
            };
            Variable<bool> variable = new Variable<bool>();
            Constraint<System.Activities.Statements.TransactionScope> constraint = new Constraint<System.Activities.Statements.TransactionScope>();
            ActivityAction<System.Activities.Statements.TransactionScope, ValidationContext> action = new ActivityAction<System.Activities.Statements.TransactionScope, ValidationContext> {
                Argument1 = argument,
                Argument2 = argument2
            };
            Sequence sequence = new Sequence {
                Variables = { variable }
            };
            ForEach<Activity> item = new ForEach<Activity>();
            GetChildSubtree subtree = new GetChildSubtree {
                ValidationContext = argument2
            };
            item.Values = subtree;
            ActivityAction<Activity> action2 = new ActivityAction<Activity> {
                Argument = delegateArgument
            };
            Sequence sequence2 = new Sequence();
            If @if = new If();
            Equal<Type, Type, bool> equal = new Equal<Type, Type, bool>();
            ObtainType type = new ObtainType {
                Input = new InArgument<Activity>(delegateArgument)
            };
            equal.Left = type;
            equal.Right = new InArgument<Type>(context => typeof(CompensableActivity));
            @if.Condition = equal;
            Assign<bool> assign = new Assign<bool> {
                To = new OutArgument<bool>(variable),
                Value = new InArgument<bool>(true)
            };
            @if.Then = assign;
            sequence2.Activities.Add(@if);
            action2.Handler = sequence2;
            item.Body = action2;
            sequence.Activities.Add(item);
            AssertValidation validation = new AssertValidation();
            Not<bool, bool> expression = new Not<bool, bool>();
            VariableValue<bool> value2 = new VariableValue<bool> {
                Variable = variable
            };
            expression.Operand = value2;
            validation.Assertion = new InArgument<bool>(expression);
            validation.Message = new InArgument<string>(System.Activities.SR.CompensableActivityInsideTransactionScopeActivity);
            validation.PropertyName = "Body";
            sequence.Activities.Add(validation);
            action.Handler = sequence;
            constraint.Body = action;
            return constraint;
        }

        private Constraint ProcessParentChainConstraints()
        {
            DelegateInArgument<System.Activities.Statements.TransactionScope> element = new DelegateInArgument<System.Activities.Statements.TransactionScope> {
                Name = "element"
            };
            DelegateInArgument<ValidationContext> argument = new DelegateInArgument<ValidationContext> {
                Name = "validationContext"
            };
            DelegateInArgument<Activity> argument2 = new DelegateInArgument<Activity> {
                Name = "parent"
            };
            Constraint<System.Activities.Statements.TransactionScope> constraint = new Constraint<System.Activities.Statements.TransactionScope>();
            ActivityAction<System.Activities.Statements.TransactionScope, ValidationContext> action = new ActivityAction<System.Activities.Statements.TransactionScope, ValidationContext> {
                Argument1 = element,
                Argument2 = argument
            };
            Sequence sequence = new Sequence();
            ForEach<Activity> item = new ForEach<Activity>();
            GetParentChain chain = new GetParentChain {
                ValidationContext = argument
            };
            item.Values = chain;
            ActivityAction<Activity> action2 = new ActivityAction<Activity> {
                Argument = argument2
            };
            Sequence sequence2 = new Sequence();
            If @if = new If();
            Equal<Type, Type, bool> equal = new Equal<Type, Type, bool>();
            ObtainType type = new ObtainType {
                Input = argument2
            };
            equal.Left = type;
            equal.Right = new InArgument<Type>(context => typeof(System.Activities.Statements.TransactionScope));
            @if.Condition = equal;
            Sequence sequence3 = new Sequence();
            AssertValidation validation = new AssertValidation {
                IsWarning = 1
            };
            AbortInstanceFlagValidator validator = new AbortInstanceFlagValidator {
                ParentActivity = argument2,
                TransactionScope = new InArgument<System.Activities.Statements.TransactionScope>(context => element.Get(context))
            };
            validation.Assertion = validator;
            validation.Message = new InArgument<string>(System.Activities.SR.AbortInstanceOnTransactionFailureDoesNotMatch);
            validation.PropertyName = "AbortInstanceOnTransactionFailure";
            sequence3.Activities.Add(validation);
            AssertValidation validation2 = new AssertValidation();
            IsolationLevelValidator validator2 = new IsolationLevelValidator {
                ParentActivity = argument2,
                CurrentIsolationLevel = new InArgument<System.Transactions.IsolationLevel>(context => element.Get(context).IsolationLevel)
            };
            validation2.Assertion = validator2;
            validation2.Message = new InArgument<string>(System.Activities.SR.IsolationLevelValidation);
            validation2.PropertyName = "IsolationLevel";
            sequence3.Activities.Add(validation2);
            @if.Then = sequence3;
            sequence2.Activities.Add(@if);
            action2.Handler = sequence2;
            item.Body = action2;
            sequence.Activities.Add(item);
            action.Handler = sequence;
            constraint.Body = action;
            return constraint;
        }

        private void ScheduleBody(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body, new CompletionCallback(this.OnCompletion));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeIsolationLevel()
        {
            return (this.IsolationLevel != System.Transactions.IsolationLevel.Serializable);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTimeout()
        {
            return this.isTimeoutSetExplicitly;
        }

        [DefaultValue(true)]
        public bool AbortInstanceOnTransactionFailure
        {
            get
            {
                return this.abortInstanceOnTransactionFailure;
            }
            set
            {
                this.abortInstanceOnTransactionFailure = value;
                this.abortInstanceFlagWasExplicitlySet = true;
            }
        }

        [DefaultValue((string) null)]
        public Activity Body { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public System.Transactions.IsolationLevel IsolationLevel { get; set; }

        private Delay NestedScopeTimeoutWorkflow
        {
            get
            {
                if (this.nestedScopeTimeoutWorkflow == null)
                {
                    Delay delay = new Delay {
                        Duration = new InArgument<TimeSpan>(this.nestedScopeTimeout)
                    };
                    this.nestedScopeTimeoutWorkflow = delay;
                }
                return this.nestedScopeTimeoutWorkflow;
            }
        }

        public InArgument<TimeSpan> Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
                this.isTimeoutSetExplicitly = true;
            }
        }

        private class AbortInstanceFlagValidator : CodeActivity<bool>
        {
            protected override bool Execute(CodeActivityContext context)
            {
                Activity activity = this.ParentActivity.Get(context);
                if (activity == null)
                {
                    return true;
                }
                System.Activities.Statements.TransactionScope scope = activity as System.Activities.Statements.TransactionScope;
                System.Activities.Statements.TransactionScope scope2 = this.TransactionScope.Get(context);
                return ((scope.AbortInstanceOnTransactionFailure == scope2.AbortInstanceOnTransactionFailure) || !scope2.abortInstanceFlagWasExplicitlySet);
            }

            public InArgument<Activity> ParentActivity { get; set; }

            public InArgument<System.Activities.Statements.TransactionScope> TransactionScope { get; set; }
        }

        private class IsolationLevelValidator : CodeActivity<bool>
        {
            protected override bool Execute(CodeActivityContext context)
            {
                Activity activity = this.ParentActivity.Get(context);
                if (activity != null)
                {
                    System.Activities.Statements.TransactionScope scope = activity as System.Activities.Statements.TransactionScope;
                    if (scope.IsolationLevel != ((IsolationLevel) this.CurrentIsolationLevel.Get(context)))
                    {
                        return false;
                    }
                }
                return true;
            }

            public InArgument<IsolationLevel> CurrentIsolationLevel { get; set; }

            public InArgument<Activity> ParentActivity { get; set; }
        }

        private class ObtainType : CodeActivity<Type>
        {
            protected override Type Execute(CodeActivityContext context)
            {
                return this.Input.Get(context).GetType();
            }

            public InArgument<Activity> Input { get; set; }
        }
    }
}

