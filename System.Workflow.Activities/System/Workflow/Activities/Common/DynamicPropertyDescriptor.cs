namespace System.Workflow.Activities.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime;

    internal class DynamicPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor realPropertyDescriptor;
        private IServiceProvider serviceProvider;

        public DynamicPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor) : base(descriptor, null)
        {
            this.serviceProvider = serviceProvider;
            this.realPropertyDescriptor = descriptor;
        }

        public override bool CanResetValue(object component)
        {
            return this.realPropertyDescriptor.CanResetValue(component);
        }

        public override object GetValue(object component)
        {
            if (component == null)
            {
                return null;
            }
            return this.realPropertyDescriptor.GetValue(component);
        }

        public override void ResetValue(object component)
        {
            this.realPropertyDescriptor.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            if (component is IComponent)
            {
                this.realPropertyDescriptor.SetValue(component, value);
            }
            else
            {
                PropertyDescriptorUtils.SetPropertyValue(this.ServiceProvider, this.realPropertyDescriptor, component, value);
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            return (string.Equals(this.realPropertyDescriptor.GetType().FullName, "System.ComponentModel.Design.InheritedPropertyDescriptor", StringComparison.Ordinal) || this.realPropertyDescriptor.ShouldSerializeValue(component));
        }

        public override AttributeCollection Attributes
        {
            get
            {
                ArrayList list = new ArrayList();
                list.AddRange(this.realPropertyDescriptor.Attributes);
                list.Add(new MergablePropertyAttribute(false));
                return new AttributeCollection((Attribute[]) list.ToArray(typeof(Attribute)));
            }
        }

        public override string Category
        {
            get
            {
                return this.realPropertyDescriptor.Category;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return this.realPropertyDescriptor.ComponentType;
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return this.realPropertyDescriptor.Converter;
            }
        }

        public override string Description
        {
            get
            {
                return this.realPropertyDescriptor.Description;
            }
        }

        public override string DisplayName
        {
            get
            {
                return this.realPropertyDescriptor.DisplayName;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.realPropertyDescriptor.IsReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.realPropertyDescriptor.PropertyType;
            }
        }

        public PropertyDescriptor RealPropertyDescriptor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.realPropertyDescriptor;
            }
        }

        public IServiceProvider ServiceProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceProvider;
            }
        }
    }
}

