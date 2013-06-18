namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Transactions;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class TransactedReceiveScope : NativeActivity
    {
        private const string AbortInstanceOnTransactionFailurePropertyName = "AbortInstanceOnTransactionFailure";
        private const string BodyPropertyName = "Body";
        private Variable<bool> isNested;
        private const string RequestPropertyName = "Request";
        private static AsyncCallback transactionCommitCallback;
        private Variable<RuntimeTransactionHandle> transactionHandle;
        private Collection<Variable> variables;

        public TransactedReceiveScope()
        {
            Variable<RuntimeTransactionHandle> variable = new Variable<RuntimeTransactionHandle> {
                Name = "TransactionHandle"
            };
            this.transactionHandle = variable;
            this.isNested = new Variable<bool>();
            base.Constraints.Add(this.ProcessChildSubtreeConstraints());
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.Request == null)
            {
                metadata.AddValidationError(new ValidationError(System.ServiceModel.Activities.SR.TransactedReceiveScopeMustHaveValidReceive(base.DisplayName), false, "Request"));
            }
            metadata.AddChild(this.Request);
            metadata.AddChild(this.Body);
            metadata.SetVariablesCollection(this.Variables);
            metadata.AddImplementationVariable(this.transactionHandle);
            metadata.AddImplementationVariable(this.isNested);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Request == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TransactedReceiveScopeRequiresReceive(base.DisplayName)));
            }
            RuntimeTransactionHandle property = this.transactionHandle.Get(context);
            context.Properties.Add(TransactedReceiveData.TransactedReceiveDataExecutionPropertyName, new TransactedReceiveData());
            RuntimeTransactionHandle handle2 = context.Properties.Find(property.ExecutionPropertyName) as RuntimeTransactionHandle;
            if (handle2 == null)
            {
                context.Properties.Add(property.ExecutionPropertyName, property);
            }
            else
            {
                if (handle2.SuppressTransaction)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CannotNestTransactedReceiveScopeWhenAmbientHandleIsSuppressed(base.DisplayName)));
                }
                if (handle2.GetCurrentTransaction(context) != null)
                {
                    property = handle2;
                    this.isNested.Set(context, true);
                }
            }
            context.ScheduleActivity(this.Request, new CompletionCallback(this.OnReceiveCompleted));
        }

        private void OnBodyCompleted(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            TransactedReceiveData data = context.Properties.Find(TransactedReceiveData.TransactedReceiveDataExecutionPropertyName) as TransactedReceiveData;
            if (!this.isNested.Get(context))
            {
                CommittableTransaction initiatingTransaction = data.InitiatingTransaction as CommittableTransaction;
                if (initiatingTransaction != null)
                {
                    initiatingTransaction.BeginCommit(TransactionCommitAsyncCallback, initiatingTransaction);
                }
                else
                {
                    (data.InitiatingTransaction as DependentTransaction).Complete();
                }
            }
            else
            {
                DependentTransaction transaction3 = data.InitiatingTransaction as DependentTransaction;
                if (transaction3 != null)
                {
                    transaction3.Complete();
                }
            }
        }

        private void OnReceiveCompleted(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body, new CompletionCallback(this.OnBodyCompleted));
            }
            else if (completedInstance.State == ActivityInstanceState.Closed)
            {
                this.OnBodyCompleted(context, completedInstance);
            }
        }

        private Constraint ProcessChildSubtreeConstraints()
        {
            DelegateInArgument<TransactedReceiveScope> argument = new DelegateInArgument<TransactedReceiveScope> {
                Name = "element"
            };
            DelegateInArgument<ValidationContext> argument2 = new DelegateInArgument<ValidationContext> {
                Name = "validationContext"
            };
            DelegateInArgument<Activity> child = new DelegateInArgument<Activity> {
                Name = "child"
            };
            Variable<bool> nestedCompensableActivity = new Variable<bool> {
                Name = "nestedCompensableActivity"
            };
            Constraint<TransactedReceiveScope> constraint = new Constraint<TransactedReceiveScope>();
            ActivityAction<TransactedReceiveScope, ValidationContext> action = new ActivityAction<TransactedReceiveScope, ValidationContext> {
                Argument1 = argument,
                Argument2 = argument2
            };
            Sequence sequence = new Sequence {
                Variables = { nestedCompensableActivity }
            };
            ForEach<Activity> item = new ForEach<Activity>();
            GetChildSubtree subtree = new GetChildSubtree {
                ValidationContext = argument2
            };
            item.Values = subtree;
            ActivityAction<Activity> action2 = new ActivityAction<Activity> {
                Argument = child
            };
            Sequence sequence2 = new Sequence();
            If @if = new If();
            Equal<Type, Type, bool> equal = new Equal<Type, Type, bool>();
            ObtainType type = new ObtainType {
                Input = new InArgument<Activity>(child)
            };
            equal.Left = type;
            equal.Right = new InArgument<Type>(context => typeof(System.Activities.Statements.TransactionScope));
            @if.Condition = equal;
            AssertValidation validation = new AssertValidation {
                IsWarning = 1
            };
            NestedChildTransactionScopeActivityAbortInstanceFlagValidator validator = new NestedChildTransactionScopeActivityAbortInstanceFlagValidator {
                Child = child
            };
            validation.Assertion = validator;
            validation.Message = new InArgument<string>(env => System.ServiceModel.Activities.SR.AbortInstanceOnTransactionFailureDoesNotMatch(child.Get(env).DisplayName, this.DisplayName));
            validation.PropertyName = "AbortInstanceOnTransactionFailure";
            @if.Then = validation;
            sequence2.Activities.Add(@if);
            If if2 = new If();
            Equal<Type, Type, bool> equal2 = new Equal<Type, Type, bool>();
            ObtainType type2 = new ObtainType {
                Input = new InArgument<Activity>(child)
            };
            equal2.Left = type2;
            equal2.Right = new InArgument<Type>(context => typeof(CompensableActivity));
            if2.Condition = equal2;
            Assign<bool> assign = new Assign<bool> {
                To = new OutArgument<bool>(nestedCompensableActivity),
                Value = new InArgument<bool>(true)
            };
            if2.Then = assign;
            sequence2.Activities.Add(if2);
            action2.Handler = sequence2;
            item.Body = action2;
            sequence.Activities.Add(item);
            AssertValidation validation2 = new AssertValidation {
                Assertion = new InArgument<bool>(env => !nestedCompensableActivity.Get(env)),
                Message = new InArgument<string>(System.ServiceModel.Activities.SR.CompensableActivityInsideTransactedReceiveScope),
                PropertyName = "Body"
            };
            sequence.Activities.Add(validation2);
            action.Handler = sequence;
            constraint.Body = action;
            return constraint;
        }

        private static void TransactionCommitCallback(IAsyncResult result)
        {
            CommittableTransaction asyncState = result.AsyncState as CommittableTransaction;
            try
            {
                asyncState.EndCommit(result);
            }
            catch (TransactionException exception)
            {
                if (System.ServiceModel.Activities.TD.TransactedReceiveScopeEndCommitFailedIsEnabled())
                {
                    System.ServiceModel.Activities.TD.TransactedReceiveScopeEndCommitFailed(asyncState.TransactionInformation.LocalIdentifier, exception.Message);
                }
            }
        }

        [DefaultValue((string) null)]
        public Activity Body { get; set; }

        [DefaultValue((string) null)]
        public Receive Request { get; set; }

        internal static AsyncCallback TransactionCommitAsyncCallback
        {
            get
            {
                if (transactionCommitCallback == null)
                {
                    transactionCommitCallback = Fx.ThunkCallback(new AsyncCallback(TransactedReceiveScope.TransactionCommitCallback));
                }
                return transactionCommitCallback;
            }
        }

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
                                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.variables = validatings;
                }
                return this.variables;
            }
        }

        private class NestedChildTransactionScopeActivityAbortInstanceFlagValidator : CodeActivity<bool>
        {
            protected override bool Execute(CodeActivityContext context)
            {
                Activity activity = this.Child.Get(context);
                if (activity != null)
                {
                    System.Activities.Statements.TransactionScope scope = activity as System.Activities.Statements.TransactionScope;
                    return scope.AbortInstanceOnTransactionFailure;
                }
                return true;
            }

            public InArgument<Activity> Child { get; set; }
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

