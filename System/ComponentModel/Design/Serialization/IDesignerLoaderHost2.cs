namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.ComponentModel.Design;

    public interface IDesignerLoaderHost2 : IDesignerLoaderHost, IDesignerHost, IServiceContainer, IServiceProvider
    {
        bool CanReloadWithErrors { get; set; }

        bool IgnoreErrorsDuringReload { get; set; }
    }
}

