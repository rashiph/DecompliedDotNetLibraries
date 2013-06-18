namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class ArgumentReference<T> : CodeActivity<Location<T>>
    {
        private RuntimeArgument targetArgument;

        public ArgumentReference()
        {
        }

        public ArgumentReference(string argumentName)
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
                else if (this.targetArgument.Type != typeof(T))
                {
                    metadata.AddValidationError(System.Activities.SR.ArgumentTypeMustBeCompatible(this.ArgumentName, this.targetArgument.Type, typeof(T)));
                }
            }
        }

        protected override Location<T> Execute(CodeActivityContext context)
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

        internal override bool TryGetValue(ActivityContext context, out Location<T> value)
        {
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                value = context.GetLocation<T>(this.targetArgument);
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

