namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(object))]
    public sealed class PropertyReferenceExtension<T> : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!string.IsNullOrEmpty(this.PropertyName))
            {
                object rootTemplatedActivity = ActivityWithResultConverter.GetRootTemplatedActivity(serviceProvider);
                if (rootTemplatedActivity != null)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(rootTemplatedActivity)[this.PropertyName];
                    if (descriptor != null)
                    {
                        return descriptor.GetValue(rootTemplatedActivity);
                    }
                }
            }
            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.PropertyReferenceNotFound(this.PropertyName)));
        }

        public string PropertyName { get; set; }
    }
}

