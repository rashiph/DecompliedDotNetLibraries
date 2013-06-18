namespace System.Activities.Validation
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class Constraint<T> : Constraint
    {
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (this.Body != null)
            {
                metadata.SetDelegatesCollection(new Collection<ActivityDelegate> { this.Body });
            }
        }

        protected override void OnExecute(NativeActivityContext context, object objectToValidate, ValidationContext objectToValidateContext)
        {
            if (this.Body != null)
            {
                context.ScheduleAction<T, ValidationContext>(this.Body, (T) objectToValidate, objectToValidateContext, null, null);
            }
        }

        public ActivityAction<T, ValidationContext> Body { get; set; }
    }
}

