namespace System.ComponentModel
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISite : IServiceProvider
    {
        IComponent Component { get; }

        IContainer Container { get; }

        bool DesignMode { get; }

        string Name { get; set; }
    }
}

