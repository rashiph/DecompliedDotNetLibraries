namespace System.ComponentModel
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IContainer : IDisposable
    {
        void Add(IComponent component);
        void Add(IComponent component, string name);
        void Remove(IComponent component);

        ComponentCollection Components { get; }
    }
}

