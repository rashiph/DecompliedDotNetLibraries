namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public interface IBindableComponent : IComponent, IDisposable
    {
        System.Windows.Forms.BindingContext BindingContext { get; set; }

        ControlBindingsCollection DataBindings { get; }
    }
}

