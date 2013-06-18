namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class ArgumentValue<T> : CodeActivity<T>
    {
        private RuntimeArgument targetArgument;

        public ArgumentValue()
        {
        }

        public ArgumentValue(string argumentName)
        {
            this.ArgumentName = argumentName;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            this.targetArgument = null;
            if (string.IsNullOrEmpty(this.ArgumentName))
            {
                metadata.AddValidationError(System.Activities.SR.ArgumentNameRequired);
            }
            else
            {
                this.targetArgument = ActivityUtilities.FindArgument(this.ArgumentName, this);
                if (this.targetArgument == null)
                {
                    metadata.AddValidationError(System.Activities.SR.ArgumentNotFound(this.ArgumentName));
                }
                else if (!TypeHelper.AreTypesCompatible(this.targetArgument.Type, typeof(T)))
                {
                    metadata.AddValidationError(System.Activities.SR.ArgumentTypeMustBeCompatible(this.ArgumentName, this.targetArgument.Type, typeof(T)));
                }
            }
        }

        protected override T Execute(CodeActivityContext context)
        {
            return base.ExecuteWithTryGetValue(context);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.ArgumentName))
            {
                return this.ArgumentName;
            }
            return base.ToString();
        }

        internal override bool TryGetValue(ActivityContext context, out T value)
        {
            if (this.targetArgument == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.UnopenedActivitiesCannotBeExecuted(base.DisplayName)));
            }
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                value = context.GetValue<T>(this.targetArgument);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
            return true;
        }

        public string ArgumentName { get; set; }
    }
}

