namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    public sealed class CorrelationToken : DependencyObject, IPropertyValueProvider
    {
        internal static readonly DependencyProperty InitializedProperty = DependencyProperty.Register("Initialized", typeof(bool), typeof(CorrelationToken), new PropertyMetadata(false, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        internal static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(CorrelationToken), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false) }));
        internal static readonly DependencyProperty OwnerActivityNameProperty = DependencyProperty.Register("OwnerActivityName", typeof(string), typeof(CorrelationToken), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new TypeConverterAttribute(typeof(PropertyValueProviderTypeConverter)) }));
        internal static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(ICollection<CorrelationProperty>), typeof(CorrelationToken), new PropertyMetadata(new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        internal static readonly DependencyProperty SubscriptionsProperty = DependencyProperty.Register("Subscriptions", typeof(IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>), typeof(CorrelationToken));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CorrelationToken()
        {
        }

        public CorrelationToken(string name)
        {
            this.Name = name;
        }

        private static IEnumerable GetEnclosingCompositeActivities(Activity startActivity)
        {
            Activity iteratorVariable0 = null;
            Stack<Activity> iteratorVariable1 = new Stack<Activity>();
            iteratorVariable1.Push(startActivity);
            while ((iteratorVariable0 = iteratorVariable1.Pop()) != null)
            {
                if (typeof(CompositeActivity).IsAssignableFrom(iteratorVariable0.GetType()) && iteratorVariable0.Enabled)
                {
                    yield return iteratorVariable0;
                }
                iteratorVariable1.Push(iteratorVariable0.Parent);
            }
        }

        public void Initialize(Activity activity, ICollection<CorrelationProperty> propertyValues)
        {
            if (this.Initialized)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CorrelationAlreadyInitialized, new object[] { this.Name }));
            }
            base.SetValue(PropertiesProperty, propertyValues);
            CorrelationTokenEventArgs e = new CorrelationTokenEventArgs(this, true);
            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> list = base.GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (list != null)
            {
                foreach (ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> info in list)
                {
                    info.InvokeDelegate(ContextActivityUtils.ContextActivity(activity), e, true, false);
                }
            }
            base.SetValue(InitializedProperty, true);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "CorrelationToken initialized for {0} owner activity {1} ", new object[] { this.Name, this.OwnerActivityName });
        }

        public void SubscribeForCorrelationTokenInitializedEvent(Activity activity, IActivityEventListener<CorrelationTokenEventArgs> dataChangeListener)
        {
            if (dataChangeListener == null)
            {
                throw new ArgumentNullException("dataChangeListener");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> item = new ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>(dataChangeListener, ContextActivityUtils.ContextActivity(activity), true);
            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> list = base.GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (list == null)
            {
                list = new List<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>();
                base.SetValue(SubscriptionsProperty, list);
            }
            list.Add(item);
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection strings = new StringCollection();
            if (string.Equals(context.PropertyDescriptor.Name, "OwnerActivityName", StringComparison.Ordinal))
            {
                ISelectionService service = context.GetService(typeof(ISelectionService)) as ISelectionService;
                if (((service == null) || (service.SelectionCount != 1)) || !(service.PrimarySelection is Activity))
                {
                    return strings;
                }
                Activity primarySelection = service.PrimarySelection as Activity;
                foreach (Activity activity2 in GetEnclosingCompositeActivities(primarySelection))
                {
                    string qualifiedName = activity2.QualifiedName;
                    if (!strings.Contains(qualifiedName))
                    {
                        strings.Add(qualifiedName);
                    }
                }
            }
            return strings;
        }

        internal void Uninitialize(Activity activity)
        {
            base.SetValue(PropertiesProperty, null);
            CorrelationTokenEventArgs e = new CorrelationTokenEventArgs(this, false);
            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> list = base.GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (list != null)
            {
                ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>[] array = new ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>[list.Count];
                list.CopyTo(array, 0);
                foreach (ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> info in array)
                {
                    info.InvokeDelegate(ContextActivityUtils.ContextActivity(activity), e, true, false);
                }
            }
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "CorrelationToken Uninitialized for {0} owner activity {1}", new object[] { this.Name, this.OwnerActivityName });
        }

        public void UnsubscribeFromCorrelationTokenInitializedEvent(Activity activity, IActivityEventListener<CorrelationTokenEventArgs> dataChangeListener)
        {
            if (dataChangeListener == null)
            {
                throw new ArgumentNullException("dataChangeListener");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            ActivityExecutorDelegateInfo<CorrelationTokenEventArgs> item = new ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>(dataChangeListener, ContextActivityUtils.ContextActivity(activity), true);
            IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>> list = base.GetValue(SubscriptionsProperty) as IList<ActivityExecutorDelegateInfo<CorrelationTokenEventArgs>>;
            if (list != null)
            {
                list.Remove(item);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool Initialized
        {
            get
            {
                return (bool) base.GetValue(InitializedProperty);
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return (string) base.GetValue(NameProperty);
            }
            set
            {
                base.SetValue(NameProperty, value);
            }
        }

        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        public string OwnerActivityName
        {
            get
            {
                return (string) base.GetValue(OwnerActivityNameProperty);
            }
            set
            {
                base.SetValue(OwnerActivityNameProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ICollection<CorrelationProperty> Properties
        {
            get
            {
                return (base.GetValue(PropertiesProperty) as ICollection<CorrelationProperty>);
            }
        }

    }
}

