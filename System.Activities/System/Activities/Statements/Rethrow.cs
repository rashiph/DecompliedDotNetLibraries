namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class Rethrow : NativeActivity
    {
        public Rethrow()
        {
            DelegateInArgument<Rethrow> argument = new DelegateInArgument<Rethrow> {
                Name = "constraintArg"
            };
            DelegateInArgument<ValidationContext> argument2 = new DelegateInArgument<ValidationContext> {
                Name = "validationContext"
            };
            Constraint<Rethrow> item = new Constraint<Rethrow>();
            ActivityAction<Rethrow, ValidationContext> action = new ActivityAction<Rethrow, ValidationContext> {
                Argument1 = argument,
                Argument2 = argument2
            };
            RethrowBuildConstraint constraint2 = new RethrowBuildConstraint();
            GetParentChain chain = new GetParentChain {
                ValidationContext = argument2
            };
            constraint2.ParentChain = chain;
            constraint2.RethrowActivity = argument;
            action.Handler = constraint2;
            item.Body = action;
            base.Constraints.Add(item);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
        }

        protected override void Execute(NativeActivityContext context)
        {
            FaultContext context2 = context.Properties.Find("{35ABC8C3-9AF1-4426-8293-A6DDBB6ED91D}") as FaultContext;
            if (context2 == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.FaultContextNotFound(base.DisplayName)));
            }
            context.RethrowException(context2);
        }

        private class RethrowBuildConstraint : NativeActivity<bool>
        {
            protected override void CacheMetadata(NativeActivityMetadata metadata)
            {
                RuntimeArgument argument = new RuntimeArgument("ParentChain", typeof(IEnumerable<Activity>), ArgumentDirection.In, true);
                metadata.Bind(this.ParentChain, argument);
                metadata.AddArgument(argument);
                RuntimeArgument argument2 = new RuntimeArgument("RethrowActivity", typeof(Rethrow), ArgumentDirection.In, true);
                metadata.Bind(this.RethrowActivity, argument2);
                metadata.AddArgument(argument2);
            }

            protected override void Execute(NativeActivityContext context)
            {
                IEnumerable<Activity> enumerable = this.ParentChain.Get(context);
                Rethrow rethrow = this.RethrowActivity.Get(context);
                Activity item = rethrow;
                bool flag = false;
                foreach (Activity activity2 in enumerable)
                {
                    if (activity2.ImplementationChildren.Contains(item))
                    {
                        flag = true;
                    }
                    TryCatch @catch = activity2 as TryCatch;
                    if ((@catch != null) && (item != null))
                    {
                        foreach (Catch catch2 in @catch.Catches)
                        {
                            ActivityDelegate action = catch2.GetAction();
                            if ((action != null) && (action.Handler == item))
                            {
                                if (flag)
                                {
                                    Constraint.AddValidationError(context, new ValidationError(System.Activities.SR.RethrowMustBeAPublicChild(rethrow.DisplayName), rethrow));
                                }
                                return;
                            }
                        }
                    }
                    item = activity2;
                }
                Constraint.AddValidationError(context, new ValidationError(System.Activities.SR.RethrowNotInATryCatch(rethrow.DisplayName), rethrow));
            }

            [DefaultValue((string) null), RequiredArgument]
            public InArgument<IEnumerable<Activity>> ParentChain { get; set; }

            [RequiredArgument, DefaultValue((string) null)]
            public InArgument<Rethrow> RethrowActivity { get; set; }
        }
    }
}

