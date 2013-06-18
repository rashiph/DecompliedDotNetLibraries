namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;

    public sealed class WorkflowDataContext : CustomTypeDescriptor, INotifyPropertyChanged, IDisposable
    {
        private System.Activities.ActivityInstance activityInstance;
        private ActivityContext cachedResolutionContext;
        private ActivityExecutor executor;
        private IDictionary<Location, PropertyDescriptorImpl> locationMapping;
        private PropertyDescriptorCollection properties;
        private System.ComponentModel.PropertyChangedEventHandler propertyChangedEventHandler;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal WorkflowDataContext(ActivityExecutor executor, System.Activities.ActivityInstance activityInstance)
        {
            this.executor = executor;
            this.activityInstance = activityInstance;
            this.properties = this.CreateProperties();
        }

        private void AddNotifyHandler(PropertyDescriptorImpl property)
        {
            using (ActivityContext context = this.ResolutionContext)
            {
                Location key = property.LocationReference.GetLocation(context);
                INotifyPropertyChanged changed = key as INotifyPropertyChanged;
                if (changed != null)
                {
                    changed.PropertyChanged += this.PropertyChangedEventHandler;
                    if (this.locationMapping == null)
                    {
                        this.locationMapping = new Dictionary<Location, PropertyDescriptorImpl>();
                    }
                    this.locationMapping.Add(key, property);
                }
            }
        }

        private void AddProperty(LocationReference reference, Dictionary<string, object> names, List<PropertyDescriptorImpl> propertyList)
        {
            if (!string.IsNullOrEmpty(reference.Name) && !names.ContainsKey(reference.Name))
            {
                names.Add(reference.Name, reference);
                PropertyDescriptorImpl item = new PropertyDescriptorImpl(reference);
                propertyList.Add(item);
                this.AddNotifyHandler(item);
            }
        }

        private PropertyDescriptorCollection CreateProperties()
        {
            Dictionary<string, object> names = new Dictionary<string, object>();
            List<PropertyDescriptorImpl> propertyList = new List<PropertyDescriptorImpl>();
            for (LocationReferenceEnvironment environment = this.activityInstance.Activity.PublicEnvironment; environment != null; environment = environment.Parent)
            {
                foreach (LocationReference reference in environment.GetLocationReferences())
                {
                    this.AddProperty(reference, names, propertyList);
                }
            }
            return new PropertyDescriptorCollection(propertyList.ToArray(), true);
        }

        public void Dispose()
        {
            if (this.locationMapping != null)
            {
                foreach (KeyValuePair<Location, PropertyDescriptorImpl> pair in this.locationMapping)
                {
                    INotifyPropertyChanged changed = pair.Value as INotifyPropertyChanged;
                    if (changed != null)
                    {
                        changed.PropertyChanged -= this.PropertyChangedEventHandler;
                    }
                }
            }
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return this.properties;
        }

        private void OnLocationChanged(object sender, PropertyChangedEventArgs e)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                PropertyDescriptorImpl impl;
                Location key = (Location) sender;
                if (this.locationMapping.TryGetValue(key, out impl))
                {
                    if (e.PropertyName == "Value")
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(impl.Name));
                    }
                    else
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(impl.Name + "." + e.PropertyName));
                    }
                }
            }
        }

        private System.ComponentModel.PropertyChangedEventHandler PropertyChangedEventHandler
        {
            get
            {
                if (this.propertyChangedEventHandler == null)
                {
                    this.propertyChangedEventHandler = new System.ComponentModel.PropertyChangedEventHandler(this.OnLocationChanged);
                }
                return this.propertyChangedEventHandler;
            }
        }

        private ActivityContext ResolutionContext
        {
            get
            {
                if (this.cachedResolutionContext == null)
                {
                    this.cachedResolutionContext = new ActivityContext(this.activityInstance, this.executor);
                    this.cachedResolutionContext.AllowChainedEnvironmentAccess = true;
                }
                else
                {
                    this.cachedResolutionContext.Reinitialize(this.activityInstance, this.executor);
                }
                return this.cachedResolutionContext;
            }
        }

        private class PropertyDescriptorImpl : PropertyDescriptor
        {
            private System.Activities.LocationReference reference;

            public PropertyDescriptorImpl(System.Activities.LocationReference reference) : base(reference.Name, new Attribute[0])
            {
                this.reference = reference;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                WorkflowDataContext context = (WorkflowDataContext) component;
                using (ActivityContext context2 = context.ResolutionContext)
                {
                    return this.reference.GetLocation(context2).Value;
                }
            }

            public override void ResetValue(object component)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.CannotResetPropertyInDataContext));
            }

            public override void SetValue(object component, object value)
            {
                if (this.IsReadOnly)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.PropertyReadOnlyInWorkflowDataContext(this.Name)));
                }
                WorkflowDataContext context = (WorkflowDataContext) component;
                using (ActivityContext context2 = context.ResolutionContext)
                {
                    this.reference.GetLocation(context2).Value = value;
                }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }

            public override Type ComponentType
            {
                get
                {
                    return typeof(WorkflowDataContext);
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public System.Activities.LocationReference LocationReference
            {
                get
                {
                    return this.reference;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this.reference.Type;
                }
            }
        }
    }
}

