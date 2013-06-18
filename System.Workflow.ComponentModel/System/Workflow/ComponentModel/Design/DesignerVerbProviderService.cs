namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class DesignerVerbProviderService : IDesignerVerbProviderService
    {
        private List<IDesignerVerbProvider> designerVerbProviders = new List<IDesignerVerbProvider>();

        public DesignerVerbProviderService()
        {
            ((IDesignerVerbProviderService) this).AddVerbProvider(new FreeFormDesignerVerbProvider());
        }

        void IDesignerVerbProviderService.AddVerbProvider(IDesignerVerbProvider verbProvider)
        {
            if (!this.designerVerbProviders.Contains(verbProvider))
            {
                this.designerVerbProviders.Add(verbProvider);
            }
        }

        void IDesignerVerbProviderService.RemoveVerbProvider(IDesignerVerbProvider verbProvider)
        {
            this.designerVerbProviders.Remove(verbProvider);
        }

        ReadOnlyCollection<IDesignerVerbProvider> IDesignerVerbProviderService.VerbProviders
        {
            get
            {
                return this.designerVerbProviders.AsReadOnly();
            }
        }
    }
}

