namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    internal class DummyTypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
    {
        private object component;
        private System.ComponentModel.PropertyDescriptor propDescriptor;
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DummyTypeDescriptorContext(IServiceProvider serviceProvider, object component, System.ComponentModel.PropertyDescriptor propDescriptor)
        {
            this.serviceProvider = serviceProvider;
            this.propDescriptor = propDescriptor;
            this.component = component;
        }

        public object GetService(Type serviceType)
        {
            if (this.serviceProvider != null)
            {
                return this.serviceProvider.GetService(serviceType);
            }
            return null;
        }

        public void OnComponentChanged()
        {
        }

        public bool OnComponentChanging()
        {
            return true;
        }

        public IContainer Container
        {
            get
            {
                return null;
            }
        }

        public object Instance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.component;
            }
        }

        public System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propDescriptor;
            }
        }
    }
}

