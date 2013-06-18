namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.XamlIntegration;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;

    [ContentProperty("Implementation")]
    public sealed class DynamicActivity : Activity, ICustomTypeDescriptor, IDynamicActivity
    {
        private Collection<Attribute> attributes;
        private Activity runtimeImplementation;
        private DynamicActivityTypeDescriptor typeDescriptor;

        public DynamicActivity()
        {
            this.typeDescriptor = new DynamicActivityTypeDescriptor(this);
        }

        internal override void InternalExecute(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            if (this.runtimeImplementation != null)
            {
                executor.ScheduleActivity(this.runtimeImplementation, instance, null, null, null);
            }
        }

        internal sealed override void OnInternalCacheMetadata(bool createEmptyBindings)
        {
            Activity activity = null;
            if (this.Implementation != null)
            {
                activity = this.Implementation();
            }
            if (activity != null)
            {
                base.SetImplementationChildrenCollection(new Collection<Activity> { activity });
            }
            this.runtimeImplementation = activity;
            Activity.ReflectedInformation information = new Activity.ReflectedInformation(this);
            base.SetImportedChildrenCollection(information.GetChildren());
            base.SetVariablesCollection(information.GetVariables());
            base.SetImportedDelegatesCollection(information.GetDelegates());
            base.SetArgumentsCollection(information.GetArguments(), createEmptyBindings);
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return this.typeDescriptor.GetAttributes();
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return this.typeDescriptor.GetClassName();
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return this.typeDescriptor.GetComponentName();
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return this.typeDescriptor.GetConverter();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return this.typeDescriptor.GetDefaultEvent();
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return this.typeDescriptor.GetDefaultProperty();
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return this.typeDescriptor.GetEditor(editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return this.typeDescriptor.GetEvents();
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return this.typeDescriptor.GetEvents(attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.typeDescriptor.GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.typeDescriptor.GetProperties(attributes);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.typeDescriptor.GetPropertyOwner(pd);
        }

        [DependsOn("Name")]
        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new Collection<Attribute>();
                }
                return this.attributes;
            }
        }

        [DependsOn("Properties")]
        public Collection<Constraint> Constraints
        {
            get
            {
                return base.Constraints;
            }
        }

        [Ambient, DefaultValue((string) null), XamlDeferLoad(typeof(FuncDeferringLoader), typeof(Activity)), Browsable(false)]
        public Func<Activity> Implementation
        {
            get
            {
                return base.Implementation;
            }
            set
            {
                base.Implementation = value;
            }
        }

        public string Name
        {
            get
            {
                return this.typeDescriptor.Name;
            }
            set
            {
                this.typeDescriptor.Name = value;
            }
        }

        [DependsOn("Attributes"), Browsable(false)]
        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get
            {
                return this.typeDescriptor.Properties;
            }
        }

        KeyedCollection<string, DynamicActivityProperty> IDynamicActivity.Properties
        {
            get
            {
                return this.Properties;
            }
        }
    }
}

