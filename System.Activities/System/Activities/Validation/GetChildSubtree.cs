namespace System.Activities.Validation
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public sealed class GetChildSubtree : CodeActivity<IEnumerable<Activity>>
    {
        protected override IEnumerable<Activity> Execute(CodeActivityContext context)
        {
            System.Activities.Validation.ValidationContext context2 = this.ValidationContext.Get(context);
            if (context2 != null)
            {
                return context2.GetChildren();
            }
            return ActivityValidationServices.EmptyChildren;
        }

        public InArgument<System.Activities.Validation.ValidationContext> ValidationContext { get; set; }
    }
}

