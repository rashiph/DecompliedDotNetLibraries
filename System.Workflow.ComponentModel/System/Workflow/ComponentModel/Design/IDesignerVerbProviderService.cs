namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.ObjectModel;

    public interface IDesignerVerbProviderService
    {
        void AddVerbProvider(IDesignerVerbProvider verbProvider);
        void RemoveVerbProvider(IDesignerVerbProvider verbProvider);

        ReadOnlyCollection<IDesignerVerbProvider> VerbProviders { get; }
    }
}

