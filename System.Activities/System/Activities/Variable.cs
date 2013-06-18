namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DebuggerDisplay("Name = {Name}, Type = {Type}")]
    public abstract class Variable : LocationReference
    {
        private int cacheId;
        private VariableModifiers modifiers;
        private string name;

        internal Variable()
        {
            base.Id = -1;
        }

        public static Variable Create(string name, Type type, VariableModifiers modifiers)
        {
            return ActivityUtilities.CreateVariable(name, type, modifiers);
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
                if (this.IsPublic || !object.ReferenceEquals(this.Owner, context.Activity))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.VariableOnlyAccessibleAtScopeOfDeclaration(context.Activity, this.Owner)));
                }
                if (!context.Environment.TryGetLocation(base.Id, out location))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.VariableDoesNotExist(this.Name)));
                }
                return location;
            }
            if (!context.Environment.TryGetLocation(base.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.VariableDoesNotExist(this.Name)));
            }
            return location;
        }

        internal bool InitializeRelationship(Activity parent, bool isPublic, ref IList<ValidationError> validationErrors)
        {
            if ((this.cacheId == parent.CacheId) && (this.Owner != null))
            {
                ValidationError data = new ValidationError(System.Activities.SR.VariableAlreadyInUseOnActivity(this.Name, parent.DisplayName, this.Owner.DisplayName), false, this.Name, parent);
                ActivityUtilities.Add<ValidationError>(ref validationErrors, data);
                return false;
            }
            this.Owner = parent;
            this.cacheId = parent.CacheId;
            this.IsPublic = isPublic;
            if (this.Default == null)
            {
                return true;
            }
            ActivityWithResult innerExpression = this.Default;
            if (innerExpression is Argument.IExpressionWrapper)
            {
                innerExpression = ((Argument.IExpressionWrapper) innerExpression).InnerExpression;
            }
            if (innerExpression.ResultType != base.Type)
            {
                ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.VariableExpressionTypeMismatch(this.Name, base.Type, innerExpression.ResultType), false, this.Name, parent));
            }
            return this.Default.InitializeRelationship(this, isPublic, ref validationErrors);
        }

        internal Location InternalGetLocation(LocationEnvironment environment)
        {
            Location location;
            if (!environment.TryGetLocation(base.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.VariableDoesNotExist(this.Name)));
            }
            return location;
        }

        public void Set(ActivityContext context, object value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.SetValue<object>((LocationReference) this, value);
        }

        internal void ThrowIfHandle()
        {
            if (this.IsHandle)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotPerformOperationOnHandle));
            }
        }

        internal void ThrowIfNotInTree()
        {
            if (!this.IsInTree)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.VariableNotOpen(this.Name, base.Type)));
            }
        }

        internal abstract bool TryPopulateLocation(ActivityExecutor executor, ActivityContext activityContext);

        [IgnoreDataMember, DefaultValue((string) null)]
        public ActivityWithResult Default
        {
            get
            {
                return this.DefaultCore;
            }
            set
            {
                this.DefaultCore = value;
            }
        }

        internal abstract ActivityWithResult DefaultCore { get; set; }

        internal bool IsHandle { get; set; }

        internal bool IsInTree
        {
            get
            {
                return (this.Owner != null);
            }
        }

        internal bool IsPublic { get; set; }

        [DefaultValue(0)]
        public VariableModifiers Modifiers
        {
            get
            {
                return this.modifiers;
            }
            set
            {
                VariableModifiersHelper.Validate(value, "value");
                this.modifiers = value;
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

