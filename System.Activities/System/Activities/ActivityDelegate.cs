namespace System.Activities
{
    using System;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Handler")]
    public abstract class ActivityDelegate
    {
        internal static string Argument10Name = "Argument10";
        internal static string Argument11Name = "Argument11";
        internal static string Argument12Name = "Argument12";
        internal static string Argument13Name = "Argument13";
        internal static string Argument14Name = "Argument14";
        internal static string Argument15Name = "Argument15";
        internal static string Argument16Name = "Argument16";
        internal static string Argument1Name = "Argument1";
        internal static string Argument2Name = "Argument2";
        internal static string Argument3Name = "Argument3";
        internal static string Argument4Name = "Argument4";
        internal static string Argument5Name = "Argument5";
        internal static string Argument6Name = "Argument6";
        internal static string Argument7Name = "Argument7";
        internal static string Argument8Name = "Argument8";
        internal static string Argument9Name = "Argument9";
        internal static string ArgumentName = "Argument";
        private int cacheId;
        private IList<RuntimeDelegateArgument> delegateParameters;
        private string displayName;
        private bool isDisplayNameSet;
        private Activity owner;
        private ActivityCollectionType parentCollectionType;
        internal static string ResultArgumentName = "Result";

        protected ActivityDelegate()
        {
        }

        internal bool CanBeScheduledBy(Activity parent)
        {
            if (object.ReferenceEquals(parent, this.owner))
            {
                return (this.parentCollectionType != ActivityCollectionType.Imports);
            }
            if (!parent.Delegates.Contains(this))
            {
                return parent.ImplementationDelegates.Contains(this);
            }
            return true;
        }

        protected internal virtual DelegateOutArgument GetResultArgument()
        {
            return null;
        }

        internal bool InitializeRelationship(Activity parent, ActivityCollectionType collectionType, ref IList<ValidationError> validationErrors)
        {
            if (this.cacheId == parent.CacheId)
            {
                Activity owner = parent.MemberOf.Owner;
                if (owner == null)
                {
                    Activity activity2 = this.Handler;
                    if (activity2 == null)
                    {
                        ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ActivityDelegateCannotBeReferencedWithoutTargetNoHandler(parent.DisplayName, this.owner.DisplayName), false, parent));
                    }
                    else
                    {
                        ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ActivityDelegateCannotBeReferencedWithoutTarget(activity2.DisplayName, parent.DisplayName, this.owner.DisplayName), false, parent));
                    }
                    return false;
                }
                if (owner.Delegates.Contains(this) || owner.ImportedDelegates.Contains(this))
                {
                    return true;
                }
                Activity handler = this.Handler;
                if (handler == null)
                {
                    ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ActivityDelegateCannotBeReferencedNoHandler(parent.DisplayName, owner.DisplayName, this.owner.DisplayName), false, parent));
                }
                else
                {
                    ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ActivityDelegateCannotBeReferenced(handler.DisplayName, parent.DisplayName, owner.DisplayName, this.owner.DisplayName), false, parent));
                }
                return false;
            }
            this.owner = parent;
            this.cacheId = parent.CacheId;
            this.parentCollectionType = collectionType;
            this.InternalCacheMetadata();
            LocationReferenceEnvironment implementationEnvironment = null;
            if (collectionType == ActivityCollectionType.Implementation)
            {
                implementationEnvironment = parent.ImplementationEnvironment;
            }
            else
            {
                implementationEnvironment = parent.PublicEnvironment;
            }
            if (this.RuntimeDelegateArguments.Count > 0)
            {
                ActivityLocationReferenceEnvironment environment2 = new ActivityLocationReferenceEnvironment(implementationEnvironment);
                implementationEnvironment = environment2;
                for (int i = 0; i < this.RuntimeDelegateArguments.Count; i++)
                {
                    RuntimeDelegateArgument argument = this.RuntimeDelegateArguments[i];
                    DelegateArgument boundArgument = argument.BoundArgument;
                    if (boundArgument != null)
                    {
                        if (boundArgument.Direction != argument.Direction)
                        {
                            ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.RuntimeDelegateArgumentDirectionIncorrect, parent));
                        }
                        if (boundArgument.Type != argument.Type)
                        {
                            ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.RuntimeDelegateArgumentTypeIncorrect, parent));
                        }
                        environment2.Declare(boundArgument, this.owner, ref validationErrors);
                    }
                }
            }
            this.Environment = implementationEnvironment;
            if (this.Handler != null)
            {
                return this.Handler.InitializeRelationship(this, collectionType, ref validationErrors);
            }
            return true;
        }

        internal void InternalCacheMetadata()
        {
            this.delegateParameters = new ReadOnlyCollection<RuntimeDelegateArgument>(this.InternalGetRuntimeDelegateArguments());
        }

        internal virtual IList<RuntimeDelegateArgument> InternalGetRuntimeDelegateArguments()
        {
            IList<RuntimeDelegateArgument> runtimeDelegateArguments = new List<RuntimeDelegateArgument>();
            this.OnGetRuntimeDelegateArguments(runtimeDelegateArguments);
            return runtimeDelegateArguments;
        }

        protected virtual void OnGetRuntimeDelegateArguments(IList<RuntimeDelegateArgument> runtimeDelegateArguments)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                ArgumentDirection direction;
                Type type;
                if (ActivityUtilities.TryGetDelegateArgumentDirectionAndType(descriptor.PropertyType, out direction, out type))
                {
                    runtimeDelegateArguments.Add(new RuntimeDelegateArgument(descriptor.Name, type, direction, (DelegateArgument) descriptor.GetValue(this)));
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeDisplayName()
        {
            return this.isDisplayNameSet;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.displayName))
                {
                    this.displayName = base.GetType().Name;
                }
                return this.displayName;
            }
            set
            {
                this.isDisplayNameSet = true;
                this.displayName = value;
            }
        }

        internal LocationReferenceEnvironment Environment { get; set; }

        [DefaultValue((string) null)]
        public Activity Handler { get; set; }

        internal Activity Owner
        {
            get
            {
                return this.owner;
            }
        }

        internal IList<RuntimeDelegateArgument> RuntimeDelegateArguments
        {
            get
            {
                if (this.delegateParameters != null)
                {
                    return this.delegateParameters;
                }
                return new ReadOnlyCollection<RuntimeDelegateArgument>(this.InternalGetRuntimeDelegateArguments());
            }
        }
    }
}

