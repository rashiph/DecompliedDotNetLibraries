namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class DelegateArgument : LocationReference
    {
        private int cacheId;
        private ArgumentDirection direction;
        private string name;
        private RuntimeDelegateArgument runtimeArgument;

        internal DelegateArgument()
        {
            base.Id = -1;
        }

        internal void Bind(RuntimeDelegateArgument runtimeArgument)
        {
            this.runtimeArgument = runtimeArgument;
        }

        internal abstract Location CreateLocation();
        public object Get(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            return context.GetValue<object>((LocationReference) this);
        }

        public override Location GetLocation(ActivityContext context)
        {
            Location location;
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            this.ThrowIfNotInTree();
            if (!context.AllowChainedEnvironmentAccess)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.DelegateArgumentDoesNotExist(this.runtimeArgument.Name)));
            }
            if (!context.Environment.TryGetLocation(base.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.DelegateArgumentDoesNotExist(this.runtimeArgument.Name)));
            }
            return location;
        }

        internal bool InitializeRelationship(Activity parent, ref IList<ValidationError> validationErrors)
        {
            if (this.cacheId == parent.CacheId)
            {
                ValidationError data = new ValidationError(System.Activities.SR.DelegateArgumentAlreadyInUseOnActivity(this.Name, parent.DisplayName, this.Owner.DisplayName), this.Owner);
                ActivityUtilities.Add<ValidationError>(ref validationErrors, data);
                return false;
            }
            this.Owner = parent;
            this.cacheId = parent.CacheId;
            return true;
        }

        internal Location InternalGetLocation(LocationEnvironment environment)
        {
            Location location;
            if (!environment.TryGetLocation(base.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.DelegateArgumentDoesNotExist(this.runtimeArgument.Name)));
            }
            return location;
        }

        internal void ThrowIfNotInTree()
        {
            if (!this.IsInTree)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.DelegateArgumentMustBeReferenced(this.Name)));
            }
        }

        public ArgumentDirection Direction
        {
            get
            {
                return this.direction;
            }
            internal set
            {
                this.direction = value;
            }
        }

        internal bool IsInTree
        {
            get
            {
                return (this.Owner != null);
            }
        }

        [DefaultValue((string) null)]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        protected override string NameCore
        {
            get
            {
                return this.name;
            }
        }

        internal Activity Owner { get; private set; }
    }
}

