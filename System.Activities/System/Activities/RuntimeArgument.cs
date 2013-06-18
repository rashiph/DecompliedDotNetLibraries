namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class RuntimeArgument : LocationReference
    {
        private PropertyDescriptor bindingProperty;
        private object bindingPropertyOwner;
        private Argument boundArgument;
        private int cacheId;
        private static InternalEvaluationOrderComparer evaluationOrderComparer;
        private bool isNameHashSet;
        private string name;
        private uint nameHash;
        private List<string> overloadGroupNames;
        private Type type;

        public RuntimeArgument(string name, Type argumentType, ArgumentDirection direction) : this(name, argumentType, direction, false)
        {
        }

        public RuntimeArgument(string name, Type argumentType, ArgumentDirection direction, bool isRequired) : this(name, argumentType, direction, isRequired, null)
        {
        }

        public RuntimeArgument(string name, Type argumentType, ArgumentDirection direction, List<string> overloadGroupNames) : this(name, argumentType, direction, false, overloadGroupNames)
        {
        }

        public RuntimeArgument(string name, Type argumentType, ArgumentDirection direction, bool isRequired, List<string> overloadGroupNames)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (argumentType == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentType");
            }
            ArgumentDirectionHelper.Validate(direction, "direction");
            this.name = name;
            this.type = argumentType;
            this.Direction = direction;
            this.IsRequired = isRequired;
            this.overloadGroupNames = overloadGroupNames;
        }

        internal RuntimeArgument(string name, Type argumentType, ArgumentDirection direction, bool isRequired, List<string> overloadGroups, Argument argument) : this(name, argumentType, direction, isRequired, overloadGroups)
        {
            Argument.Bind(argument, this);
        }

        internal RuntimeArgument(string name, Type argumentType, ArgumentDirection direction, bool isRequired, List<string> overloadGroups, PropertyDescriptor bindingProperty, object propertyOwner) : this(name, argumentType, direction, isRequired, overloadGroups)
        {
            this.bindingProperty = bindingProperty;
            this.bindingPropertyOwner = propertyOwner;
        }

        private void EnsureHash()
        {
            if (!this.isNameHashSet)
            {
                this.nameHash = CRCHashCode.Calculate(base.Name);
                this.isNameHashSet = true;
            }
        }

        public object Get(ActivityContext context)
        {
            return context.GetValue<object>(this);
        }

        public T Get<T>(ActivityContext context)
        {
            return context.GetValue<T>(this);
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
                if (!object.ReferenceEquals(this.Owner, context.Activity))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CanOnlyGetOwnedArguments(context.Activity.DisplayName, base.Name, this.Owner.DisplayName)));
                }
                if (!context.Environment.TryGetLocation(base.Id, out location))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ArgumentDoesNotExistInEnvironment(base.Name)));
                }
                return location;
            }
            if (!context.Environment.TryGetLocation(base.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ArgumentDoesNotExistInEnvironment(base.Name)));
            }
            return location;
        }

        internal bool InitializeRelationship(Activity parent, ref IList<ValidationError> validationErrors)
        {
            if (this.cacheId == parent.CacheId)
            {
                if (this.Owner == parent)
                {
                    ActivityUtilities.Add<ValidationError>(ref validationErrors, this.ProcessViolation(parent, System.Activities.SR.ArgumentIsAddedMoreThanOnce(base.Name, this.Owner.DisplayName)));
                    return false;
                }
                ActivityUtilities.Add<ValidationError>(ref validationErrors, this.ProcessViolation(parent, System.Activities.SR.ArgumentAlreadyInUse(base.Name, this.Owner.DisplayName, parent.DisplayName)));
                return false;
            }
            if ((this.boundArgument != null) && (this.boundArgument.RuntimeArgument != this))
            {
                ActivityUtilities.Add<ValidationError>(ref validationErrors, this.ProcessViolation(parent, System.Activities.SR.RuntimeArgumentBindingInvalid(base.Name, this.boundArgument.RuntimeArgument.Name)));
                return false;
            }
            this.Owner = parent;
            this.cacheId = parent.CacheId;
            if (this.boundArgument != null)
            {
                this.boundArgument.Validate(parent, ref validationErrors);
                if (!this.BoundArgument.IsEmpty)
                {
                    return this.BoundArgument.Expression.InitializeRelationship(this, ref validationErrors);
                }
            }
            return true;
        }

        internal Location InternalGetLocation(LocationEnvironment environment)
        {
            Location location;
            if (!environment.TryGetLocation(base.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ArgumentDoesNotExistInEnvironment(base.Name)));
            }
            return location;
        }

        private ValidationError ProcessViolation(Activity owner, string errorMessage)
        {
            return new ValidationError(errorMessage, false, base.Name) { Source = owner, Id = owner.Id };
        }

        public void Set(ActivityContext context, object value)
        {
            context.SetValue<object>(this, value);
        }

        internal void SetupBinding(Activity owningElement, bool createEmptyBinding)
        {
            if (this.bindingProperty != null)
            {
                Argument argument = (Argument) this.bindingProperty.GetValue(this.bindingPropertyOwner);
                if (argument == null)
                {
                    argument = (Argument) Activator.CreateInstance(this.bindingProperty.PropertyType);
                    argument.WasDesignTimeNull = true;
                    if (createEmptyBinding && !this.bindingProperty.IsReadOnly)
                    {
                        this.bindingProperty.SetValue(this.bindingPropertyOwner, argument);
                    }
                }
                Argument.Bind(argument, this);
            }
            else if (!this.IsBound)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(owningElement);
                PropertyDescriptor descriptor = null;
                for (int i = 0; i < properties.Count; i++)
                {
                    ArgumentDirection direction;
                    Type type;
                    PropertyDescriptor descriptor2 = properties[i];
                    if ((((descriptor2.Name == base.Name) && descriptor2.PropertyType.IsGenericType) && (ActivityUtilities.TryGetArgumentDirectionAndType(descriptor2.PropertyType, out direction, out type) && (base.Type == type))) && (this.Direction == direction))
                    {
                        descriptor = descriptor2;
                        break;
                    }
                }
                Argument argument2 = null;
                if (descriptor != null)
                {
                    argument2 = (Argument) descriptor.GetValue(owningElement);
                }
                if (argument2 == null)
                {
                    if (descriptor != null)
                    {
                        if (descriptor.PropertyType.IsGenericType)
                        {
                            argument2 = (Argument) Activator.CreateInstance(descriptor.PropertyType);
                        }
                        else
                        {
                            argument2 = ActivityUtilities.CreateArgument(base.Type, this.Direction);
                        }
                    }
                    else
                    {
                        argument2 = ActivityUtilities.CreateArgument(base.Type, this.Direction);
                    }
                    argument2.WasDesignTimeNull = true;
                    if (((descriptor != null) && createEmptyBinding) && !descriptor.IsReadOnly)
                    {
                        descriptor.SetValue(owningElement, argument2);
                    }
                }
                Argument.Bind(argument2, this);
            }
        }

        internal void ThrowIfNotInTree()
        {
            if (!this.IsInTree)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RuntimeArgumentNotOpen(base.Name)));
            }
        }

        internal bool TryPopulateValue(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance targetActivityInstance, ActivityContext resolutionContext, object argumentValueOverride, Location resultLocation, bool skipFastPath)
        {
            if (argumentValueOverride != null)
            {
                Location location = this.boundArgument.CreateDefaultLocation();
                targetEnvironment.Declare(this, location, targetActivityInstance);
                location.Value = argumentValueOverride;
                return true;
            }
            if (!this.boundArgument.IsEmpty)
            {
                if (skipFastPath)
                {
                    this.BoundArgument.Declare(targetEnvironment, targetActivityInstance);
                    return false;
                }
                resolutionContext.Activity = this.boundArgument.Expression;
                return this.boundArgument.TryPopulateValue(targetEnvironment, targetActivityInstance, resolutionContext);
            }
            if ((resultLocation != null) && this.IsResult)
            {
                targetEnvironment.Declare(this, resultLocation, targetActivityInstance);
                return true;
            }
            Location location2 = this.boundArgument.CreateDefaultLocation();
            targetEnvironment.Declare(this, location2, targetActivityInstance);
            return true;
        }

        internal Argument BoundArgument
        {
            get
            {
                return this.boundArgument;
            }
            set
            {
                this.boundArgument = value;
            }
        }

        public ArgumentDirection Direction { get; private set; }

        internal static IComparer<RuntimeArgument> EvaluationOrderComparer
        {
            get
            {
                if (evaluationOrderComparer == null)
                {
                    evaluationOrderComparer = new InternalEvaluationOrderComparer();
                }
                return evaluationOrderComparer;
            }
        }

        internal bool IsBound
        {
            get
            {
                return (this.boundArgument != null);
            }
        }

        internal bool IsEvaluationOrderSpecified
        {
            get
            {
                return (this.IsBound && (this.BoundArgument.EvaluationOrder != Argument.UnspecifiedEvaluationOrder));
            }
        }

        internal bool IsInTree
        {
            get
            {
                return (this.Owner != null);
            }
        }

        public bool IsRequired { get; private set; }

        internal bool IsResult
        {
            get
            {
                return this.Owner.IsResultArgument(this);
            }
        }

        protected override string NameCore
        {
            get
            {
                return this.name;
            }
        }

        public ReadOnlyCollection<string> OverloadGroupNames
        {
            get
            {
                if (this.overloadGroupNames == null)
                {
                    this.overloadGroupNames = new List<string>(0);
                }
                return new ReadOnlyCollection<string>(this.overloadGroupNames);
            }
        }

        internal Activity Owner { get; private set; }

        protected override Type TypeCore
        {
            get
            {
                return this.type;
            }
        }

        internal static class CRCHashCode
        {
            private const uint polynomial = 0x82f63b78;

            public static unsafe uint Calculate(string s)
            {
                uint maxValue = uint.MaxValue;
                int num2 = s.Length * 2;
                fixed (char* str = ((char*) s))
                {
                    char* chPtr = str;
                    byte* numPtr = (byte*) chPtr;
                    for (int i = 0; i < num2; i++)
                    {
                        maxValue ^= numPtr[i];
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                        maxValue = ((maxValue & 1) * 0x82f63b78) ^ (maxValue >> 1);
                    }
                }
                return ~maxValue;
            }
        }

        private class InternalEvaluationOrderComparer : IComparer<RuntimeArgument>
        {
            public int Compare(RuntimeArgument x, RuntimeArgument y)
            {
                if (!x.IsEvaluationOrderSpecified)
                {
                    if (y.IsEvaluationOrderSpecified)
                    {
                        return -1;
                    }
                    return this.CompareNameHashes(x, y);
                }
                if (y.IsEvaluationOrderSpecified)
                {
                    return x.BoundArgument.EvaluationOrder.CompareTo(y.BoundArgument.EvaluationOrder);
                }
                return 1;
            }

            private int CompareNameHashes(RuntimeArgument x, RuntimeArgument y)
            {
                x.EnsureHash();
                y.EnsureHash();
                if (x.nameHash != y.nameHash)
                {
                    return x.nameHash.CompareTo(y.nameHash);
                }
                return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
            }
        }
    }
}

