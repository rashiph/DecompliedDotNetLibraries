namespace System.Xaml.Replacements
{
    using System;
    using System.ComponentModel;
    using System.Windows.Markup;

    internal class DateTimeValueSerializerContext : IValueSerializerContext, ITypeDescriptorContext, IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return null;
        }

        public ValueSerializer GetValueSerializerFor(System.ComponentModel.PropertyDescriptor descriptor)
        {
            return null;
        }

        public ValueSerializer GetValueSerializerFor(Type type)
        {
            return null;
        }

        public void OnComponentChanged()
        {
        }

        public bool OnComponentChanging()
        {
            return false;
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
            get
            {
                return null;
            }
        }

        public System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return null;
            }
        }
    }
}

