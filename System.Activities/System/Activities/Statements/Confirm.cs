namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public sealed class Confirm : NativeActivity
    {
        private static Constraint confirmWithNoTarget = ConfirmWithNoTarget();
        private Variable<CompensationToken> currentCompensationToken = new Variable<CompensationToken>();
        private System.Activities.Statements.DefaultConfirmation defaultConfirmation;
        private System.Activities.Statements.InternalConfirm internalConfirm;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.currentCompensationToken });
            metadata.SetImplementationChildrenCollection(new Collection<Activity> { this.DefaultConfirmation, this.InternalConfirm });
        }

        protected override void Cancel(NativeActivityContext context)
        {
        }

        private static Constraint ConfirmWithNoTarget()
        {
            DelegateInArgument<Confirm> element = new DelegateInArgument<Confirm> {
                Name = "element"
            };
            DelegateInArgument<ValidationContext> argument = new DelegateInArgument<ValidationContext> {
                Name = "validationContext"
            };
            Variable<bool> assertFlag = new Variable<bool> {
                Name = "assertFlag"
            };
            Variable<IEnumerable<Activity>> elements = new Variable<IEnumerable<Activity>> {
                Name = "elements"
            };
            Variable<int> index = new Variable<int> {
                Name = "index"
            };
            Constraint<Confirm> constraint = new Constraint<Confirm>();
            ActivityAction<Confirm, ValidationContext> action = new ActivityAction<Confirm, ValidationContext> {
                Argument1 = element,
                Argument2 = argument
            };
            Sequence sequence = new Sequence {
                Variables = { assertFlag, elements, index }
            };
            If item = new If {
                Condition = new InArgument<bool>(env => element.Get(env).Target != null)
            };
            Assign<bool> assign = new Assign<bool> {
                To = assertFlag,
                Value = 1
            };
            item.Then = assign;
            Sequence sequence2 = new Sequence();
            Assign<IEnumerable<Activity>> assign2 = new Assign<IEnumerable<Activity>> {
                To = elements
            };
            GetParentChain chain = new GetParentChain {
                ValidationContext = argument
            };
            assign2.Value = chain;
            sequence2.Activities.Add(assign2);
            While @while = new While(env => !assertFlag.Get(env) && (index.Get(env) < elements.Get(env).Count<Activity>()));
            Sequence sequence3 = new Sequence();
            If if2 = new If(env => elements.Get(env).ElementAt<Activity>(index.Get(env)).GetType() == typeof(CompensationParticipant));
            Assign<bool> assign3 = new Assign<bool> {
                To = assertFlag,
                Value = 1
            };
            if2.Then = assign3;
            sequence3.Activities.Add(if2);
            Assign<int> assign4 = new Assign<int> {
                To = index,
                Value = new InArgument<int>(env => index.Get(env) + 1)
            };
            sequence3.Activities.Add(assign4);
            @while.Body = sequence3;
            sequence2.Activities.Add(@while);
            item.Else = sequence2;
            sequence.Activities.Add(item);
            AssertValidation validation = new AssertValidation {
                Assertion = new InArgument<bool>(assertFlag),
                Message = new InArgument<string>(System.Activities.SR.ConfirmWithNoTargetConstraint)
            };
            sequence.Activities.Add(validation);
            action.Handler = sequence;
            constraint.Body = action;
            return constraint;
        }

        protected override void Execute(NativeActivityContext context)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            if (extension == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ConfirmWithoutCompensableActivity(base.DisplayName)));
            }
            if (this.Target.IsEmpty)
            {
                CompensationToken token = (CompensationToken) context.Properties.Find("System.Compensation.CompensationToken");
                CompensationTokenData data = (token == null) ? null : extension.Get(token.CompensationId);
                if ((data == null) || !data.IsTokenValidInSecondaryRoot)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidConfirmActivityUsage(base.DisplayName)));
                }
                this.currentCompensationToken.Set(context, token);
                if (data.ExecutionTracker.Count > 0)
                {
                    context.ScheduleActivity(this.DefaultConfirmation);
                }
            }
            else
            {
                CompensationToken token2 = this.Target.Get(context);
                CompensationTokenData data2 = (token2 == null) ? null : extension.Get(token2.CompensationId);
                if (token2 == null)
                {
                    throw FxTrace.Exception.Argument("Target", System.Activities.SR.InvalidCompensationToken(base.DisplayName));
                }
                if (!token2.ConfirmCalled)
                {
                    if ((data2 == null) || (data2.CompensationState != CompensationState.Completed))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CompensableActivityAlreadyConfirmedOrCompensated));
                    }
                    data2.CompensationState = CompensationState.Confirming;
                    token2.ConfirmCalled = true;
                    context.ScheduleActivity(this.InternalConfirm);
                }
            }
        }

        internal override IList<Constraint> InternalGetConstraints()
        {
            return new List<Constraint>(1) { confirmWithNoTarget };
        }

        private System.Activities.Statements.DefaultConfirmation DefaultConfirmation
        {
            get
            {
                if (this.defaultConfirmation == null)
                {
                    System.Activities.Statements.DefaultConfirmation confirmation = new System.Activities.Statements.DefaultConfirmation {
                        Target = new InArgument<CompensationToken>(this.currentCompensationToken)
                    };
                    this.defaultConfirmation = confirmation;
                }
                return this.defaultConfirmation;
            }
        }

        private System.Activities.Statements.InternalConfirm InternalConfirm
        {
            get
            {
                if (this.internalConfirm == null)
                {
                    System.Activities.Statements.InternalConfirm confirm = new System.Activities.Statements.InternalConfirm();
                    ArgumentValue<CompensationToken> expression = new ArgumentValue<CompensationToken> {
                        ArgumentName = "Target"
                    };
                    confirm.Target = new InArgument<CompensationToken>(expression);
                    this.internalConfirm = confirm;
                }
                return this.internalConfirm;
            }
        }

        public InArgument<CompensationToken> Target { get; set; }
    }
}

