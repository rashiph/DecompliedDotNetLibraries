namespace System.Activities
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Runtime;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    public sealed class Variable<T> : Variable
    {
        private Activity<T> defaultExpression;

        public Variable()
        {
            base.IsHandle = ActivityUtilities.IsHandle(typeof(T));
        }

        public Variable(Expression<Func<ActivityContext, T>> defaultExpression) : this()
        {
            if (defaultExpression != null)
            {
                this.Default = new LambdaValue<T>(defaultExpression);
            }
        }

        public Variable(string name) : this()
        {
            if (!string.IsNullOrEmpty(name))
            {
                base.Name = name;
            }
        }

        public Variable(string name, Expression<Func<ActivityContext, T>> defaultExpression) : this(defaultExpression)
        {
            if (!string.IsNullOrEmpty(name))
            {
                base.Name = name;
            }
        }

        public Variable(string name, T defaultValue) : this(name)
        {
            this.Default = new Literal<T>(defaultValue);
        }

        internal override Location CreateLocation()
        {
            return new VariableLocation<T>(base.Modifiers, base.IsHandle);
        }

        public T Get(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            return context.GetValue<T>((LocationReference) this);
        }

        public Location<T> GetLocation(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            return context.GetLocation<T>(this);
        }

        public void Set(ActivityContext context, T value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.SetValue<T>((LocationReference) this, value);
        }

        internal override bool TryPopulateLocation(ActivityExecutor executor, ActivityContext activityContext)
        {
            T local;
            bool flag = true;
            VariableLocation<T> location = new VariableLocation<T>(base.Modifiers, base.IsHandle);
            if (base.IsHandle)
            {
                activityContext.Environment.DeclareHandle(this, location, activityContext.CurrentInstance);
                using (HandleInitializationContext context = new HandleInitializationContext(executor, activityContext.CurrentInstance))
                {
                    location.SetInitialValue((T) context.CreateAndInitializeHandle(typeof(T)));
                    return flag;
                }
            }
            activityContext.Environment.Declare(this, location, activityContext.CurrentInstance);
            if (this.Default == null)
            {
                return flag;
            }
            if (this.Default.TryGetValue(activityContext, out local))
            {
                location.SetInitialValue(local);
                return flag;
            }
            location.SetIsWaitingOnDefaultValue();
            return false;
        }

        [DefaultValue((string) null)]
        public Activity<T> Default
        {
            get
            {
                return this.defaultExpression;
            }
            set
            {
                base.ThrowIfHandle();
                this.defaultExpression = value;
            }
        }

        internal override ActivityWithResult DefaultCore
        {
            get
            {
                return this.Default;
            }
            set
            {
                base.ThrowIfHandle();
                if (value == null)
                {
                    this.defaultExpression = null;
                }
                else if (value is Activity<T>)
                {
                    this.defaultExpression = (Activity<T>) value;
                }
                else
                {
                    this.defaultExpression = new ActivityWithResultWrapper<T>(value);
                }
            }
        }

        protected override Type TypeCore
        {
            get
            {
                return typeof(T);
            }
        }

        [DataContract]
        private sealed class VariableLocation : Location<T>, INotifyPropertyChanged
        {
            [DataMember(EmitDefaultValue=false)]
            private bool isHandle;
            [DataMember(EmitDefaultValue=false)]
            private bool isWaitingOnDefaultValue;
            [DataMember(EmitDefaultValue=false)]
            private VariableModifiers modifiers;
            private PropertyChangedEventHandler propertyChanged;
            private NotifyCollectionChangedEventHandler valueCollectionChanged;
            private PropertyChangedEventHandler valuePropertyChanged;

            public event PropertyChangedEventHandler PropertyChanged
            {
                add
                {
                    this.propertyChanged = (PropertyChangedEventHandler) Delegate.Combine(this.propertyChanged, value);
                    INotifyPropertyChanged changed = this.Value as INotifyPropertyChanged;
                    if (changed != null)
                    {
                        changed.PropertyChanged += this.ValuePropertyChangedHandler;
                    }
                    INotifyCollectionChanged changed2 = this.Value as INotifyCollectionChanged;
                    if (changed2 != null)
                    {
                        changed2.CollectionChanged += this.ValueCollectionChangedHandler;
                    }
                }
                remove
                {
                    this.propertyChanged = (PropertyChangedEventHandler) Delegate.Remove(this.propertyChanged, value);
                    INotifyPropertyChanged changed = this.Value as INotifyPropertyChanged;
                    if (changed != null)
                    {
                        changed.PropertyChanged -= this.ValuePropertyChangedHandler;
                    }
                    INotifyCollectionChanged changed2 = this.Value as INotifyCollectionChanged;
                    if (changed2 != null)
                    {
                        changed2.CollectionChanged -= this.ValueCollectionChangedHandler;
                    }
                }
            }

            public VariableLocation(VariableModifiers modifiers, bool isHandle)
            {
                this.modifiers = modifiers;
                this.isHandle = isHandle;
            }

            private void NotifyPropertyChanged()
            {
                PropertyChangedEventHandler propertyChanged = this.propertyChanged;
                if (propertyChanged != null)
                {
                    propertyChanged(this, ActivityUtilities.ValuePropertyChangedEventArgs);
                }
            }

            private void NotifyValueCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                this.NotifyPropertyChanged();
            }

            private void NotifyValuePropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                PropertyChangedEventHandler propertyChanged = this.propertyChanged;
                if (propertyChanged != null)
                {
                    propertyChanged(this, e);
                }
            }

            internal void SetInitialValue(T value)
            {
                base.Value = value;
            }

            internal void SetIsWaitingOnDefaultValue()
            {
                if (VariableModifiersHelper.IsReadOnly(this.modifiers))
                {
                    this.isWaitingOnDefaultValue = true;
                }
            }

            internal override bool CanBeMapped
            {
                get
                {
                    return VariableModifiersHelper.IsMappable(this.modifiers);
                }
            }

            public override T Value
            {
                get
                {
                    return base.Value;
                }
                set
                {
                    if (this.isHandle)
                    {
                        Handle handle = base.Value as Handle;
                        if ((handle != null) && handle.IsInitialized)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotPerformOperationOnHandle));
                        }
                        if (value != null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotPerformOperationOnHandle));
                        }
                    }
                    if (VariableModifiersHelper.IsReadOnly(this.modifiers))
                    {
                        if (!this.isWaitingOnDefaultValue)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ConstVariableCannotBeSet));
                        }
                        this.isWaitingOnDefaultValue = false;
                    }
                    base.Value = value;
                    this.NotifyPropertyChanged();
                }
            }

            private NotifyCollectionChangedEventHandler ValueCollectionChangedHandler
            {
                get
                {
                    if (this.valueCollectionChanged == null)
                    {
                        this.valueCollectionChanged = new NotifyCollectionChangedEventHandler(this.NotifyValueCollectionChanged);
                    }
                    return this.valueCollectionChanged;
                }
            }

            private PropertyChangedEventHandler ValuePropertyChangedHandler
            {
                get
                {
                    if (this.valuePropertyChanged == null)
                    {
                        this.valuePropertyChanged = new PropertyChangedEventHandler(this.NotifyValuePropertyChanged);
                    }
                    return this.valuePropertyChanged;
                }
            }
        }
    }
}

