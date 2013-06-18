namespace System.Workflow.Activities.Common
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal class TypePropertyDescriptor : System.Workflow.Activities.Common.DynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TypePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor actualPropDesc) : base(serviceProvider, actualPropDesc)
        {
        }

        public override object GetValue(object component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            object designTimeTypeName = base.GetValue(component);
            if (designTimeTypeName == null)
            {
                DependencyObject owner = component as DependencyObject;
                if (owner != null)
                {
                    object key = DependencyProperty.FromName(base.RealPropertyDescriptor.Name, base.RealPropertyDescriptor.ComponentType);
                    designTimeTypeName = System.Workflow.Activities.Common.Helpers.GetDesignTimeTypeName(owner, key);
                    if (string.IsNullOrEmpty(designTimeTypeName as string))
                    {
                        key = base.RealPropertyDescriptor.ComponentType.FullName + "." + base.RealPropertyDescriptor.Name;
                        designTimeTypeName = System.Workflow.Activities.Common.Helpers.GetDesignTimeTypeName(owner, key);
                    }
                }
            }
            return designTimeTypeName;
        }

        public override void SetValue(object component, object value)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (value != null)
            {
                Type type = value as Type;
                ITypeFilterProvider provider = System.Workflow.Activities.Common.PropertyDescriptorUtils.GetComponent(new System.Workflow.Activities.Common.TypeDescriptorContext(base.ServiceProvider, base.RealPropertyDescriptor, component)) as ITypeFilterProvider;
                if (provider != null)
                {
                    provider.CanFilterType(type, true);
                }
            }
            base.SetValue(component, value);
        }

        public override TypeConverter Converter
        {
            get
            {
                TypeConverter converter = base.Converter;
                string fullName = converter.GetType().FullName;
                Type c = Assembly.GetExecutingAssembly().GetType(fullName);
                if ((c != null) && typeof(System.Workflow.Activities.Common.TypePropertyTypeConverter).IsAssignableFrom(c))
                {
                    return converter;
                }
                return new System.Workflow.Activities.Common.TypePropertyTypeConverter();
            }
        }
    }
}

