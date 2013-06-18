namespace System.Activities
{
    using System;

    internal class InlinedLocationReference : LocationReference
    {
        private LocationReference innerReference;
        private Activity validAccessor;

        public InlinedLocationReference(LocationReference innerReference, Activity validAccessor)
        {
            this.innerReference = innerReference;
            this.validAccessor = validAccessor;
        }

        public override Location GetLocation(ActivityContext context)
        {
            Location location;
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.ThrowIfDisposed();
            if (!object.ReferenceEquals(context.Activity, this.validAccessor))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InlinedLocationReferenceOnlyAccessibleByOwner(context.Activity, this.validAccessor)));
            }
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                location = this.innerReference.GetLocation(context);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
            return location;
        }

        protected override string NameCore
        {
            get
            {
                return this.innerReference.Name;
            }
        }

        protected override Type TypeCore
        {
            get
            {
                return this.innerReference.Type;
            }
        }
    }
}

