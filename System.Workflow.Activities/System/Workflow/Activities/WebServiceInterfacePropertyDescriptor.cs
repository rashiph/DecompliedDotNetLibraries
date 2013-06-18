namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    internal sealed class WebServiceInterfacePropertyDescriptor : System.Workflow.Activities.Common.DynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WebServiceInterfacePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor pd) : base(serviceProvider, pd)
        {
        }

        public override void SetValue(object component, object value)
        {
            string str = value as string;
            if ((str != null) && (str.Length > 0))
            {
                ITypeProvider service = (ITypeProvider) base.ServiceProvider.GetService(typeof(ITypeProvider));
                if (service == null)
                {
                    throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                Type type = service.GetType(value as string);
                if (type == null)
                {
                    throw new Exception(SR.GetString("Error_TypeNotResolved", new object[] { value }));
                }
                TypeFilterProviderAttribute attribute = this.Attributes[typeof(TypeFilterProviderAttribute)] as TypeFilterProviderAttribute;
                if (attribute != null)
                {
                    ITypeFilterProvider provider2 = null;
                    Type type2 = Type.GetType(attribute.TypeFilterProviderTypeName);
                    if (type2 != null)
                    {
                        provider2 = Activator.CreateInstance(type2, new object[] { base.ServiceProvider }) as ITypeFilterProvider;
                    }
                    if (provider2 != null)
                    {
                        provider2.CanFilterType(type, true);
                    }
                }
                value = type.AssemblyQualifiedName;
            }
            base.RealPropertyDescriptor.SetValue(component, value);
        }
    }
}

