namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;

    internal sealed class ComponentChangeDispatcher : IDisposable
    {
        private object component;
        private object newValue;
        private object oldValue;
        private PropertyDescriptor property;
        private IServiceProvider serviceProvider;

        public ComponentChangeDispatcher(IServiceProvider serviceProvider, object component, PropertyDescriptor propertyDescriptor)
        {
            this.serviceProvider = serviceProvider;
            this.component = component;
            this.property = propertyDescriptor;
            IComponentChangeService service = serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                try
                {
                    this.newValue = this.oldValue = propertyDescriptor.GetValue(component);
                    propertyDescriptor.AddValueChanged(component, new EventHandler(this.OnValueChanged));
                    service.OnComponentChanging(component, propertyDescriptor);
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw exception;
                    }
                }
            }
        }

        public void Dispose()
        {
            IComponentChangeService service = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                service.OnComponentChanged(this.component, this.property, this.oldValue, this.newValue);
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            this.newValue = this.property.GetValue(this.component);
            this.property.RemoveValueChanged(this.component, new EventHandler(this.OnValueChanged));
        }
    }
}

