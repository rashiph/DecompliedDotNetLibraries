namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.ObjectModel;

    public interface IDesignerGlyphProviderService
    {
        void AddGlyphProvider(IDesignerGlyphProvider glyphProvider);
        void RemoveGlyphProvider(IDesignerGlyphProvider glyphProvider);

        ReadOnlyCollection<IDesignerGlyphProvider> GlyphProviders { get; }
    }
}

