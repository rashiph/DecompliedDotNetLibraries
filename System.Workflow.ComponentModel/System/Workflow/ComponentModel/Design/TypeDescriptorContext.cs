namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime;

    internal sealed class TypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
    {
        private object instance;
        private System.ComponentModel.PropertyDescriptor propDesc;
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TypeDescriptorContext(IServiceProvider serviceProvider, System.ComponentModel.PropertyDescriptor propDesc, object instance)
        {
            this.serviceProvider = serviceProvider;
            this.propDesc = propDesc;
            this.instance = instance;
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public void OnComponentChanged()
        {
            IComponentChangeService service = (IComponentChangeService) this.serviceProvider.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.OnComponentChanged(this.instance, this.propDesc, null, null);
            }
        }

        public bool OnComponentChanging()
        {
            IComponentChangeService service = (IComponentChangeService) this.serviceProvider.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                try
                {
                    service.OnComponentChanging(this.instance, this.propDesc);
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw exception;
                    }
                    return false;
                }
            }
            return true;
        }

        public IContainer Container
        {
            get
            {
                return (IContainer) this.serviceProvider.GetService(typeof(IContainer));
            }
        }

        public object Instance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.instance;
            }
        }

        public System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propDesc;
            }
        }
    }
}

