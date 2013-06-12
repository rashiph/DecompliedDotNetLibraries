namespace System.ComponentModel
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ITypeDescriptorContext : IServiceProvider
    {
        void OnComponentChanged();
        bool OnComponentChanging();

        IContainer Container { get; }

        object Instance { get; }

        System.ComponentModel.PropertyDescriptor PropertyDescriptor { get; }
    }
}

