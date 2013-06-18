namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public sealed class Compensate : NativeActivity
    {
        private static Constraint compensateWithNoTarget = CompensateWithNoTarget();
        private Variable<CompensationToken> currentCompensationToken = new Variable<CompensationToken>();
        private System.Activities.Statements.DefaultCompensation defaultCompensation;
        private System.Activities.Statements.InternalCompensate internalCompensate;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.currentCompensationToken });
            metadata.SetImplementationChildrenCollection(new Collection<Activity> { this.DefaultCompensation, this.InternalCompensate });
        }

        protected override void Cancel(NativeActivityContext context)
        {
        }

        private static Constraint CompensateWithNoTarget()
        {
            DelegateInArgument<Compensate> element = new DelegateInArgument<Compensate> {
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
            Constraint<Compensate> constraint = new Constraint<Compensate>();
            ActivityAction<Compensate, ValidationContext> action = new ActivityAction<Compensate, ValidationContext> {
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
                Message = new InArgument<string>(System.Activities.SR.CompensateWithNoTargetConstraint)
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
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CompensateWithoutCompensableActivity(base.DisplayName)));
            }
            if (this.Target.IsEmpty)
            {
                CompensationToken token = (CompensationToken) context.Properties.Find("System.Compensation.CompensationToken");
                CompensationTokenData data = (token == null) ? null : extension.Get(token.CompensationId);
                if ((data == null) || !data.IsTokenValidInSecondaryRoot)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidCompensateActivityUsage(base.DisplayName)));
                }
                this.currentCompensationToken.Set(context, token);
                if (data.ExecutionTracker.Count > 0)
                {
                    context.ScheduleActivity(this.DefaultCompensation);
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
                if (!token2.CompensateCalled)
                {
                    if ((data2 == null) || (data2.CompensationState != CompensationState.Completed))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CompensableActivityAlreadyConfirmedOrCompensated));
                    }
                    data2.CompensationState = CompensationState.Compensating;
                    token2.CompensateCalled = true;
                    context.ScheduleActivity(this.InternalCompensate);
                }
            }
        }

        internal override IList<Constraint> InternalGetConstraints()
        {
            return new List<Constraint>(1) { compensateWithNoTarget };
        }

        private System.Activities.Statements.DefaultCompensation DefaultCompensation
        {
            get
            {
                if (this.defaultCompensation == null)
                {
                    System.Activities.Statements.DefaultCompensation compensation = new System.Activities.Statements.DefaultCompensation {
                        Target = new InArgument<CompensationToken>(this.currentCompensationToken)
                    };
                    this.defaultCompensation = compensation;
                }
                return this.defaultCompensation;
            }
        }

        private System.Activities.Statements.InternalCompensate InternalCompensate
        {
            get
            {
                if (this.internalCompensate == null)
                {
                    System.Activities.Statements.InternalCompensate compensate = new System.Activities.Statements.InternalCompensate();
                    ArgumentValue<CompensationToken> expression = new ArgumentValue<CompensationToken> {
                        ArgumentName = "Target"
                    };
                    compensate.Target = new InArgument<CompensationToken>(expression);
                    this.internalCompensate = compensate;
                }
                return this.internalCompensate;
            }
        }

        [DefaultValue((string) null)]
        public InArgument<CompensationToken> Target { get; set; }
    }
}

