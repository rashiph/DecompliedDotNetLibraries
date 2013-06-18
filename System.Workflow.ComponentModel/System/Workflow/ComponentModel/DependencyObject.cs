namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Threading;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer)), DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    public abstract class DependencyObject : IComponent, IDependencyObjectAccessor, IDisposable
    {
        private IDictionary<DependencyProperty, object> dependencyPropertyValues;
        [NonSerialized]
        private IDictionary<DependencyProperty, object> metaProperties = new Dictionary<DependencyProperty, object>();
        private static DependencyProperty ParentDependencyObjectProperty = DependencyProperty.Register("ParentDependencyObject", typeof(DependencyObject), typeof(DependencyObject), new PropertyMetadata(null, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private static DependencyProperty ReadonlyProperty = DependencyProperty.Register("Readonly", typeof(bool), typeof(DependencyObject), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        [NonSerialized]
        private bool readonlyPropertyValue;
        private static DependencyProperty SiteProperty = DependencyProperty.Register("Site", typeof(ISite), typeof(DependencyObject), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private static DependencyProperty UserDataProperty = DependencyProperty.Register("UserData", typeof(IDictionary), typeof(DependencyObject), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        private event EventHandler disposed;

        event EventHandler IComponent.Disposed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] add
            {
                this.disposed += value;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] remove
            {
                this.disposed -= value;
            }
        }

        protected DependencyObject()
        {
            this.SetReadOnlyPropertyValue(ReadonlyProperty, false);
            this.readonlyPropertyValue = false;
            this.SetReadOnlyPropertyValue(UserDataProperty, Hashtable.Synchronized(new Hashtable()));
        }

        public void AddHandler(DependencyProperty dependencyEvent, object value)
        {
            if (dependencyEvent == null)
            {
                throw new ArgumentNullException("dependencyEvent");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value is ActivityBind)
            {
                throw new ArgumentException(SR.GetString("Error_DPSetValueBind"), "value");
            }
            if ((dependencyEvent.DefaultMetadata != null) && dependencyEvent.DefaultMetadata.IsMetaProperty)
            {
                throw new ArgumentException(SR.GetString("Error_DPAddHandlerMetaProperty"), "dependencyEvent");
            }
            if (!dependencyEvent.IsEvent)
            {
                throw new ArgumentException(SR.GetString("Error_DPAddHandlerNonEvent"), "dependencyEvent");
            }
            if (dependencyEvent.PropertyType == null)
            {
                throw new ArgumentException(SR.GetString("Error_DPPropertyTypeMissing"), "dependencyEvent");
            }
            if (dependencyEvent.OwnerType == null)
            {
                throw new ArgumentException(SR.GetString("Error_MissingOwnerTypeProperty"), "dependencyEvent");
            }
            if (!dependencyEvent.IsAttached && !dependencyEvent.OwnerType.IsAssignableFrom(base.GetType()))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidDependencyProperty", new object[] { base.GetType().FullName, dependencyEvent.Name, dependencyEvent.OwnerType.FullName }));
            }
            if ((value != null) && !dependencyEvent.PropertyType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException(SR.GetString("Error_DynamicPropertyTypeValueMismatch", new object[] { dependencyEvent.PropertyType.FullName, dependencyEvent.Name, value.GetType().FullName }), "value");
            }
            IDictionary<DependencyProperty, object> dependencyPropertyValues = this.DependencyPropertyValues;
            ArrayList list = null;
            if (dependencyPropertyValues.ContainsKey(dependencyEvent))
            {
                list = (ArrayList) dependencyPropertyValues[dependencyEvent];
            }
            else
            {
                list = new ArrayList();
                dependencyPropertyValues.Add(dependencyEvent, list);
            }
            list.Add(value);
            if (this.DesignMode && this.metaProperties.ContainsKey(dependencyEvent))
            {
                this.metaProperties.Remove(dependencyEvent);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if ((this.Site != null) && (this.Site.Container != null))
                {
                    this.Site.Container.Remove(this);
                }
                if (this.disposed != null)
                {
                    this.disposed(this, EventArgs.Empty);
                }
            }
        }

        ~DependencyObject()
        {
            this.Dispose(false);
        }

        internal virtual void FixUpMetaProperties(DependencyObject originalObject)
        {
            if (originalObject == null)
            {
                throw new ArgumentNullException();
            }
            this.metaProperties = originalObject.metaProperties;
            this.readonlyPropertyValue = true;
            foreach (KeyValuePair<DependencyProperty, object> pair in this.DependencyPropertyValues)
            {
                if ((pair.Key != ParentDependencyObjectProperty) && originalObject.DependencyPropertyValues.ContainsKey(pair.Key))
                {
                    object obj2 = originalObject.DependencyPropertyValues[pair.Key];
                    if (pair.Value is DependencyObject)
                    {
                        ((DependencyObject) pair.Value).FixUpMetaProperties(obj2 as DependencyObject);
                    }
                    else if (pair.Value is WorkflowParameterBindingCollection)
                    {
                        IList list = pair.Value as IList;
                        IList list2 = obj2 as IList;
                        for (int i = 0; i < list.Count; i++)
                        {
                            ((DependencyObject) list[i]).FixUpMetaProperties(list2[i] as DependencyObject);
                        }
                    }
                }
            }
        }

        public ActivityBind GetBinding(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            if (!this.metaProperties.ContainsKey(dependencyProperty))
            {
                return null;
            }
            return (this.metaProperties[dependencyProperty] as ActivityBind);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual object GetBoundValue(ActivityBind bind, Type targetType)
        {
            if (bind == null)
            {
                throw new ArgumentNullException("bind");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            object runtimeValue = bind;
            Activity activity = this.ResolveOwnerActivity();
            if (activity != null)
            {
                runtimeValue = bind.GetRuntimeValue(activity, targetType);
            }
            return runtimeValue;
        }

        internal object GetHandler(DependencyProperty dependencyEvent)
        {
            if (dependencyEvent == null)
            {
                throw new ArgumentNullException("dependencyEvent");
            }
            if (!dependencyEvent.IsEvent)
            {
                throw new ArgumentException("dependencyEvent");
            }
            IDictionary<DependencyProperty, object> dependencyPropertyValues = this.DependencyPropertyValues;
            if (dependencyPropertyValues.ContainsKey(dependencyEvent))
            {
                ArrayList list = dependencyPropertyValues[dependencyEvent] as ArrayList;
                if ((list != null) && (list.Count != 0))
                {
                    if (list.Count == 1)
                    {
                        return list[0];
                    }
                    Delegate a = list[0] as Delegate;
                    for (int i = 1; i < list.Count; i++)
                    {
                        a = Delegate.Combine(a, list[i] as Delegate);
                    }
                    return a;
                }
            }
            return null;
        }

        protected T[] GetInvocationList<T>(DependencyProperty dependencyEvent)
        {
            if (dependencyEvent == null)
            {
                throw new ArgumentNullException("dependencyEvent");
            }
            if (!dependencyEvent.IsEvent)
            {
                throw new ArgumentException(SR.GetString("Error_DPAddHandlerNonEvent"), "dependencyEvent");
            }
            IDictionary<DependencyProperty, object> dependencyPropertyValues = null;
            if (this.DependencyPropertyValues.ContainsKey(dependencyEvent))
            {
                dependencyPropertyValues = this.DependencyPropertyValues;
            }
            else
            {
                dependencyPropertyValues = this.metaProperties;
            }
            List<T> list = new List<T>();
            if (dependencyPropertyValues.ContainsKey(dependencyEvent))
            {
                if (dependencyPropertyValues[dependencyEvent] is ActivityBind)
                {
                    if (!this.DesignMode)
                    {
                        T item = default(T);
                        item = (T) this.GetBoundValue((ActivityBind) dependencyPropertyValues[dependencyEvent], typeof(T));
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
                else
                {
                    foreach (object obj2 in (ArrayList) dependencyPropertyValues[dependencyEvent])
                    {
                        if (obj2 is T)
                        {
                            list.Add((T) obj2);
                        }
                    }
                }
            }
            return list.ToArray();
        }

        public object GetValue(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            if (dependencyProperty.IsEvent)
            {
                throw new ArgumentException(SR.GetString("Error_DPGetValueHandler"), "dependencyProperty");
            }
            PropertyMetadata defaultMetadata = dependencyProperty.DefaultMetadata;
            if (defaultMetadata.GetValueOverride != null)
            {
                return defaultMetadata.GetValueOverride(this);
            }
            return this.GetValueCommon(dependencyProperty, defaultMetadata);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public object GetValueBase(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            return this.GetValueCommon(dependencyProperty, dependencyProperty.DefaultMetadata);
        }

        private object GetValueCommon(DependencyProperty dependencyProperty, PropertyMetadata metadata)
        {
            object boundValue;
            if (!this.DependencyPropertyValues.TryGetValue(dependencyProperty, out boundValue) && ((this.metaProperties == null) || !this.metaProperties.TryGetValue(dependencyProperty, out boundValue)))
            {
                return dependencyProperty.DefaultMetadata.DefaultValue;
            }
            if (((this.metaProperties != null) && !this.DesignMode) && ((boundValue is ActivityBind) && !typeof(ActivityBind).IsAssignableFrom(dependencyProperty.PropertyType)))
            {
                boundValue = this.GetBoundValue((ActivityBind) boundValue, dependencyProperty.PropertyType);
            }
            if ((boundValue == null) || (boundValue is ActivityBind))
            {
                return dependencyProperty.DefaultMetadata.DefaultValue;
            }
            if (!dependencyProperty.PropertyType.IsAssignableFrom(boundValue.GetType()))
            {
                throw new InvalidOperationException(SR.GetString("Error_DynamicPropertyTypeValueMismatch", new object[] { dependencyProperty.PropertyType.FullName, dependencyProperty.Name, boundValue.GetType().FullName }));
            }
            return boundValue;
        }

        protected virtual void InitializeProperties()
        {
        }

        public bool IsBindingSet(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            return ((!this.DependencyPropertyValues.ContainsKey(dependencyProperty) && this.metaProperties.ContainsKey(dependencyProperty)) && (this.metaProperties[dependencyProperty] is ActivityBind));
        }

        public bool MetaEquals(DependencyObject dependencyObject)
        {
            return ((dependencyObject != null) && (dependencyObject.metaProperties == this.metaProperties));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal virtual void OnInitializeActivatingInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            this.InitializeProperties();
        }

        internal virtual void OnInitializeDefinitionForRuntime()
        {
        }

        internal virtual void OnInitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
        }

        public void RemoveHandler(DependencyProperty dependencyEvent, object value)
        {
            if (dependencyEvent == null)
            {
                throw new ArgumentNullException("dependencyEvent");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value is ActivityBind)
            {
                throw new ArgumentException(SR.GetString("Error_DPRemoveHandlerBind"), "value");
            }
            if ((dependencyEvent.DefaultMetadata != null) && dependencyEvent.DefaultMetadata.IsMetaProperty)
            {
                throw new ArgumentException(SR.GetString("Error_DPAddHandlerMetaProperty"), "dependencyEvent");
            }
            if (!dependencyEvent.IsEvent)
            {
                throw new ArgumentException(SR.GetString("Error_DPAddHandlerNonEvent"), "dependencyEvent");
            }
            IDictionary<DependencyProperty, object> dependencyPropertyValues = this.DependencyPropertyValues;
            if (dependencyPropertyValues.ContainsKey(dependencyEvent))
            {
                ArrayList list = (ArrayList) dependencyPropertyValues[dependencyEvent];
                if (list.Contains(value))
                {
                    list.Remove(value);
                }
                if (list.Count == 0)
                {
                    dependencyPropertyValues.Remove(dependencyEvent);
                }
            }
        }

        public bool RemoveProperty(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            if ((dependencyProperty.DefaultMetadata != null) && dependencyProperty.DefaultMetadata.IsMetaProperty)
            {
                if (!this.DesignMode)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                return this.metaProperties.Remove(dependencyProperty);
            }
            return (this.metaProperties.Remove(dependencyProperty) | this.DependencyPropertyValues.Remove(dependencyProperty));
        }

        private Activity ResolveOwnerActivity()
        {
            DependencyObject parentDependencyObject = this;
            while ((parentDependencyObject != null) && !(parentDependencyObject is Activity))
            {
                parentDependencyObject = parentDependencyObject.ParentDependencyObject;
            }
            return (parentDependencyObject as Activity);
        }

        public void SetBinding(DependencyProperty dependencyProperty, ActivityBind bind)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            if ((dependencyProperty.DefaultMetadata != null) && dependencyProperty.DefaultMetadata.IsReadOnly)
            {
                throw new ArgumentException(SR.GetString("Error_DPReadOnly"), "dependencyProperty");
            }
            if (dependencyProperty.OwnerType == null)
            {
                throw new ArgumentException(SR.GetString("Error_MissingOwnerTypeProperty"), "dependencyProperty");
            }
            if (!dependencyProperty.IsAttached && !dependencyProperty.OwnerType.IsAssignableFrom(base.GetType()))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidDependencyProperty", new object[] { base.GetType().FullName, dependencyProperty.Name, dependencyProperty.OwnerType.FullName }));
            }
            if (!this.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            if (((dependencyProperty.DefaultMetadata != null) && dependencyProperty.DefaultMetadata.IsMetaProperty) && !typeof(ActivityBind).IsAssignableFrom(dependencyProperty.PropertyType))
            {
                throw new ArgumentException(SR.GetString("Error_DPMetaPropertyBinding"), "dependencyProperty");
            }
            if (this.metaProperties.ContainsKey(dependencyProperty))
            {
                this.metaProperties[dependencyProperty] = bind;
            }
            else
            {
                this.metaProperties.Add(dependencyProperty, bind);
            }
            if (this.DependencyPropertyValues.ContainsKey(dependencyProperty))
            {
                this.DependencyPropertyValues.Remove(dependencyProperty);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void SetBoundValue(ActivityBind bind, object value)
        {
            if (bind == null)
            {
                throw new ArgumentNullException("bind");
            }
            Activity activity = this.ResolveOwnerActivity();
            if (activity != null)
            {
                bind.SetRuntimeValue(activity, value);
            }
        }

        protected internal void SetReadOnlyPropertyValue(DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            if (!dependencyProperty.DefaultMetadata.IsReadOnly)
            {
                throw new InvalidOperationException(SR.GetString("Error_NotReadOnlyProperty", new object[] { dependencyProperty.Name, dependencyProperty.OwnerType.FullName }));
            }
            if (!dependencyProperty.IsAttached && !dependencyProperty.OwnerType.IsAssignableFrom(base.GetType()))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidDependencyProperty", new object[] { base.GetType().FullName, dependencyProperty.Name, dependencyProperty.OwnerType.FullName }));
            }
            IDictionary<DependencyProperty, object> metaProperties = null;
            if (dependencyProperty.DefaultMetadata.IsMetaProperty)
            {
                metaProperties = this.metaProperties;
            }
            else
            {
                metaProperties = this.DependencyPropertyValues;
            }
            if (metaProperties.ContainsKey(dependencyProperty))
            {
                metaProperties[dependencyProperty] = value;
            }
            else
            {
                metaProperties.Add(dependencyProperty, value);
            }
        }

        public void SetValue(DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            PropertyMetadata defaultMetadata = dependencyProperty.DefaultMetadata;
            this.SetValueCommon(dependencyProperty, value, defaultMetadata, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetValueBase(DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            PropertyMetadata defaultMetadata = dependencyProperty.DefaultMetadata;
            this.SetValueCommon(dependencyProperty, value, defaultMetadata, defaultMetadata.ShouldAlwaysCallOverride);
        }

        internal void SetValueCommon(DependencyProperty dependencyProperty, object value, PropertyMetadata metadata, bool shouldCallSetValueOverrideIfExists)
        {
            if (dependencyProperty.DefaultMetadata.IsReadOnly)
            {
                throw new ArgumentException(SR.GetString("Error_DPReadOnly"), "dependencyProperty");
            }
            if (value is ActivityBind)
            {
                throw new ArgumentException(SR.GetString("Error_DPSetValueBind"), "value");
            }
            if (dependencyProperty.IsEvent)
            {
                throw new ArgumentException(SR.GetString("Error_DPSetValueHandler"), "dependencyProperty");
            }
            if (!dependencyProperty.IsAttached && !dependencyProperty.OwnerType.IsAssignableFrom(base.GetType()))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidDependencyProperty", new object[] { base.GetType().FullName, dependencyProperty.Name, dependencyProperty.OwnerType.FullName }));
            }
            if ((!this.DesignMode && dependencyProperty.DefaultMetadata.IsMetaProperty) && (dependencyProperty != ConditionTypeConverter.DeclarativeConditionDynamicProp))
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            if ((value != null) && !dependencyProperty.PropertyType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException(SR.GetString("Error_DynamicPropertyTypeValueMismatch", new object[] { dependencyProperty.PropertyType.FullName, dependencyProperty.Name, value.GetType().FullName }), "value");
            }
            if (shouldCallSetValueOverrideIfExists && (metadata.SetValueOverride != null))
            {
                metadata.SetValueOverride(this, value);
            }
            else
            {
                IDictionary<DependencyProperty, object> metaProperties = null;
                if (dependencyProperty.DefaultMetadata.IsMetaProperty)
                {
                    metaProperties = this.metaProperties;
                }
                else
                {
                    metaProperties = this.DependencyPropertyValues;
                }
                object obj2 = null;
                if ((this.metaProperties != null) && this.metaProperties.ContainsKey(dependencyProperty))
                {
                    obj2 = this.metaProperties[dependencyProperty];
                    if (this.DesignMode)
                    {
                        this.metaProperties.Remove(dependencyProperty);
                    }
                }
                if (!this.DesignMode && (obj2 is ActivityBind))
                {
                    this.SetBoundValue((ActivityBind) obj2, value);
                }
                else if (metaProperties.ContainsKey(dependencyProperty))
                {
                    metaProperties[dependencyProperty] = value;
                }
                else
                {
                    metaProperties.Add(dependencyProperty, value);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        T[] IDependencyObjectAccessor.GetInvocationList<T>(DependencyProperty dependencyEvent)
        {
            return this.GetInvocationList<T>(dependencyEvent);
        }

        void IDependencyObjectAccessor.InitializeActivatingInstanceForRuntime(DependencyObject parentDependencyObject, IWorkflowCoreRuntime workflowCoreRuntime)
        {
            if (parentDependencyObject != null)
            {
                this.DependencyPropertyValues[ParentDependencyObjectProperty] = parentDependencyObject;
            }
            foreach (DependencyProperty property in this.MetaDependencyProperties)
            {
                object obj2 = this.metaProperties[property];
                if (obj2 is DependencyObject)
                {
                    ((IDependencyObjectAccessor) obj2).InitializeActivatingInstanceForRuntime(this, workflowCoreRuntime);
                    this.DependencyPropertyValues[property] = obj2;
                }
                else if (obj2 is WorkflowParameterBindingCollection)
                {
                    IList list = obj2 as IList;
                    for (int i = 0; i < list.Count; i++)
                    {
                        ((IDependencyObjectAccessor) list[i]).InitializeActivatingInstanceForRuntime(this, workflowCoreRuntime);
                    }
                    this.DependencyPropertyValues[property] = obj2;
                }
            }
            this.OnInitializeActivatingInstanceForRuntime(workflowCoreRuntime);
            this.Readonly = true;
        }

        void IDependencyObjectAccessor.InitializeDefinitionForRuntime(DependencyObject parentDependencyObject)
        {
            if (parentDependencyObject != null)
            {
                this.DependencyPropertyValues[ParentDependencyObjectProperty] = parentDependencyObject;
            }
            foreach (DependencyProperty property in this.MetaDependencyProperties)
            {
                object obj2 = this.metaProperties[property];
                if (obj2 is DependencyObject)
                {
                    ((IDependencyObjectAccessor) obj2).InitializeDefinitionForRuntime(this);
                    this.DependencyPropertyValues[property] = obj2;
                }
                else if (obj2 is WorkflowParameterBindingCollection)
                {
                    IList list = obj2 as IList;
                    for (int i = 0; i < list.Count; i++)
                    {
                        ((IDependencyObjectAccessor) list[i]).InitializeDefinitionForRuntime(this);
                    }
                    this.DependencyPropertyValues[property] = obj2;
                }
                else if (obj2 is ActivityBind)
                {
                    Activity activity = this.ResolveOwnerActivity();
                    if (activity != null)
                    {
                        ((ActivityBind) obj2).SetContext(activity);
                    }
                }
            }
            this.OnInitializeDefinitionForRuntime();
            this.InitializeProperties();
            this.Readonly = true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDependencyObjectAccessor.InitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            this.OnInitializeInstanceForRuntime(workflowCoreRuntime);
        }

        internal IDictionary<DependencyProperty, object> DependencyPropertyValues
        {
            get
            {
                if (this.dependencyPropertyValues == null)
                {
                    this.dependencyPropertyValues = new Dictionary<DependencyProperty, object>();
                }
                return this.dependencyPropertyValues;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected internal bool DesignMode
        {
            get
            {
                return !this.readonlyPropertyValue;
            }
        }

        internal IList<DependencyProperty> MetaDependencyProperties
        {
            get
            {
                return new List<DependencyProperty>(this.metaProperties.Keys).AsReadOnly();
            }
        }

        protected DependencyObject ParentDependencyObject
        {
            get
            {
                return (DependencyObject) this.GetValue(ParentDependencyObjectProperty);
            }
        }

        internal bool Readonly
        {
            get
            {
                return (bool) this.GetValue(ReadonlyProperty);
            }
            set
            {
                this.SetReadOnlyPropertyValue(ReadonlyProperty, value);
                this.readonlyPropertyValue = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public ISite Site
        {
            get
            {
                return (ISite) this.GetValue(SiteProperty);
            }
            set
            {
                this.SetValue(SiteProperty, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDictionary UserData
        {
            get
            {
                return (IDictionary) this.GetValue(UserDataProperty);
            }
        }
    }
}

